namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using Accounts;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using DHI.Services.Authentication.PasswordHistory;
    using DHI.Services.Notifications;
    using DHI.Services.Test.Authentication.PasswordHistory;
    using Mails;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoRegistrationDataAttribute : AutoDataAttribute
    {
        public AutoRegistrationDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize<Account>(c => c.With(account => account.Email, fixture.Create<MailAddress>().Address));
                fixture.Customize<MailTemplate>(c => c
                                                     .With(template => template.From, fixture.Create<MailAddress>().Address)
                                                     .With(template => template.Body, "default body")
                                                     .With(template => template.Bodies, new Dictionary<string, string> { { "custom", "custom body" } }));
                fixture.Customize<PasswordHistory>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                var userGroups = fixture.CreateMany<PasswordHistory>().ToList();

                fixture.Inject(TimeSpan.FromDays(1));
                fixture.Register<IMailSender>(() => new FakeMailSender());
                fixture.Register<IAccountRepository>(() => new FakeAccountRepository());
                fixture.Register<IPasswordHistoryRepository>(() => new FakePasswordHistoryRepository(userGroups));
                fixture.Register<PasswordExpirationPolicy>(() => new PasswordExpirationPolicy());
                fixture.Register<ILogger>(() => NullLogger.Instance);

                return fixture;
            })
        {
        }
    }
}