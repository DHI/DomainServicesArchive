namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoHostDataAttribute : AutoDataAttribute
    {
        public AutoHostDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize<Host>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var hostList = fixture.CreateMany<Host>().ToList();
                    fixture.Register<IHostRepository>(() => new FakeHostRepository(hostList));
                    fixture.Register<IGroupedHostRepository>(() => new FakeGroupedHostRepository(hostList));
                }
                else
                {
                    fixture.Register<IHostRepository>(() => new FakeHostRepository());
                    fixture.Register<IGroupedHostRepository>(() => new FakeGroupedHostRepository());
                }

                return fixture;
            })
        {
        }
    }
}