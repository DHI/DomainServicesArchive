namespace DHI.Services.Test
{
    using Accounts;
    using DHI.Services.Notifications;
    using Mails;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ServiceTest
    {
        [Fact]
        public void GetLoggerProviderTypesIsOk()
        {
            var loggerTypes = Service.GetProviderTypes<ILogger>();

            Assert.Contains(typeof(SimpleLogger), loggerTypes);
        }

        [Fact]
        public void GetMailTemplateRepositoryProviderTypesIsOk()
        {
            var repositoryTypes = Service.GetProviderTypes<IMailTemplateRepository>();

            Assert.Contains(typeof(MailTemplateRepository), repositoryTypes);
        }

        [Fact]
        public void GetAccountRepositoryProviderTypesIsOk()
        {
            var repositoryTypes = Service.GetProviderTypes<IAccountRepository>();

            Assert.Contains(typeof(AccountRepository), repositoryTypes);
        }

        [Fact]
        public void GetAccountRepositoryProviderTypesOverloadIsOk()
        {
            var repositoryTypes = Service.GetProviderTypes<IAccountRepository>(null, "DHI.Services.dll");

            Assert.Contains(typeof(AccountRepository), repositoryTypes);
        }

        [Fact]
        public void GetAccountRepositoryProviderTypesOverloadReturnsEmpty()
        {
            var repositoryTypes = Service.GetProviderTypes<IAccountRepository>(null, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}