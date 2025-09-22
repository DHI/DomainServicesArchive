namespace DHI.Services.Test.Authentication.PasswordHistory
{
    using DHI.Services.Accounts;
    using DHI.Services.Authentication.PasswordHistory;
    using DHI.Services.Test.Notifications;
    using Microsoft.Extensions.Logging.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class PasswordHistoryServiceTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public PasswordHistoryServiceTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void GetRepositoryTypes()
        {
            var repositoryTypes = PasswordHistoryService.GetRepositoryTypes();

            Assert.Contains(typeof(PasswordHistoryRepository), repositoryTypes);
        }

        [Theory, AutoPasswordHistoryData]
        public void GetNonExistingThrows(PasswordHistoryService passwordHistoryService)
        {
            Assert.Throws<KeyNotFoundException>(() => passwordHistoryService.Get("UnknownId"));
        }

        [Theory, AutoPasswordHistoryData]
        public void AddAndGetPasswordHistory(PasswordHistoryService passwordHistoryService, PasswordHistory passwordHistory)
        {
            passwordHistoryService.Add(passwordHistory);

            Assert.Equal(passwordHistory.Id, passwordHistoryService.Get(passwordHistory.Id).Id);
        }

        [Theory, AutoPasswordHistoryData]
        public async Task GetSpecificPasswordHistory(IPasswordHistoryRepository passwordHistoryRepository, IAccountRepository accountRepository)
        {
            // Arrange
            var fakeLogger = new TestLoggerMicrosoft(_testOutputHelper);
            var passwordExpirationPolicy = new PasswordExpirationPolicy()
            {
                PreviousPasswordsReUseLimit = 2,
                PasswordExpiryDurationInDays = 2
            };
            var accountService = new AccountService(accountRepository);
            var passwordHisttoryService = new PasswordHistoryService(passwordHistoryRepository, fakeLogger, accountRepository, passwordExpirationPolicy);

            // Mock the data
            var accountData = new Account("admin", "admin");
            accountData.SetPassword("webapi");
            accountService.Add(accountData);

            // Act
            var passwordHistory = await passwordHisttoryService.AddPasswordHistoryAsync(accountData, "webapi", DateTime.Now);
            var getPasswordHistory = await passwordHisttoryService.GetCurrentPasswordHistoryAsync(accountData.Id, "webapi");

            // Assert
            Assert.Equal(passwordHistory.Id, getPasswordHistory.Id);
        }

        [Theory, AutoPasswordHistoryData]
        public async Task AddAndGetPasswordHistoryWithAccountData(IPasswordHistoryRepository passwordHistoryRepository, IAccountRepository accountRepository)
        {
            // Arrange
            var fakeLogger = new TestLoggerMicrosoft(_testOutputHelper);
            var passwordExpirationPolicy = new PasswordExpirationPolicy()
            {
                PreviousPasswordsReUseLimit = 2,
                PasswordExpiryDurationInDays = 2
            };
            var accountService = new AccountService(accountRepository);
            var passwordHisttoryService = new PasswordHistoryService(passwordHistoryRepository, fakeLogger, accountRepository, passwordExpirationPolicy);

            // Mock the data
            var accountData = new Account("john.doe", "john.doe");
            accountData.SetPassword("password");
            accountService.Add(accountData);

            // Act
            var passwordHistory = await passwordHisttoryService.AddPasswordHistoryAsync(accountData, "password", DateTime.Now, null);
            var getPasswordHistoryList = await passwordHisttoryService.IsPasswordAlreadyUsedAsync(accountData.Id, "password");

            // Assert
            Assert.NotNull(passwordHistory.Id);
        }

        [Theory, AutoPasswordHistoryData]
        public async Task UpdateAccountWithPasswordPolicyValidation(IPasswordHistoryRepository passwordHistoryRepository, IAccountRepository accountRepository, string clientIp)
        {
            // Arrange
            var fakeLogger = new TestLoggerMicrosoft(_testOutputHelper);
            var passwordExpirationPolicy = new PasswordExpirationPolicy()
            {
                PreviousPasswordsReUseLimit = 2,
                PasswordExpiryDurationInDays = 1
            };
            var accountService = new AccountService(accountRepository);
            var passwordHistoryService = new PasswordHistoryService(passwordHistoryRepository, fakeLogger, accountRepository, passwordExpirationPolicy);

            // Mock the data - start with account having passwordA
            var accountData = new Account("john.doe", "john.doe");
            accountData.SetPassword("passwordA");
            accountService.Add(accountData);

            // Act - Step 1: Add passwordA to history
            _testOutputHelper.WriteLine("Adding passwordA to history");
            var passHistoryA = await passwordHistoryService.AddPasswordHistoryAsync(accountData, "passwordA", DateTime.Now);
            Assert.NotNull(passHistoryA);

            // Step 2: Update to passwordB
            _testOutputHelper.WriteLine("Updating to passwordB");
            accountData.SetPassword("passwordB");
            accountService.UpdateMe(accountData);
            var passHistoryB = await passwordHistoryService.AddPasswordHistoryAsync(accountData, "passwordB", DateTime.Now);
            Assert.NotNull(passHistoryB);
            Assert.NotEqual(passHistoryA.Id, passHistoryB.Id);

            // Step 3: Update to passwordC
            _testOutputHelper.WriteLine("Updating to passwordC");
            accountData.SetPassword("passwordC");
            accountService.UpdateMe(accountData);
            var passHistoryC = await passwordHistoryService.AddPasswordHistoryAsync(accountData, "passwordC", DateTime.Now);
            Assert.NotNull(passHistoryC);
            Assert.NotEqual(passHistoryB.Id, passHistoryC.Id);

            // Step 4: Debug checking the current state
            _testOutputHelper.WriteLine("Checking current password state");
            var currentMatch = await passwordHistoryService.GetCurrentPasswordHistoryAsync(accountData.Id, "passwordC", null);
            Assert.NotNull(currentMatch);
            _testOutputHelper.WriteLine($"Current password is passwordC: {currentMatch != null}");

            var isPasswordBUsed = await passwordHistoryService.IsPasswordAlreadyUsedAsync(accountData.Id, "passwordB", null);
            _testOutputHelper.WriteLine($"Is passwordB already used: {isPasswordBUsed}");
            Assert.True(isPasswordBUsed);

            // Step 5: Try to reuse passwordB - should throw exception
            _testOutputHelper.WriteLine("Trying to reuse passwordB");

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await passwordHistoryService.AddPasswordHistoryAsync(accountData, "passwordB", DateTime.Now)
            );
            Assert.Contains("Cannot use the same password as before", ex.Message);
        }

        [Theory, AutoPasswordHistoryData]
        public async Task ValidatePasswordHistoryDataWithConditionTest(PasswordHistoryService passwordHistoryService)
        {
            var passwordHistory = new PasswordHistory("john.doe")
            {
                AccountId = "john.doe",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("password")
            };
            passwordHistoryService.Add(passwordHistory);

            var passwordHistoryB = new PasswordHistory("john.doe.b")
            {
                AccountId = "john.doe",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("passwordB")
            };
            passwordHistoryService.Add(passwordHistoryB);

            var passwordHistoryC = new PasswordHistory("john.doe.c")
            {
                AccountId = "john.doe",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("passwordC")
            };
            passwordHistoryService.Add(passwordHistoryC);

            var passwordHistoryD = new PasswordHistory("john.doe.d")
            {
                AccountId = "john.doe",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("passwordD")
            };
            passwordHistoryService.Add(passwordHistoryD);

            var checkWithCondition = await passwordHistoryService.IsPasswordAlreadyUsedAsync("john.doe", "passwordB");

            Assert.True(checkWithCondition);
        }

    }
}
