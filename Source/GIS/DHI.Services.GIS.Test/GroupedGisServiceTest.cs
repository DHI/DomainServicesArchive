namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using GIS;
    using AutoFixture;
    using Spatial;
    using Xunit;
    using System.IO;

    public class GroupedGisServiceTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedGisService<Guid>(null));
        }

        [Theory, AutoGisData]
        public void GetNonExistingThrows(GroupedGisService<Guid> gisService, IEnumerable<QueryCondition> filter)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.Get(Guid.NewGuid()));
            Assert.Throws<KeyNotFoundException>(() => gisService.Get(Guid.NewGuid(), filter));
        }

        [Theory, AutoGisData]
        public void GetInfoForNonExistingThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetInfo(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetMultipleNonExistingReturnsEmpty(GisService<Guid> gisService)
        {
            Assert.Equal(0, gisService.Get(new[] { Guid.NewGuid(), Guid.NewGuid() }).Count);
        }

        [Theory, AutoGisData]
        public void GetByGroupForNonExistingThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoGisData]
        public void GetByGroupForNullGroupThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<ArgumentNullException>(() => gisService.GetByGroup(null));
        }

        [Theory, AutoGisData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoGisData]
        public void GetFullNamesForNullOrEmptyGroupThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<ArgumentNullException>(() => gisService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => gisService.GetFullNames(""));
        }

        [Theory, AutoGisData]
        public void GetNonExistingAttributeThrows(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            var filter = new List<QueryCondition>() { new QueryCondition("NonExistingAttribute", "") };
            Assert.Throws<KeyNotFoundException>(() => gisService.Get(featureCollection.Id, filter));
        }

        [Theory, AutoGisData]
        public void GetGeometryNonExistingThrows(GroupedGisService<Guid> gisService, IEnumerable<QueryCondition> filter)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetGeometry(Guid.NewGuid()));
            Assert.Throws<KeyNotFoundException>(() => gisService.GetGeometry(Guid.NewGuid(), filter));
        }

        [Theory, AutoGisData]
        public void GetGeometryNonExistingAttributeThrows(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            var filter = new List<QueryCondition>() { new QueryCondition("NonExistingAttribute", "") };
            Assert.Throws<KeyNotFoundException>(() => gisService.GetGeometry(featureCollection.Id, filter));
        }

        [Theory, AutoGisData]
        public void GetEnvelopeForNonExistingFeatureCollectionThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetEnvelope(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetFootprintForNonExistingFeatureCollectionThrows(GroupedGisService<Guid> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFootprint(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetStreamForNonExistingThrows(GroupedGisService<Guid> gisService)
        {
            var e = Assert.Throws<KeyNotFoundException>(() => gisService.GetStream(Guid.NewGuid()));
            Assert.Contains("not found", e.Message);
        }

        [Theory, AutoGisData]
        public void GetIsOk(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.Equal(featureCollection.Id, gisService.Get(featureCollection.Id).Id);
        }

        [Theory, AutoGisData]
        public void GetInfoIsOk(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            var info = gisService.GetInfo(featureCollection.Id);

            Assert.Equal(featureCollection.Id, info.Id);
        }

        [Theory, AutoGisData]
        public void GetInfoByGroupIsOk(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.Single(gisService.GetInfo(featureCollection.Group));
        }

        [Theory, AutoGisData]
        public void GetMultipleIsOk(GroupedGisService<Guid> gisService)
        {
            var id1 = gisService.GetIds().ToArray()[0];
            var id2 = gisService.GetIds().ToArray()[1];
            var featureCollections = gisService.Get(new[] { id1, id2 });
            Assert.Equal(id1, featureCollections[id1].Id);
            Assert.Equal(id2, featureCollections[id2].Id);
        }

        [Theory, AutoGisData]
        public void GetByGroupIsOk(GroupedGisService<Guid> gisService)
        {
            var group = gisService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(gisService.GetByGroup(group).Any());
        }

        [Theory, AutoGisData]
        public void GetByGroupsIsOk(GroupedGisService<Guid> gisService)
        {
            var group = gisService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(gisService.GetByGroups(new List<string> { group, group }).Any());
        }

        [Theory, AutoGisData]
        public void GetFullNamesByGroupIsOk(GroupedGisService<Guid> gisService)
        {
            var group = gisService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = gisService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoGisData]
        public void GetFullNamesIsOk(GroupedGisService<Guid> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.GetFullNames().Count());
        }

        [Theory, AutoGisData]
        public void GetGeometryTypesByGroupIsOk(GroupedGisService<Guid> gisService)
        {
            var group = gisService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var geometryTypes = gisService.GetGeometryTypes(group);
            Assert.Single(geometryTypes);
        }

        [Theory, AutoGisData]
        public void GetGeometryTypesIsOk(GroupedGisService<Guid> gisService)
        {
            var geometryTypes = gisService.GetGeometryTypes().ToList();
            Assert.True(geometryTypes.Any());
            Assert.Equal(3, geometryTypes[0].Value.Length);
        }

        [Fact]
        public void GetStreamIsOk()
        {
            var featureCollectionList = new List<FeatureCollection<Guid>>();
            var id = Guid.NewGuid();
            var features = new List<IFeature> {new Feature(new Point(new Position(9.9, 8.8)))};
            var featureCollection = new FeatureCollection<Guid>(id, "myFeatureCollection", features);
            featureCollectionList.Add(featureCollection);
            var gisService = new GroupedGisService<Guid>(new FakeGisRepository(featureCollectionList));
            var (stream, fileType, fileName) = gisService.GetStream(id);

            Assert.IsAssignableFrom<Stream>(stream);
            Assert.Equal(string.Empty, fileType);
            Assert.Equal(string.Empty, fileName);
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
            var gisService = new GroupedGisService<Guid>(new FakeGisRepository(featureCollectionList));

            var geometryTypes = gisService.GetGeometryTypes();
            Assert.Contains("myFeatureCollection", geometryTypes);
            Assert.Single(geometryTypes["myFeatureCollection"]);
            Assert.Equal("Point", geometryTypes["myFeatureCollection"].First());
        }

        [Theory, AutoGisData]
        public void GetGeometryReturnsGeometryCollection(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.IsType<GeometryCollection>(gisService.GetGeometry(featureCollection.Id));
        }

        [Theory, AutoGisData]
        public void GetAllIsOk(GroupedGisService<Guid> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.GetAll().Count());
        }

        [Theory, AutoGisData]
        public void GetIdsIsOk(GroupedGisService<Guid> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.GetIds().Count());
        }

        [Theory, AutoGisData]
        public void CountIsOk(GroupedGisService<Guid> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.Count());
        }

        [Theory, AutoGisData]
        public void ExistsIsOk(GroupedGisService<Guid> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.True(gisService.Exists(featureCollection.Id));
        }

        [Theory, AutoGisData]
        public void DoesNotExistsIsOk(GroupedGisService<Guid> gisService)
        {
            Assert.False(gisService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetIdsFilteredIsOk(GroupedGisService<Guid> gisService)
        {
            var allCollections = gisService.GetAll();
            var featureCollection = allCollections.ToArray()[0];
            var filter = new List<QueryCondition>() { new QueryCondition("id", featureCollection.Id) };
            var ids = gisService.GetIds(filter).ToList();
            Assert.Single(ids);
            Assert.Equal(featureCollection.Id, ids[0]);
        }
    }
}