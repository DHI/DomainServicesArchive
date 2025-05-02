namespace DHI.Services.Scalars.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using Logging;
    using Microsoft.Extensions.Logging;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoScalarDataAttribute : AutoDataAttribute
    {
        public AutoScalarDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Register<object>(() => new Random().Next(0, 100));
                fixture.Register(() => new Scalar<Guid, int>(Guid.NewGuid(), fixture.Create<string>(), "System.Int32", fixture.Create<string>(), fixture.Create<ScalarData<int>>())
                {
                    Description = fixture.Create<string>()
                });

                fixture.Register(() => new Scalar<string, int>(Guid.NewGuid().ToString(), fixture.Create<string>(), "System.Int32", fixture.Create<string>(), fixture.Create<ScalarData<int>>())
                {
                    Description = fixture.Create<string>()
                });

                var scalarList = fixture.CreateMany<Scalar<Guid, int>>().ToList();

                fixture.Register<IScalarRepository<Guid, int>>(() => new FakeScalarRepository(scalarList));
                fixture.Register<IGroupedScalarRepository<Guid, int>>(() => new FakeScalarRepository(scalarList));
                fixture.Register<ILogger>(() => new FakeLogger());
                return fixture;
            })
        {
        }
    }
}