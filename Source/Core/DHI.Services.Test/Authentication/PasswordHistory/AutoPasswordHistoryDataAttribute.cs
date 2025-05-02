namespace DHI.Services.Test.Authentication.PasswordHistory
{
    using System;
    using System.Linq;
    using Accounts;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using DHI.Services.Authentication.PasswordHistory;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoPasswordHistoryDataAttribute : AutoDataAttribute
    {
        public AutoPasswordHistoryDataAttribute(int repeatCount = 0)
             : base(() =>
             {
                 var fixture = new Fixture();
                 if (repeatCount > 0)
                 {
                     fixture.RepeatCount = repeatCount;
                     fixture.Customize<PasswordHistory>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                     var userGroups = fixture.CreateMany<PasswordHistory>().ToList();
                     fixture.Register<IPasswordHistoryRepository>(() => new FakePasswordHistoryRepository(userGroups));
                     fixture.Register<IAccountRepository>(() => new FakeAccountRepository());
                     fixture.Register<PasswordExpirationPolicy>(() => new PasswordExpirationPolicy() { PasswordExpiryDurationInDays = 3, PreviousPasswordsReUseLimit = 3 });
                 }
                 else
                 {
                     fixture.Register<IPasswordHistoryRepository>(() => new FakePasswordHistoryRepository());
                     fixture.Register<IAccountRepository>(() => new FakeAccountRepository());
                     fixture.Register<PasswordExpirationPolicy>(() => new PasswordExpirationPolicy() { PasswordExpiryDurationInDays = 3, PreviousPasswordsReUseLimit = 3 });
                 }

                 return fixture;
             })
        {
        }
    }
}
