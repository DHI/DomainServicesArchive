namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using GIS;
    using Spatial;
    using Xunit;

    public class BaseGroupedGisRepositoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Theory, AutoGisData]
        public void CountIsOk(IGroupedGisRepository<Guid> repository)
        {
            Assert.Equal(_fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoGisData]
        public void GetIsOk(IGroupedGisRepository<Guid> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.Equal(timeSeries.Id, repository.Get(timeSeries.Id).Value.Id);
        }

        [Theory, AutoGisData]
        public void GetMultipleIsOk(IGroupedGisRepository<Guid> repository)
        {
            var id1 = repository.GetIds().ToArray()[0];
            var id2 = repository.GetIds().ToArray()[1];
            var featureCollections = repository.Get(new[] { id1, id2 }, false);
            Assert.Equal(id1, featureCollections[id1].Id);
            Assert.Equal(id2, featureCollections[id2].Id);
        }

        [Theory, AutoGisData]
        public void GetFullNamesIsOk(IGroupedGisRepository<Guid> repository)
        {
            Assert.True(repository.GetFullNames().Any());
            Assert.IsType<string>(repository.GetFullNames().First());
        }

        [Theory, AutoGisData]
        public void GetGeometryTypesByGroupIsOk(IGroupedGisRepository<Guid> repository)
        {
            var group = repository.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var geometryTypes = repository.GetGeometryTypes(group);
            Assert.Single(geometryTypes);
        }

        [Theory, AutoGisData]
        public void GetGeometryTypesIsOk(IGroupedGisRepository<Guid> repository)
        {
            var geometryTypes = repository.GetGeometryTypes().ToList();
            Assert.True(geometryTypes.Any());
            Assert.Equal(3, geometryTypes[0].Value.Length);
        }

        [Fact]
        public void GetGeometryTypesReturnsDistinct()
        {
            var featureCollectionList = new List<FeatureCollection<Guid>>();
            var features = new List<IFeature>();
            for (int i = 1; i < 4; i++)
            {
                features.Add(new Feature(new Point(new Position(i * 9.9, i * 8.8))));
            }

            var featureCollection = new FeatureCollection<Guid>(Guid.NewGuid(), "myFeatureCollection", features);
            featureCollectionList.Add(featureCollection);
            var repository = new FakeGisRepository(featureCollectionList);

            var geometryTypes = repository.GetGeometryTypes();
            Assert.Contains("myFeatureCollection", geometryTypes);
            Assert.Single(geometryTypes["myFeatureCollection"]);
            Assert.Equal("Point", geometryTypes["myFeatureCollection"].First());
        }

        [Theory, AutoGisData]
        public void GetIdsIsOk(IGroupedGisRepository<Guid> repository)
        {
            Assert.True(repository.GetIds().Any());
            Assert.IsType<Guid>(repository.GetIds().First());
        }

        [Theory, AutoGisData]
        public void ContainsIsOk(IGroupedGisRepository<Guid> repository)
        {
            var timeSeries = repository.GetAll().First();
            Assert.True(repository.Contains(timeSeries.Id));
        }

        [Theory, AutoGisData]
        public void DoesNotContainIsOk(IGroupedGisRepository<Guid> repository)
        {
            Assert.False(repository.Contains(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void ContainsAttributeIsOk(IGroupedGisRepository<Guid> repository)
        {
            var featureCollection = repository.GetAll().First();
            Assert.True(repository.ContainsAttribute(featureCollection.Id, featureCollection.Attributes.First().Name));
        }

        [Theory, AutoGisData]
        public void GetGeometryIsOk(IGroupedGisRepository<Guid> repository)
        {
            var featureCollection = repository.GetAll().First();
            Assert.IsAssignableFrom<IEnumerable<IGeometry>>(repository.GetGeometry(featureCollection.Id));
        }
    }
}