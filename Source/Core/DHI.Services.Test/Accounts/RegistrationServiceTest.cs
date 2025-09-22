namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounts;
    using DHI.Services.Authentication;
    using DHI.Services.Authentication.PasswordHistory;
    using DHI.Services.Test.Authentication.PasswordHistory;
    using DHI.Services.Test.Notifications;
    using Mails;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class RegistrationServiceTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RegistrationServiceTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrationService(null, new FakeMailSender(), new MailTemplate("myTemplate1", "My Template1"), new MailTemplate("myTemplate2", "My Template2")));
        }

        [Fact]
        public void CreateWithNullMailSenderThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrationService(new FakeAccountRepository(), null, new MailTemplate("myTemplate1", "My Template1"), new MailTemplate("myTemplate2", "My Template2")));
        }

        [Fact]
        public void CreateWithNullActivationMailTemplateThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrationService(new FakeAccountRepository(), new FakeMailSender(), null, new MailTemplate("myTemplate", "My Template")));
        }

        [Fact]
        public void CreateWithNullPasswordResetMailTemplateThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrationService(new FakeAccountRepository(), new FakeMailSender(), new MailTemplate("myTemplate", "My Template"), null));
        }

        [Theory, AutoRegistrationData]
        public void RegisterWithIllegalMailThrows(RegistrationService registrationService)
        {
            var account = new Account("user", "User") { Email = "IllegalEmail" };
            Assert.Throws<ArgumentException>(() => registrationService.Register(account, ""));
        }

        [Theory, AutoRegistrationData]
        public void UpdatePasswordWithNullOrEmptyPasswordThrows(RegistrationService registrationService, string token)
        {
            Assert.Throws<ArgumentException>(() => registrationService.UpdatePassword(token, ""));
        }

        [Theory, AutoRegistrationData]
        public void ResetPasswordThrowsIfNotAllowed(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate)
        {
            var accountService = new AccountService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, new FakeMailSender(), activationTemplate, resetTemplate);
            account.AllowMePasswordChange = false;
            accountService.Add(account);
            Assert.Throws<InvalidOperationException>(() => registrationService.ResetPassword(account.Email, "", "default"));
        }

        [Theory, AutoRegistrationData]
        public void ResetPasswordWithBadMailBodyThrows(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate)
        {
            var accountService = new AccountService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, new FakeMailSender(), activationTemplate, resetTemplate);
            account.AllowMePasswordChange = true;
            accountService.Add(account);

            var e = Assert.Throws<ArgumentOutOfRangeException>(() => registrationService.ResetPassword(account.Email, "", "notValid"));
            Assert.Contains("Mail template bodies does not include a definition for 'notValid'", e.Message);
        }

        [Theory, AutoRegistrationData]
        public void RegisterIsOk(RegistrationService registrationService, Account account)
        {
            account.SetPassword("password");
            registrationService.Register(account, "");

            Assert.False(account.Activated);
            Assert.NotNull(account.Token);
            Assert.Equal("Guest, User", account.Roles);
        }

        [Theory, AutoRegistrationData]
        public void ActivateIsOk(RegistrationService registrationService, Account account)
        {
            account.SetPassword("password");
            registrationService.Register(account, "");
            registrationService.Activate(account.Token);

            Assert.True(account.Activated);
            Assert.Null(account.Token);
        }

        [Theory, AutoRegistrationData]
        public void ActivateAlreadyActivatedIsOk(RegistrationService registrationService, Account account)
        {
            account.SetPassword("password");
            registrationService.Register(account, "");
            registrationService.Activate(account.Token);
            registrationService.Activate(account.Token);
            registrationService.Activate("TokenIsNotUsedAnyway...");

            Assert.True(account.Activated);
            Assert.Null(account.Token);
        }

        [Theory, AutoRegistrationData]
        public void ActivateWithWrongTokenFails(RegistrationService registrationService, Account account)
        {
            account.SetPassword("password");
            registrationService.Register(account, "");
            registrationService.Activate("WrongToken");

            Assert.False(account.Activated);
            Assert.NotNull(account.Token);
        }

        [Theory, AutoRegistrationData]
        public void ActivateWithExpiredTokenFails(RegistrationService registrationService, Account account)
        {
            account.SetPassword("password");
            account = registrationService.Register(account, "");
            account.TokenExpiration = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
            registrationService.Activate(account.Token);

            Assert.False(account.Activated);
        }

        [Theory, AutoRegistrationData]
        public void ResetPasswordReturnsEmptyMaybeIfAccountNotFound(RegistrationService registrationService)
        {
            Assert.False(registrationService.ResetPassword("NonExistingIdOrMail", "", "default").HasValue);
        }

        [Theory, AutoRegistrationData]
        public void ResetPasswordWithDefaultMailBodyIsOk(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate)
        {
            var fakeMailSender = new FakeMailSender();
            var accountService = new AccountService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, fakeMailSender, activationTemplate, resetTemplate);
            account.AllowMePasswordChange = true;
            accountService.Add(account);
            var maybe = registrationService.ResetPassword(account.Email, "", "default");

            Assert.True(maybe.HasValue);
            Assert.NotNull(maybe.Value.Token);
            Assert.Equal("default body", fakeMailSender.Message.Body);
        }

        [Theory, AutoRegistrationData]
        public void ResetPasswordWithCustomMailBodyIsOk(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate)
        {
            var fakeMailSender = new FakeMailSender();
            var accountService = new AccountService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, fakeMailSender, activationTemplate, resetTemplate);
            account.AllowMePasswordChange = true;
            accountService.Add(account);
            var maybe = registrationService.ResetPassword(account.Email, "", "custom");

            Assert.True(maybe.HasValue);
            Assert.NotNull(maybe.Value.Token);
            Assert.Equal("custom body", fakeMailSender.Message.Body);
        }

        [Theory, AutoRegistrationData]
        public async Task UpdatePasswordIsOk(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate, IPasswordHistoryRepository passwordHistoryRepository)
        {
            var fakeLogger = new TestLoggerMicrosoft(_testOutputHelper);//NullLogger.Instance;
            var accountService = new AccountService(accountRepository);
            var authenticationService = new AuthenticationService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, new FakeMailSender(), activationTemplate, resetTemplate, default, passwordHistoryRepository, new PasswordExpirationPolicy(), fakeLogger);

            account.AllowMePasswordChange = true;
            accountService.Add(account);
            account = registrationService.ResetPassword(account.Email, "", "default").Value;
            registrationService.UpdatePassword(account.Token, "password");

            Assert.True(await authenticationService.Validate(account.Id, "password"));
        }

        [Theory, AutoRegistrationData]
        public void UpdatePasswordWithWrongTokenFails(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate)
        {
            var accountService = new AccountService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, new FakeMailSender(), activationTemplate, resetTemplate);
            account.AllowMePasswordChange = true;
            account.SetPassword("password");
            accountService.Add(account);
            account = registrationService.ResetPassword(account.Email, "", "default").Value;
            registrationService.UpdatePassword("WrongToken", "NewPassword");

            Assert.False(account.ValidatePassword("NewPassword"));
        }

        [Theory, AutoRegistrationData]
        public void UpdatePasswordWithExpiredTokenFails(IAccountRepository accountRepository, Account account, MailTemplate activationTemplate, MailTemplate resetTemplate)
        {
            var accountService = new AccountService(accountRepository);
            var registrationService = new RegistrationService(accountRepository, new FakeMailSender(), activationTemplate, resetTemplate);
            account.AllowMePasswordChange = true;
            account.SetPassword("password");
            accountService.Add(account);
            account = registrationService.ResetPassword(account.Email, "", "default").Value;
            account.TokenExpiration = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
            registrationService.UpdatePassword(account.Token, "NewPassword");

            Assert.False(account.ValidatePassword("NewPassword"));
        }

        [Fact]
        public void GetRepositoryTypesIsOk()
        {
            var repositoryTypes = RegistrationService.GetRepositoryTypes();

            Assert.Contains(typeof(AccountRepository), repositoryTypes);
        }
    }
}