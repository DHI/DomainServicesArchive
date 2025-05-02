namespace DHI.Services.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoConnectionDataAttribute : AutoDataAttribute
    {
        public AutoConnectionDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var connectionList = fixture.CreateMany<FakeConnection>().ToList();
                    fixture.Register<IConnectionRepository>(() => new FakeConnectionRepository(connectionList));
                }
                else
                {
                    fixture.Register<IConnectionRepository>(() => new FakeConnectionRepository());
                }

                return fixture;
            })
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class ConnectionDataAttribute : AutoDataAttribute
    {
        public ConnectionDataAttribute()
            : base(() =>
            {
                var _filePath = Path.Combine(Path.GetTempPath(), $"__connections_{DateTime.Now:yyyy_MM_dd_hh_mm_ss_sss}.json");
                var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
                fixture.Register<IConnectionRepository>(() => new ConnectionRepository(_filePath));
                return fixture;
            })
        {
        }
    }

}