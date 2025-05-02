namespace DHI.Services.Test.Authorization
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using DHI.Services.Authorization;

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class AutoUserGroupDataAttribute : AutoDataAttribute
    {
        public AutoUserGroupDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    fixture.Customize<UserGroup>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                    var userGroups = fixture.CreateMany<UserGroup>().ToList();
                    fixture.Register<IUserGroupRepository>(() => new FakeUserGroupRepository(userGroups));
                }
                else
                {
                    fixture.Register<IUserGroupRepository>(() => new FakeUserGroupRepository());
                }

                return fixture;
            })
        {
        }
    }
}