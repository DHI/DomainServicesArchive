namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using Scenarios;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoScenarioDataAttribute : AutoDataAttribute
    {
        public AutoScenarioDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize<Scenario>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var scenarios = fixture.CreateMany<Scenario>().ToList();
                    fixture.Register<IScenarioRepository>(() => new FakeScenarioRepository(scenarios));
                    var jobList = fixture.CreateMany<Job<Guid, string>>().ToList();
                    fixture.Register<IJobRepository<Guid, string>>(() => new FakeJobRepository(jobList));
                }
                else
                {
                    fixture.Register<IScenarioRepository>(() => new FakeScenarioRepository());
                    fixture.Register<IJobRepository<Guid, string>>(() => new FakeJobRepository());
                }

                return fixture;
            })
        {
        }
    }
}