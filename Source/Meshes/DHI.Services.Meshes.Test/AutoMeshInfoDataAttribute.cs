namespace DHI.Services.Meshes.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using TimeSeries;
    using Spatial.Data;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoMeshInfoDataAttribute : AutoDataAttribute
    {
        public AutoMeshInfoDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize<MeshInfo<Guid>>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                var itemList = fixture.CreateMany<Item>().ToList();
                fixture.Register<IEnumerable<Item>>(() => new List<Item>(itemList));
                fixture.Register(() => AggregationType.Maximum);
                fixture.Inject(new DateRange(fixture.Create<DateTime>(), fixture.Create<TimeSpan>()));
                var meshInfoList = fixture.CreateMany<MeshInfo<Guid>>().ToList();
                fixture.Register<IGroupedMeshRepository<Guid>>(() => new FakeGroupedMeshRepository(meshInfoList));
                fixture.Register<IMeshRepository<Guid>>(() => new FakeGroupedMeshRepository(meshInfoList));
                return fixture;
            })
        {
        }
    }
}