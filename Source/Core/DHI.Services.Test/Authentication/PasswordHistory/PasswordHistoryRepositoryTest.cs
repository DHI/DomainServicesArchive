namespace DHI.Services.Test.Authentication.PasswordHistory
{
    using AutoFixture.Xunit2;
    using DHI.Services.Accounts;
    using DHI.Services.Authentication.PasswordHistory;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class PasswordHistoryRepositoryTest : IDisposable
    {

        private readonly string _filePath = Path.Combine(Path.GetTempPath(), "__passwordhistory.json");
        private readonly PasswordHistoryRepository _repository;

        public PasswordHistoryRepositoryTest()
        {
            _repository = new PasswordHistoryRepository(_filePath);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordHistoryRepository(null));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(PasswordHistory passwordHistory)
        {
            _repository.Add(passwordHistory);
            var actual = _repository.Get(passwordHistory.Id).Value;
            Assert.Equal(passwordHistory.Id, actual.Id);
        }

        [Fact]
        public async Task GetMostRecentByAccountIdAsyncTest()
        {
            var passwordHistory = new PasswordHistory("john.doe")
            {
                AccountId = "john.doe",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("password")
            };
            _repository.Add(passwordHistory);

            var passwordHistoryB = new PasswordHistory("john.doe.b")
            {
                AccountId = "john.doe",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("passwordB")
            };
            _repository.Add(passwordHistoryB);

            var getPasswordHistory = await _repository.GetMostRecentByAccountId("john.doe");

            

            Assert.Equal(passwordHistoryB.AccountId, getPasswordHistory.AccountId);
        }

        [Fact]
        public async Task GetRecentByAccountIdAsyncTest()
        {
            var passwordExpirationPolicy = new PasswordExpirationPolicy()
            {
                PreviousPasswordsReUseLimit = 2
            };

            var passwordHistory = new PasswordHistory("john.doer")
            {
                AccountId = "john.doer",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("password")
            };
            _repository.Add(passwordHistory);

            var passwordHistoryB = new PasswordHistory("john.doer.b")
            {
                AccountId = "john.doer",
                Added = DateTime.Now,
                PasswordExpiryDate = DateTime.Now,
                EncryptedPassword = Account.HashPasswordStrong("passwordB")
            };
            _repository.Add(passwordHistoryB);

            var getPasswordHistoryList = await _repository.GetRecentByAccountId("john.doer", passwordExpirationPolicy.PreviousPasswordsReUseLimit);

            Assert.Equal(2, getPasswordHistoryList.Count());
        }

    }
}
