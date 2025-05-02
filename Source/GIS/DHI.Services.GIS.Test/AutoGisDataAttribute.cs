namespace DHI.Services.GIS.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;
    using GIS;
    using Spatial;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoGisDataAttribute : AutoDataAttribute
    {
        public AutoGisDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
                fixture.Customize<FeatureCollection<Guid>>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
                fixture.Register<IFeature>(() => new FakeFeature(fixture.Create<string>(), fixture.Create<IGeometry>()));
                fixture.Register<IAttribute>(() => fixture.Create<Spatial.Attribute>());
                var featureCollectionList = fixture.CreateMany<FeatureCollection<Guid>>().ToList();
                foreach (var featureCollection in featureCollectionList)
                {
                    fixture.AddManyTo(featureCollection.Attributes);
                }

                fixture.Register<IGroupedUpdatableGisRepository<Guid, string>>(() => new FakeGisRepository(featureCollectionList));
                fixture.Register<IGroupedGisRepository<Guid>>(() => new FakeGisRepository(featureCollectionList));
                fixture.Register<IUpdatableGisRepository<Guid, string>>(() => new FakeGisRepository(featureCollectionList));
                fixture.Register<IGisRepository<Guid>>(() => new FakeGisRepository(featureCollectionList));

                return fixture;
            })
        {
        }
    }
}