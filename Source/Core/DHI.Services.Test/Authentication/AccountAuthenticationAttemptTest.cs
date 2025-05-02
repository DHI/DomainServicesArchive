namespace DHI.Services.Test.Authentication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounts;
    using DHI.Services.Authentication;
    using DHI.Services.Logging;
    using DHI.Services.Notifications;
    using DHI.Services.Test.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class AccountAuthenticationAttemptTest
    {
        public LoginAttemptPolicy _loginAttemptPolicy;
        private readonly ITestOutputHelper _testOutputHelper;

        public AccountAuthenticationAttemptTest(LoginAttemptPolicy loginAttemptPolicy = null, ITestOutputHelper testOutputHelper = null)// ITestOuputHelper - output for the x.unit test
        {
            _loginAttemptPolicy = loginAttemptPolicy ?? new LoginAttemptPolicy();
            _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }

        // Unit Test Scenario for lock the account when the user tries to log in multiple times
        [Theory, AutoAccountData]
        public async Task LockAccountLoginAttemptsFails(IAccountRepository accountRepository, Account fixtureSampleData, string clientIp)
        {
            var accountService = new AccountService(accountRepository);
            var authenticationService = new AuthenticationService(accountRepository, _loginAttemptPolicy);
            accountService.Add(fixtureSampleData);

            // Arrange
            var numberofloginattempt = 3;
            var loginDatetime = DateTime.Now;
            var userPassword = "password";
            var wrongPassword = "failedPassword";
            var myAccountSampleData = new Account("marco.poloe", "Marco Poloe")
            {
                AllowMePasswordChange = true,
                EncryptedPassword = Account.HashPasswordStrong(userPassword),
                Activated = true,
                LastLoginAttemptedDate = loginDatetime,
                NoOfUnsuccessfulLoginAttempts = 0,
                Locked = false,
                LockedDateEnd = null,
            };
            accountService.Add(myAccountSampleData);

            // Act
            for (int i = 1; i <= numberofloginattempt; i++)
            {
                _ = await authenticationService.Validate(myAccountSampleData.Id, wrongPassword, clientIp);

                Thread.Sleep(5000); // Simulate delay between attempts
            }

            // Assert
            var accountResult = accountService.Get(myAccountSampleData.Id);
            Assert.Equal(myAccountSampleData.Name, accountResult.Name);
            Assert.Equal(3, accountResult.NoOfUnsuccessfulLoginAttempts);
            Assert.True(accountResult.Locked);
            Assert.NotNull(accountResult.LockedDateEnd);
        }

        // Unit Test Scenario for enabled the account when the user tries to log in again after the locked period ends
        [Theory, AutoAccountData]
        public async Task UnlockLoginAttemptsFails(IAccountRepository accountRepository, Account fixtureSampleData, string clientIp)
        {
            var accountService = new AccountService(accountRepository);
            var authenticationService = new AuthenticationService(accountRepository);
            accountService.Add(fixtureSampleData);

            DateTime parsedDate;
            DateTime.TryParseExact("15/11/2023", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate);
            // Arrange
            var loginDatetime = DateTime.Now;
            var userPassword = "password";
            var myAccountSampleData = new Account("marco.poloe", "Marco Poloe")
            {
                AllowMePasswordChange = true,
                EncryptedPassword = Account.HashPasswordStrong(userPassword),
                Activated = true,
                LastLoginAttemptedDate = loginDatetime,
                NoOfUnsuccessfulLoginAttempts = 3,
                Locked = true,
                LockedDateEnd = parsedDate,
            };
            accountService.Add(myAccountSampleData);

            // Act
            await authenticationService.Validate(myAccountSampleData.Id, userPassword, clientIp);

            // Assert
            var accountResult = accountService.Get(myAccountSampleData.Id);
            Assert.Equal(myAccountSampleData.Name, accountResult.Name);
            Assert.Equal(0, accountResult.NoOfUnsuccessfulLoginAttempts);
            Assert.False(accountResult.Locked);
        }
    }
}
