namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounts;
    using AutoFixture.Xunit2;
    using Xunit;

    public sealed class AccountRepositoryTest : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), "__accounts.json");
        private readonly AccountRepository _repository;

        public AccountRepositoryTest()
        {
            _repository = new AccountRepository(_filePath);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new AccountRepository(null));
        }

        [Theory, AutoData]
        public void AddExistingThrows(Account account)
        {
            _repository.Add(account);
            Assert.Throws<ArgumentException>(() => _repository.Add(account));
        }

        [Theory, AutoData]
        public async Task ValidatePasswordOfNonExistingUserIdThrows(Account account)
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.ValidatePassword(account.Id, "password"));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(Account account)
        {
            _repository.Add(account);
            var actual = _repository.Get(account.Id).Value;
            Assert.Equal(account.Id, actual.Id);
        }

        [Theory, AutoData]
        public void AddAndGetByTokenIsOk(Account account)
        {
            _repository.Add(account);
            var actual = _repository.GetByToken(account.Token).Value;
            Assert.Equal(account.Id, actual.Id);
        }

        [Theory, AutoData]
        public void AddAndGetByEmailIsOk(Account account)
        {
            _repository.Add(account);
            var actual = _repository.GetByEmail(account.Email).Value;
            Assert.Equal(account.Id, actual.Id);
        }

        [Theory, AutoData]
        public void ContainsIsOk(Account account)
        {
            _repository.Add(account);
            Assert.True(_repository.Contains(account.Id));
        }

        [Theory, AutoData]
        public async Task ValidatePasswordIsOk(Account account)
        {
            account.SetPassword("password");
            _repository.Add(account);
            Assert.True(await _repository.ValidatePassword(account.Id, "password"));
        }

        [Theory, AutoData]
        public async Task ValidateWrongPasswordReturnsFalse(Account account)
        {
            account.SetPassword("password");
            _repository.Add(account);
            Assert.False(await _repository.ValidatePassword(account.Id, "wrongPassword"));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_repository.Contains("NonExistingAccount"));
        }

        [Theory, AutoData]
        public void CountIsOk(Account account)
        {
            _repository.Add(account);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoData]
        public void GetAllIsOk(Account account)
        {
            _repository.Add(account);
            Assert.Single(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetIdsIsOk(Account account)
        {
            _repository.Add(account);
            Assert.Equal(account.Id, _repository.GetIds().First());
        }

        [Theory, AutoData]
        public void RemoveIsOk(Account account)
        {
            _repository.Add(account);
            _repository.Remove(account.Id);
            Assert.False(_repository.Contains(account.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void RemoveUsingPredicateIsOk(Account account)
        {
            _repository.Add(account);
            _repository.Remove(e => e.Id == account.Id);
            Assert.False(_repository.Contains(account.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void UpdateIsOk(Account account)
        {
            _repository.Add(account);
            var updatedAccount = new Account(account.Id, "Updated name");
            _repository.Update(updatedAccount);
            Assert.Equal("Updated name", _repository.Get(account.Id).Value.Name);
        }

        [Fact]
        public void CaseInsensitiveComparerIsOk()
        {
            var repository = new JsonRepository<FakeEntity, string>(_filePath, comparer: StringComparer.InvariantCultureIgnoreCase);
            repository.Add(new FakeEntity("MyEntity", "My Entity"));
            Assert.True(repository.Contains("myentity"));
        }

        [Theory, AutoData]
        public void GetAllReturnsClones(Account account)
        {
            _repository.Add(account);
            foreach (var e in _repository.GetAll())
            {
                e.Metadata.Add("Description", "A description");
            }

            Assert.Empty(_repository.Get(account.Id).Value.Metadata);
        }

        [Fact]
        public void GetAllForEmptyRepositoryIsOk()
        {
            Assert.Empty(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetReturnsClone(Account account)
        {
            _repository.Add(account);
            var e = _repository.Get(account.Id).Value;
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(account.Id).Value.Metadata);
        }

        [Theory, AutoData]
        public void GetByPreidcateReturnsClones(Account account)
        {
            _repository.Add(account);
            var e = _repository.Get(ent => ent.Id == account.Id).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(account.Id).Value.Metadata);
        }
    }
}