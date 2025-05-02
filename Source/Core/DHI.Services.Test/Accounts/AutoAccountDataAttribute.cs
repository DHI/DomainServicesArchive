namespace DHI.Services.Test
{
    using System;
    using System.Linq;
    using System.Net.Mail;
    using Accounts;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using DHI.Services.Authentication;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoAccountDataAttribute : AutoDataAttribute
    {
        public AutoAccountDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize<Account>(c => c.With(account => account.Email, fixture.Create<MailAddress>().Address));
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var accountList = fixture.CreateMany<Account>().ToList();
                    fixture.Register<IAccountRepository>(() => new FakeAccountRepository(accountList));
                    fixture.Register<IAuthenticationProvider>(() => new FakeAccountRepository(accountList));
                }
                else
                {
                    fixture.Register<IAccountRepository>(() => new FakeAccountRepository());
                    fixture.Register<IAuthenticationProvider>(() => new FakeAccountRepository());
                }

                return fixture;
            })
        {
        }
    }
}