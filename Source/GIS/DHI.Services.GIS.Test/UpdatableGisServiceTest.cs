namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GIS;
    using AutoFixture;
    using Spatial;
    using Xunit;

    public class UpdatableGisServiceTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatableGisService<Guid, string>(null));
        }

        [Theory, AutoGisData]
        public void GetNonExistingThrows(UpdatableGisService<Guid, string> gisService, IEnumerable<QueryCondition> filter)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.Get(Guid.NewGuid()));
            Assert.Throws<KeyNotFoundException>(() => gisService.Get(Guid.NewGuid(), filter));
        }

        [Theory, AutoGisData]
        public void GetInfoForNonExistingThrows(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetInfo(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetMultipleNonExistingReturnsEmpty(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Equal(0, gisService.Get(new[] { Guid.NewGuid(), Guid.NewGuid() }).Count);
        }

        [Theory, AutoGisData]
        public void GetNonExistingAttributeThrows(UpdatableGisService<Guid, string> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            var filter = new List<QueryCondition>() { new QueryCondition("NonExistingAttribute", "") };
            Assert.Throws<KeyNotFoundException>(() => gisService.Get(featureCollection.Id, filter));
        }

        [Theory, AutoGisData]
        public void GetGeometryNonExistingThrows(UpdatableGisService<Guid, string> gisService, IEnumerable<QueryCondition> filter)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetGeometry(Guid.NewGuid()));
            Assert.Throws<KeyNotFoundException>(() => gisService.GetGeometry(Guid.NewGuid(), filter));
        }

        [Theory, AutoGisData]
        public void GetGeometryNonExistingAttributeThrows(UpdatableGisService<Guid, string> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            var filter = new List<QueryCondition>() { new QueryCondition("NonExistingAttribute", "") };
            Assert.Throws<KeyNotFoundException>(() => gisService.GetGeometry(featureCollection.Id, filter));
        }

        [Theory, AutoGisData]
        public void UpdateNonExistingThrows(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.Update(featureCollection));
        }

        [Theory, AutoGisData]
        public void RemoveNonExistingThrows(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.Remove(featureCollection.Id));
        }

        [Theory, AutoGisData]
        public void AddExistingThrows(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            gisService.Add(featureCollection);
            Assert.Throws<ArgumentException>(() => gisService.Add(featureCollection));
        }

        [Theory, AutoGisData]
        public void UpdateNonExistingFeatureThrows(UpdatableGisService<Guid, string> gisService, IFeature feature)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.UpdateFeature(collectionId, feature));
        }

        [Theory, AutoGisData]
        public void UpdateFeatureWithoutIdThrows(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var feature = new Feature(new Point(new Position(1, 1)));
            Assert.Throws<ArgumentException>(() => gisService.UpdateFeature(collectionId, feature));
        }

        [Theory, AutoGisData]
        public void UpdateFeatureInNonExistingCollectionThrows(UpdatableGisService<Guid, string> gisService, IFeature feature)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.UpdateFeature(Guid.NewGuid(), feature));
        }

        [Theory, AutoGisData]
        public void RemoveNonExistingFeatureThrows(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.RemoveFeature(collectionId, "NonExistingFeatureId"));
        }

        [Theory, AutoGisData]
        public void RemoveFeatureInNonExistingCollectionThrows(UpdatableGisService<Guid, string> gisService, string featureId)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.RemoveFeature(Guid.NewGuid(), featureId));
        }

        [Theory, AutoGisData]
        public void RemoveFeaturesInNonExistingCollectionThrows(UpdatableGisService<Guid, string> gisService, IList<string> featureIds)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.RemoveFeatures(Guid.NewGuid(), featureIds));
        }

        [Theory, AutoGisData]
        public void AddExistingFeatureThrows(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var feature = collection.Features[0];
            Assert.Throws<ArgumentException>(() => gisService.AddFeature(collection.Id, feature));
        }

        [Theory, AutoGisData]
        public void AddFeatureWithoutIdThrows(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var feature = new Feature(new Point(new Position(1,1)));
            Assert.Throws<ArgumentException>(() => gisService.AddFeature(collection.Id, feature));
        }

        [Theory, AutoGisData]
        public void AddFeatureInNonExistingCollectionThrows(UpdatableGisService<Guid, string> gisService, IFeature feature)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.AddFeature(Guid.NewGuid(), feature));
        }

        [Theory, AutoGisData]
        public void GetEnvelopeForNonExistingFeatureCollectionThrows(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetEnvelope(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetFootprintForNonExistingFeatureCollectionThrows(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFootprint(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void GetFeatureForNonExistingFeatureCollectionThrows(UpdatableGisService<Guid, string> gisService, string featureId)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFeature(Guid.NewGuid(), featureId));
        }

        [Theory, AutoGisData]
        public void GetFeatureForNonExistingFeatureThrows(UpdatableGisService<Guid, string> gisService, string featureId)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFeature(collectionId, featureId));
        }

        [Theory, AutoGisData]
        public void GetFeatureInfoForNonExistingFeatureThrows(UpdatableGisService<Guid, string> gisService, string featureId)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFeatureInfo(collectionId, featureId));
        }

        [Theory, AutoGisData]
        public void GetFeatureIdsForNonExistingFeatureCollectionThrows(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Throws<KeyNotFoundException>(() => gisService.GetFeatureIds(Guid.NewGuid(), null));
        }

        [Theory, AutoGisData]
        public void GetStreamForNonExistingThrows(UpdatableGisService<Guid, string> gisService)
        {
            var e = Assert.Throws<KeyNotFoundException>(() => gisService.GetStream(Guid.NewGuid()));
            Assert.Contains("not found", e.Message);
        }

        [Theory, AutoGisData]
        public void GetIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.Equal(featureCollection.Id, gisService.Get(featureCollection.Id).Id);
        }

        [Theory, AutoGisData]
        public void GetInfoIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.Equal(featureCollection.Id, gisService.GetInfo(featureCollection.Id).Id);
        }

        [Theory, AutoGisData]
        public void GetMultipleIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var id1 = gisService.GetIds().ToArray()[0];
            var id2 = gisService.GetIds().ToArray()[1];
            var featureCollections = gisService.Get(new[] { id1, id2 });
            Assert.Equal(id1, featureCollections[id1].Id);
            Assert.Equal(id2, featureCollections[id2].Id);
        }

        [Fact]
        public void GetStreamIsOk()
        {
            var featureCollectionList = new List<FeatureCollection<Guid>>();
            var id = Guid.NewGuid();
            var features = new List<IFeature> { new Feature(new Point(new Position(9.9, 8.8))) };
            var featureCollection = new FeatureCollection<Guid>(id, "myFeatureCollection", features);
            featureCollectionList.Add(featureCollection);
            var gisService = new GroupedGisService<Guid>(new FakeGisRepository(featureCollectionList));
            var (stream, fileType, fileName) = gisService.GetStream(id);

            Assert.IsAssignableFrom<Stream>(stream);
            Assert.Equal(string.Empty, fileType);
            Assert.Equal(string.Empty, fileName);
        }

        [Theory, AutoGisData]
        public void GetGeometryReturnsGeometryCollection(UpdatableGisService<Guid, string> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.IsType<GeometryCollection>(gisService.GetGeometry(featureCollection.Id));
        }

        [Theory, AutoGisData]
        public void GetAllIsOk(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.GetAll().Count());
        }

        [Theory, AutoGisData]
        public void GetIdsIsOk(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.GetIds().Count());
        }

        [Theory, AutoGisData]
        public void GetGeometryTypesIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var geometryTypes = gisService.GetGeometryTypes().ToList();
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
            var gisService = new UpdatableGisService<Guid, string>(new FakeGisRepository(featureCollectionList));

            var geometryTypes = gisService.GetGeometryTypes();
            Assert.Contains("myFeatureCollection", geometryTypes);
            Assert.Single(geometryTypes["myFeatureCollection"]);
            Assert.Equal("Point", geometryTypes["myFeatureCollection"].First());
        }

        [Theory, AutoGisData]
        public void CountIsOk(UpdatableGisService<Guid, string> gisService)
        {
            Assert.Equal(_fixture.RepeatCount, gisService.Count());
        }

        [Theory, AutoGisData]
        public void ExistsIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var featureCollection = gisService.GetAll().ToArray()[0];
            Assert.True(gisService.Exists(featureCollection.Id));
        }

        [Theory, AutoGisData]
        public void DoesNotExistsIsOk(UpdatableGisService<Guid, string> gisService)
        {
            Assert.False(gisService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoGisData]
        public void AddIsOk(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            gisService.Add(featureCollection);
            Assert.Equal(featureCollection.Id, gisService.Get(featureCollection.Id).Id);
        }

        [Theory, AutoGisData]
        public void RemoveIsOk(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            gisService.Add(featureCollection);
            gisService.Remove(featureCollection.Id);

            Assert.False(gisService.Exists(featureCollection.Id));
        }

        [Theory, AutoGisData]
        public void UpdateIsOk(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            gisService.Add(featureCollection);
            var updatedFeatureCollection = new FeatureCollection<Guid>(featureCollection.Id, "Updated Name");
            gisService.Update(updatedFeatureCollection);

            Assert.Equal(updatedFeatureCollection.Name, gisService.Get(featureCollection.Id).Name);
        }

        [Theory, AutoGisData]
        public void EventsAreRaisedOnAdd(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            var raisedEvents = new List<string>();
            gisService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            gisService.Added += (s, e) => { raisedEvents.Add("Added"); };

            gisService.Add(featureCollection);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoGisData]
        public void EventsAreRaisedOnRemove(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            var raisedEvents = new List<string>();
            gisService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            gisService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            gisService.Add(featureCollection);

            gisService.Remove(featureCollection.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoGisData]
        public void EventsAreRaisedOnUpdate(UpdatableGisService<Guid, string> gisService, FeatureCollection<Guid> featureCollection)
        {
            var raisedEvents = new List<string>();
            gisService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            gisService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            gisService.Add(featureCollection);

            var updatedAccount = new FeatureCollection<Guid>(featureCollection.Id, "Updated name");
            gisService.Update(updatedAccount);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoGisData]
        public void GetFeatureIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var featureId = (string)collection.Features[0].AttributeValues["id"];
            var feature = gisService.GetFeature(collection.Id, featureId);

            Assert.IsAssignableFrom<IFeature>(feature);
        }

        [Theory, AutoGisData]
        public void GetFeatureInfoIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var featureId = (string)collection.Features[0].AttributeValues["id"];
            var feature = gisService.GetFeatureInfo(collection.Id, featureId);

            Assert.IsAssignableFrom<FeatureInfo>(feature);
        }

        [Theory, AutoGisData]
        public void GetFeatureIdsIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var featureId = (string)collection.Features[0].AttributeValues["id"];
            var featureIds = gisService.GetFeatureIds(collection.Id, new List<QueryCondition> { new QueryCondition("id", featureId) });

            Assert.Single(featureIds);
        }

        [Theory, AutoGisData]
        public void AddFeatureIsOk(UpdatableGisService<Guid, string> gisService, IFeature feature)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            gisService.AddFeature(collectionId, feature);

            Assert.Equal(4, gisService.Get(collectionId).Features.Count);
        }

        [Theory, AutoGisData]
        public void RemoveFeatureIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var featureId = (string)collection.Features[0].AttributeValues["id"];
            gisService.RemoveFeature(collection.Id, featureId);

            Assert.Equal(2, gisService.Get(collection.Id).Features.Count);
        }

        [Theory, AutoGisData]
        public void RemoveFeaturesIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var featureCount = collection.Features.Count;
            var featureIds = collection.Features.Select(f => (string)f.AttributeValues["id"]).ToList();
            gisService.RemoveFeatures(collection.Id, featureIds);
            Assert.Equal(featureCount - featureIds.Count(), gisService.Get(collection.Id).Features.Count);
        }

        [Theory, AutoGisData]
        public void UpdateFeatureIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collection = gisService.GetAll().ToList()[0];
            var feature = collection.Features[0];
            var updatedFeature = new Feature(new Point(new Position(1,1)));
            updatedFeature.AttributeValues.Add("id", feature.AttributeValues["id"]);
            gisService.UpdateFeature(collection.Id, updatedFeature);

            var newFeature = gisService.Get(collection.Id).Features[0];
            Assert.Equal(1, ((Position)((Point)newFeature.Geometry).Coordinates).X);
        }

        [Theory, AutoGisData]
        public void GetIdsFilteredIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var allCollections = gisService.GetAll();
            var featureCollection = allCollections.ToArray()[0];
            var filter = new List<QueryCondition>() { new QueryCondition("id", featureCollection.Id) };
            var ids = gisService.GetIds(filter).ToList();
            Assert.Single(ids);
            Assert.Equal(featureCollection.Id, ids[0]);
        }

        [Theory, AutoGisData]
        public void AddAttributeIsOk(UpdatableGisService<Guid, string> gisService, IAttribute attribute)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var attributeCount = gisService.Get(collectionId).Attributes.Count;
            gisService.AddAttribute(collectionId, attribute);
            var updatedCount = gisService.Get(collectionId).Attributes.Count;
            Assert.Equal(attributeCount + 1, updatedCount);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var attribute = gisService.Get(collectionId).Attributes.First();
            var length = attribute.Length;
            attribute.Length = length + 1;
            gisService.UpdateAttribute(collectionId, attribute);
            attribute = gisService.Get(collectionId).Attributes.First();
            Assert.Equal(length + 1, attribute.Length);
        }

        [Theory, AutoGisData]
        public void RemoveAttributeIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var attributeCount = gisService.Get(collectionId).Attributes.Count;
            gisService.RemoveAttribute(collectionId, 0);
            var updatedCount = gisService.Get(collectionId).Attributes.Count;
            Assert.Equal(attributeCount - 1, updatedCount);
        }

        [Theory, AutoGisData]
        public void GetAttributeIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var attributeIndex = 0;
            var collectionId = gisService.GetIds().ToList()[0];
            var name = gisService.Get(collectionId).Attributes[attributeIndex].Name;
            var attribute = gisService.GetAttribute(collectionId, attributeIndex);
            Assert.Equal(name, attribute.Name);
        }

        [Theory, AutoGisData]
        public void GetAttributeByNameIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var attributeName = gisService.Get(collectionId).Attributes.Last().Name;
            var attribute = gisService.GetAttribute(collectionId, attributeName);
            Assert.Equal(attributeName, attribute.Name);
        }
        
        [Theory, AutoGisData]
        public void GetAttributeIndexByNameIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var attributes = gisService.Get(collectionId).Attributes;
            var attributeName = attributes.Last().Name;
            var attributeIndex = gisService.GetAttributeIndexByName(collectionId, attributeName);
            Assert.Equal(attributes.Count - 1, attributeIndex);
        }

        [Theory, AutoGisData]
        public void GetAttributeByNonExistingNameThrows(UpdatableGisService<Guid, string> gisService, string attributeName)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.GetAttribute(collectionId, attributeName));
        }

        [Theory, AutoGisData]
        public void GetAttributeIndexByNonExistingNameThrows(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attributeIndex = featureCollection.Attributes.Count + 1;
            Assert.Throws<KeyNotFoundException>(() => gisService.GetAttribute(collectionId, attributeIndex));
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValueByIdIsOk(UpdatableGisService<Guid, string> gisService, int value)
        {
            var attributeIndex = 1;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            gisService.UpdateAttributeValue(collectionId, featureId, attributeIndex, value);
            var feature = gisService.GetFeature(collectionId, featureId);
            var attribute = gisService.GetAttribute(collectionId, attributeIndex);
            Assert.Equal(value, feature.AttributeValues[attribute.Name]);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValueByIdsIsOk(UpdatableGisService<Guid, string> gisService, int value)
        {
            var attributeIndex = 1;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureIds = gisService.Get(collectionId).Features.Select(f => (string)f.AttributeValues["id"]).ToList();
            gisService.UpdateAttributeValue(collectionId, featureIds, attributeIndex, value);
            var attribute = gisService.GetAttribute(collectionId, attributeIndex);
            var attributeName = attribute.Name;
            var attributeValues = gisService.Get(collectionId).Features.Select(f => f.AttributeValues[attributeName]).ToList();
            Assert.All(attributeValues, v => Assert.Equal(value, v));
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValueByFilterIsOk(UpdatableGisService<Guid, string> gisService, int value)
        {
            var attributeIndex = 1;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            var filter = new List<QueryCondition>() { new QueryCondition("id", featureId) };
            gisService.UpdateAttributeValue(collectionId, filter, attributeIndex, value);
            var feature = gisService.GetFeature(collectionId, featureId);
            var attribute = gisService.GetAttribute(collectionId, attributeIndex);
            var attributeName = attribute.Name;
            Assert.Equal(value, feature.AttributeValues[attribute.Name]);
        }

        [Theory, AutoGisData]
        public void GetAttributeByNonExistentNameThrows(UpdatableGisService<Guid, string> gisService, string attributeName)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.GetAttribute(collectionId, attributeName));
        }

        [Theory, AutoGisData]
        public void RemoveNonExistentAttributeByNameThrows(UpdatableGisService<Guid, string> gisService, string attributeName)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            Assert.Throws<KeyNotFoundException>(() => gisService.RemoveAttribute(collectionId, attributeName));
        }

        [Theory, AutoGisData]
        public void RemoveAttributeByNameIsOk(UpdatableGisService<Guid, string> gisService)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attributeName = featureCollection.Attributes.Last().Name;
            gisService.RemoveAttribute(collectionId, attributeName);
            var updatedFeatureCollection = gisService.Get(collectionId);
            Assert.DoesNotContain(attributeName, updatedFeatureCollection.Attributes.Select(a => a.Name));
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValueByNameIsOk(UpdatableGisService<Guid, string> gisService, int value)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var featureId = (string)featureCollection.Features[0].AttributeValues["id"];
            var attributeName = featureCollection.Attributes.Last().Name;
            gisService.UpdateAttributeValue(collectionId, featureId, attributeName, value);
            var feature = gisService.GetFeature(collectionId, featureId);
            Assert.Equal(value, feature.AttributeValues[attributeName]);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValueByNameByIdsIsOk(UpdatableGisService<Guid, string> gisService, int value)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attributeName = featureCollection.Attributes.Last().Name;
            var featureIds = featureCollection.Features.Select(f => (string)f.AttributeValues["id"]).ToList();
            gisService.UpdateAttributeValue(collectionId, featureIds, attributeName, value);
            var attributeValues = gisService.Get(collectionId).Features.Select(f => f.AttributeValues[attributeName]).ToList();
            Assert.All(attributeValues, v => Assert.Equal(value, v));
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValueByNameAndFilter(UpdatableGisService<Guid, string> gisService, int value)
        {
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attributeName = featureCollection.Attributes.Last().Name;
            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            var filter = new List<QueryCondition>() { new QueryCondition("id", featureId) };
            gisService.UpdateAttributeValue(collectionId, filter, attributeName, value);
            var feature = gisService.GetFeature(collectionId, featureId);
            Assert.Equal(value, feature.AttributeValues[attributeName]);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValuesIsOk(UpdatableGisService<Guid, string> gisService, int value1, string value2)
        {
            var attribute1 = 1;
            var attribute2 = 2;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attribute1Name = featureCollection.Attributes[attribute1].Name;
            var attribute2Name = featureCollection.Attributes[attribute2].Name;
            var attributes = new Dictionary<int, object>() { { attribute1, value1 }, { attribute2, value2 } };

            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            gisService.UpdateAttributeValues(collectionId, featureId, attributes);
            var feature = gisService.GetFeature(collectionId, featureId);
            Assert.Equal(value1, feature.AttributeValues[attribute1Name]);
            Assert.Equal(value2, feature.AttributeValues[attribute2Name]);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValuesByIdsManyIsOk(UpdatableGisService<Guid, string> gisService, int value1, string value2)
        {
            var attribute1 = 1;
            var attribute2 = 2;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attribute1Name = featureCollection.Attributes[attribute1].Name;
            var attribute2Name = featureCollection.Attributes[attribute2].Name;
            var attributes = new Dictionary<int, object>() { { attribute1, value1 }, { attribute2, value2 } };

            var featureIds = featureCollection.Features.Select(f => (string)f.AttributeValues["id"]).ToList();
            gisService.UpdateAttributeValues(collectionId, featureIds, attributes);
            var features = gisService.Get(collectionId).Features;
            Assert.All(features.Select(f => f.AttributeValues[attribute1Name]).ToList(), v => Assert.Equal(value1, v));
            Assert.All(features.Select(f => f.AttributeValues[attribute2Name]).ToList(), v => Assert.Equal(value2, v));
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValuesWhereIsOk(UpdatableGisService<Guid, string> gisService, int value1, string value2)
        {
            var attribute1 = 1;
            var attribute2 = 2;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attribute1Name = featureCollection.Attributes[attribute1].Name;
            var attribute2Name = featureCollection.Attributes[attribute2].Name;
            var attributes = new Dictionary<int, object>() { { attribute1, value1 }, { attribute2, value2 } };

            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            var filter = new List<QueryCondition>() { new QueryCondition("id", featureId) };
            gisService.UpdateAttributeValues(collectionId, filter, attributes);
            var feature = gisService.GetFeature(collectionId, featureId);
            Assert.Equal(value1, feature.AttributeValues[attribute1Name]);
            Assert.Equal(value2, feature.AttributeValues[attribute2Name]);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValuesByNameIsOk(UpdatableGisService<Guid, string> gisService, int value1, string value2)
        {
            var attribute1 = 1;
            var attribute2 = 2;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attribute1Name = featureCollection.Attributes[attribute1].Name;
            var attribute2Name = featureCollection.Attributes[attribute2].Name;
            var attributes = new Dictionary<string, object>() { { attribute1Name, value1 }, { attribute2Name, value2 } };

            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            gisService.UpdateAttributeValues(collectionId, featureId, attributes);
            var feature = gisService.GetFeature(collectionId, featureId);
            Assert.Equal(value1, feature.AttributeValues[attribute1Name]);
            Assert.Equal(value2, feature.AttributeValues[attribute2Name]);
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValuesByNameByIdsIsOk(UpdatableGisService<Guid, string> gisService, int value1, string value2)
        {
            var attribute1 = 1;
            var attribute2 = 2;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attribute1Name = featureCollection.Attributes[attribute1].Name;
            var attribute2Name = featureCollection.Attributes[attribute2].Name;
            var attributes = new Dictionary<string, object>() { { attribute1Name, value1 }, { attribute2Name, value2 } };

            var featureIds = featureCollection.Features.Select(f => (string)f.AttributeValues["id"]).ToList();
            gisService.UpdateAttributeValues(collectionId, featureIds, attributes);
            var features = gisService.Get(collectionId).Features;
            Assert.All(features.Select(f => f.AttributeValues[attribute1Name]).ToList(), v => Assert.Equal(value1, v));
            Assert.All(features.Select(f => f.AttributeValues[attribute2Name]).ToList(), v => Assert.Equal(value2, v));
        }

        [Theory, AutoGisData]
        public void UpdateAttributeValuesByNameWhereIsOk(UpdatableGisService<Guid, string> gisService, int value1, string value2)
        {
            var attribute1 = 1;
            var attribute2 = 2;
            var collectionId = gisService.GetIds().ToList()[0];
            var featureCollection = gisService.Get(collectionId);
            var attribute1Name = featureCollection.Attributes[attribute1].Name;
            var attribute2Name = featureCollection.Attributes[attribute2].Name;
            var attributes = new Dictionary<string, object>() { { attribute1Name, value1 }, { attribute2Name, value2 } };

            var featureId = (string)gisService.Get(collectionId).Features[0].AttributeValues["id"];
            var filter = new List<QueryCondition>() { new QueryCondition("id", featureId) };
            gisService.UpdateAttributeValues(collectionId, filter, attributes);
            var feature = gisService.GetFeature(collectionId, featureId);
            Assert.Equal(value1, feature.AttributeValues[attribute1Name]);
            Assert.Equal(value2, feature.AttributeValues[attribute2Name]);
        }
    }
}