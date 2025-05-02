namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounts;
    using DHI.Services.Authentication;
    using Xunit;

    public class AccountServiceTest
    {
        private const int RepeatCount = 10;

        [Theory, AutoAccountData]
        [Obsolete]
        public void ValidateNonExistingAccountThrows(AccountService accountService)
        {
            Assert.Throws<KeyNotFoundException>(() => accountService.Validate("UnknownAccount", "password"));
        }

        [Theory, AutoAccountData]
        [Obsolete]
        public async Task ValidateWithClientIpNonExistingAccountThrows(AccountService accountService, string clientIp)
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => accountService.Validate("UnknownAccount", "password", clientIp));
        }

        [Theory, AutoAccountData]
        [Obsolete]
        public async Task ValidateWithNullClientIpThrows(AccountService accountService, Account account)
        {
            account.SetPassword("password");
            accountService.Add(account);

            await Assert.ThrowsAsync<ArgumentNullException>(() => accountService.Validate(account.Id, "password", null));
        }

        [Theory, AutoAccountData]
        public void UpdateNonExistingThrows(AccountService accountService, Account account)
        {
            Assert.Throws<KeyNotFoundException>(() => accountService.Update(account));
        }

        [Theory, AutoAccountData]
        public void RemoveNonExistingThrows(AccountService accountService, Account account)
        {
            Assert.Throws<KeyNotFoundException>(() => accountService.Remove(account.Id));
        }

        [Theory, AutoAccountData]
        public void AddAccountWithNoPasswordThrows(AccountService accountService)
        {
            var account = new Account("user", "User");
            Assert.Throws<ArgumentException>(() => accountService.Add(account));
        }

        [Theory, AutoAccountData]
        public void RemoveAdminThrows(AccountService accountService)
        {
            Assert.Throws<ArgumentException>(() => accountService.Remove("admin"));
        }

        [Theory, AutoAccountData(RepeatCount)]
        public void GetAllIsOk(AccountService accountService)
        {
            Assert.Equal(RepeatCount, accountService.GetAll().Count());
        }

        [Theory, AutoAccountData(RepeatCount)]
        public void GetIdsIsOk(AccountService accountService)
        {
            Assert.Equal(RepeatCount, accountService.GetIds().Count());
        }

        [Theory, AutoAccountData]
        public void AddAndGetIsOk(AccountService accountService, Account account)
        {
            accountService.Add(account);
            accountService.TryGet(account.Id, out var myEntity);
            Assert.Equal(account.Id, myEntity.Id);
        }

        [Theory, AutoAccountData(RepeatCount)]
        public void CountIsOk(AccountService accountService)
        {
            Assert.Equal(RepeatCount, accountService.Count());
        }

        [Theory, AutoAccountData(RepeatCount)]
        public void ExistsIsOk(AccountService accountService)
        {
            var account = accountService.GetAll().ToArray()[0];
            Assert.True(accountService.Exists(account.Id));
        }

        [Theory, AutoAccountData(RepeatCount)]
        public void DoesNotExistIsOk(AccountService accountService)
        {
            Assert.False(accountService.Exists("NonExistingAccount"));
        }

        [Theory, AutoAccountData]
        public void EventsAreRaisedOnAdd(AccountService accountService, Account account)
        {
            var raisedEvents = new List<string>();
            accountService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            accountService.Added += (s, e) => { raisedEvents.Add("Added"); };

            accountService.Add(account);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory]
        [InlineData("Guest, User, Editor", new[] { "Guest", "User", "Editor" })]
        [InlineData("Administrator", new[] { "Administrator" })]
        [InlineData("Guest, Editor", new[] { "Guest", "Editor" })]
        [Obsolete]
        public void GetRolesIsOk(string roles, string[] rolesArray)
        {
            var account = new Account("user", "User");
            account.SetPassword("password");
            account.Roles = roles;
            var accountService = new AccountService(new FakeAccountRepository());
            accountService.Add(account);

            Assert.Equal(rolesArray, accountService.GetRoles(account.Id));
        }

        [Theory, AutoAccountData]
        [Obsolete]
        public void ValidateNonActivatedFails(AccountService accountService, Account account)
        {
            account.SetPassword("password");
            accountService.Add(account);
            account.Activated = false;
            accountService.Update(account);

            Assert.False(accountService.Validate(account.Id, "password"));
        }

        [Theory, AutoAccountData]
        [Obsolete]
        public async Task ValidateWithClientIpNonActivatedFails(AccountService accountService, Account account, string clientIp)
        {
            account.SetPassword("password");
            accountService.Add(account);
            account.Activated = false;
            accountService.Update(account);

            Assert.False(await accountService.Validate(account.Id, "password", clientIp));
        }

        [Theory, AutoAccountData]
        public void RemoveIsOk(AccountService accountService, Account account)
        {
            accountService.Add(account);
            accountService.Remove(account.Id);

            Assert.False(accountService.Exists(account.Id));
            Assert.Equal(0, accountService.Count());
        }

        [Theory, AutoAccountData]
        public void EventsAreRaisedOnRemove(AccountService accountService, Account account)
        {
            var raisedEvents = new List<string>();
            accountService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            accountService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            accountService.Add(account);

            accountService.Remove(account.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoAccountData]
        public void UpdateIsOk(AccountService accountService, Account account)
        {
            accountService.Add(account);
            var updatedAccount = new Account(account.Id, "Updated name");
            accountService.Update(updatedAccount);

            accountService.TryGet(account.Id, out var myEntity);
            Assert.Equal(updatedAccount.Name, myEntity.Name);
            Assert.Equal(updatedAccount.EncryptedPassword, account.EncryptedPassword);
            Assert.Equal(updatedAccount.Token, account.Token);
            Assert.Equal(updatedAccount.Activated, account.Activated);
        }

        [Theory, AutoAccountData]
        public void AddOrUpdateIsOk(AccountService accountService, Account account)
        {
            var raisedEvents = new List<string>();
            accountService.Added += (s, e) => { raisedEvents.Add("Added"); };
            accountService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            accountService.AddOrUpdate(account);
            var updated = new Account(account.Id, "Updated name");
            accountService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            accountService.TryGet(account.Id, out var myEntity);
            Assert.Equal(updated.Name, myEntity.Name);
            Assert.Equal(updated.EncryptedPassword, account.EncryptedPassword);
            Assert.Equal(updated.Token, account.Token);
            Assert.Equal(updated.Activated, account.Activated);
        }

        [Theory, AutoAccountData]
        public void TryAddIsOk(AccountService accountService, Account account)
        {
            Assert.True(accountService.TryAdd(account));
            accountService.TryGet(account.Id, out var myEntity);
            Assert.Equal(account.Id, myEntity.Id);
        }

        [Theory, AutoAccountData]
        public void TryAddExistingReturnsFalse(AccountService accountService, Account account)
        {
            accountService.Add(account);
            Assert.False(accountService.TryAdd(account));
        }

        [Theory, AutoAccountData]
        public void TryUpdateIsOk(AccountService accountService, Account account)
        {
            accountService.Add(account);
            var updatedAccount = new Account(account.Id, "Updated name");

            Assert.True(accountService.TryUpdate(updatedAccount));
            accountService.TryGet(account.Id, out var myEntity);
            Assert.Equal(updatedAccount.Name, myEntity.Name);
            Assert.Equal(updatedAccount.EncryptedPassword, account.EncryptedPassword);
            Assert.Equal(updatedAccount.Token, account.Token);
            Assert.Equal(updatedAccount.Activated, account.Activated);
        }

        [Theory, AutoAccountData]
        public void TryUpdateNonExistingReturnsFalse(AccountService accountService, Account account)
        {
            Assert.False(accountService.TryUpdate(account));
        }

        [Theory, AutoAccountData]
        public async Task UpdateMeIsOk(IAccountRepository accountRepository, Account me)
        {
            var accountService = new AccountService(accountRepository);
            var authenticationService = new AuthenticationService(accountRepository);
            me.AllowMePasswordChange = true;
            accountService.Add(me);
            var meUpdated = new Account(me.Id, "New Name")
            {
                Company = "New Company",
                PhoneNumber = "123",
                Email = "New Email",
                Roles = "Guest, User"
            };
            meUpdated.SetPassword("New Password");
            accountService.UpdateMe(meUpdated);

            accountService.TryGet(me.Id, out var myEntity);
            Assert.Equal(me.Name, myEntity.Name); // Name is not updated
            Assert.Equal(meUpdated.Company, myEntity.Company);
            Assert.Equal(meUpdated.PhoneNumber, myEntity.PhoneNumber);
            Assert.Equal(meUpdated.Email, myEntity.Email);
            Assert.Equal(me.Roles, myEntity.Roles); // Roles are not updated
            Assert.True(await authenticationService.Validate(me.Id, "New Password"));
        }

        [Theory, AutoAccountData]
        public void UpdateMeIgnoresPasswordIfChangeNotAllowed(AccountService accountService, Account me)
        {
            me.AllowMePasswordChange = false;
            me.SetPassword("password");
            accountService.Add(me);
            var meUpdated = new Account(me.Id, "New Name")
            {
                Company = "New Company",
                PhoneNumber = "123",
                Email = "New Email",
                Roles = "Guest, User"
            };
            meUpdated.SetPassword("New Password");
            accountService.UpdateMe(meUpdated);

            accountService.TryGet(me.Id, out var myEntity);
            Assert.Equal(me.Name, myEntity.Name); // Name is not updated
            Assert.Equal(meUpdated.Company, myEntity.Company);
            Assert.Equal(meUpdated.PhoneNumber, myEntity.PhoneNumber);
            Assert.Equal(meUpdated.Email, myEntity.Email);
            Assert.Equal(me.Roles, myEntity.Roles); // Roles are not updated
            Assert.False(accountService.Validate(me.Id, "New Password")); // Password is not set
        }

        [Theory, AutoAccountData]
        public void EventsAreRaisedOnUpdate(AccountService accountService, Account account)
        {
            var raisedEvents = new List<string>();
            accountService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            accountService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            accountService.Add(account);

            var updatedAccount = new Account(account.Id, "Updated name");
            accountService.Update(updatedAccount);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = AccountService.GetRepositoryTypes();

            Assert.Contains(typeof(AccountRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesOverloadIsOk()
        {
            var repositoryTypes = AccountService.GetRepositoryTypes(null, "DHI.Services.dll");

            Assert.Contains(typeof(AccountRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesOverloadReturnsEmpty()
        {
            var repositoryTypes = AccountService.GetRepositoryTypes(null, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}