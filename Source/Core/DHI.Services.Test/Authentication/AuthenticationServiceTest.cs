namespace DHI.Services.Test.Authentication
{
    using System;
    using System.Threading.Tasks;
    using Accounts;
    using DHI.Services.Authentication;
    using Xunit;
    using System.Threading;

    public class AuthenticationServiceTest
    {

        [Theory, AutoAccountData]
        public void GetNonExistingThrows(AuthenticationService authenticationService)
        {
            Assert.False(authenticationService.TryGet("UnknownAccount", out _));
        }

        [Theory, AutoAccountData]
        public async Task ValidateWithNullClientIpThrows(IAccountRepository accountRepository, Account account)
        {
            var accountServices = new AccountService(accountRepository);
            account.SetPassword("password");
            accountServices.Add(account);
            var authenticationService = new AuthenticationService(accountRepository);

            await Assert.ThrowsAsync<ArgumentNullException>(() => authenticationService.Validate(account.Id, "password", null));
        }

        [Theory, AutoAccountData]
        public async Task ValidateWithClientIpIsOk(IAccountRepository accountRepository, Account account, string clientIp)
        {
            var accountServices = new AccountService(accountRepository);
            account.SetPassword("password");
            accountServices.Add(account);
            var authenticationService = new AuthenticationService(accountRepository);

            Assert.True(await authenticationService.Validate(account.Id, "password", clientIp));
            Assert.False(await authenticationService.Validate(account.Id, "WrongPassword", clientIp));
        }

        [Theory, AutoAccountData]
        public async Task ValidateNonExistingAccountFails(AuthenticationService authenticationService, string clientIp)
        {
            Assert.False(await authenticationService.Validate("UnknownAccount", "password", clientIp));
        }

        [Theory, AutoAccountData]
        public async Task ValidateNonActivatedFails(IAccountRepository accountRepository, Account account, string clientIp)
        {
            var accountServices = new AccountService(accountRepository);
            account.SetPassword("password");
            accountServices.Add(account);
            account.Activated = false;
            accountServices.Update(account);
            var authenticationService = new AuthenticationService(accountRepository);

            Assert.False(await authenticationService.Validate(account.Id, "password", clientIp));
        }

        [Fact]
        public void GetAuthenticationProviderTypesIsOk()
        {
            var providerTypes = AuthenticationService.GetAuthenticationProviderTypes();

            Assert.Contains(typeof(AccountRepository), providerTypes);
        }

        [Fact]
        public void GetAuthenticationProviderTypesOverloadIsOk()
        {
            var providerTypes = AuthenticationService.GetAuthenticationProviderTypes(null, "DHI.Services.dll");

            Assert.Contains(typeof(AccountRepository), providerTypes);
        }

        [Fact]
        public void GetAuthenticationProviderTypesOverloadReturnsEmpty()
        {
            var providerTypes = AuthenticationService.GetAuthenticationProviderTypes(null, "DHI.Solutions*.dll");

            Assert.Empty(providerTypes);
        }


        [Theory, AutoAccountData]
        public async Task ValidationFailureDelayIsOk(IAccountRepository accountRepository, Account account, string clientIp)
        {
            var accountServices = new AccountService(accountRepository);
            account.SetPassword("password");
            accountServices.Add(account);
            var authenticationService = new AuthenticationService(accountRepository);

            // 1st attempt (failure)
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);

            // 2nd attempt (failure)
            var before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);
            Thread.Sleep(1000);
            Assert.True(DateTime.UtcNow - before >= TimeSpan.FromSeconds(1));

            // 3rd attempt (failure)
            before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);
            Thread.Sleep(2000);
            Assert.True(DateTime.UtcNow - before >= TimeSpan.FromSeconds(2));

            // 4th attempt (failure)
            before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);
            Thread.Sleep(4000);
            Assert.True(DateTime.UtcNow - before >= TimeSpan.FromSeconds(4));
        }

        [Theory, AutoAccountData]
        public async Task ValidationFailureDelayResetIsOk(IAccountRepository accountRepository, Account account, string clientIp)
        {
            // Arrange
            var accountServices = new AccountService(accountRepository);
            account.SetPassword("password");
            accountServices.Add(account);
            var authenticationService = new AuthenticationService(accountRepository);

            // Act & Assert
            // 1st attempt (failure)
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);

            // 2nd attempt (failure)
            var before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);
            await Task.Delay(1000);
            Assert.True(DateTime.UtcNow - before >= TimeSpan.FromSeconds(1));

            // 3rd attempt (failure)
            before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "WrongPassword", clientIp);
            await Task.Delay(2000);
            Assert.True(DateTime.UtcNow - before >= TimeSpan.FromSeconds(2));

            // 4th attempt (success)
            before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "password", clientIp);
            Assert.True(DateTime.UtcNow - before <= TimeSpan.FromSeconds(0.1));

            // 5th attempt (after reset)
            before = DateTime.UtcNow;
            await authenticationService.Validate(account.Id, "password", clientIp);
            Assert.True(DateTime.UtcNow - before <= TimeSpan.FromSeconds(0.1));
        }

    }
}