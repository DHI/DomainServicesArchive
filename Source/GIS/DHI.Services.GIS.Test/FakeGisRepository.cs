namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using GIS;
    using Spatial;

    public class FakeGisRepository : BaseGroupedGisRepository<Guid>, IGroupedUpdatableGisRepository<Guid, string>
    {
        private readonly Dictionary<Guid, FeatureCollection<Guid>> _featureCollectionList = new Dictionary<Guid, FeatureCollection<Guid>>();

        public FakeGisRepository(List<FeatureCollection<Guid>> featureCollectionList)
        {
            foreach (var featureCollection in featureCollectionList)
            {
                _featureCollectionList.Add(featureCollection.Id, featureCollection);
            }
        }

        public override Maybe<FeatureCollection<Guid>> Get(Guid id, IEnumerable<QueryCondition> filter, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, associations);

            if (!maybe.HasValue) {
                return Maybe.Empty<FeatureCollection<Guid>>();
            }

            var featureCollection = maybe.Value;
            
            var queryable = featureCollection.Features.AsQueryable();
            foreach (var condition in filter) {
                queryable = queryable.Where(f => f.AttributeValues[condition.Item] == condition.Value);
            }
            var features = queryable.Select(i => i).ToList();

            return new FeatureCollection<Guid>(featureCollection.Id, featureCollection.Name, features).ToMaybe();
        }

        public override Maybe<FeatureCollection<Guid>> Get(Guid id, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return Get(id);
        }

        public override IEnumerable<FeatureCollection<Guid>> GetAll(ClaimsPrincipal user = null)
        {
            return _featureCollectionList.Values.ToList();
        }

        public override (Maybe<Stream>, string fileType, string fileName) GetStream(Guid id, ClaimsPrincipal user = null)
        {
            var maybe = Get(id);
            var stream = maybe.HasValue ? new MemoryStream().ToMaybe<Stream>() : Maybe.Empty<Stream>();
            return (stream, string.Empty, string.Empty);
        }

        public void Add(FeatureCollection<Guid> featureCollection, ClaimsPrincipal user = null)
        {
            _featureCollectionList[featureCollection.Id] = featureCollection;
        }

        public void Remove(Guid id, ClaimsPrincipal user = null)
        {
            _featureCollectionList.Remove(id);
        }

        public void Update(FeatureCollection<Guid> featureCollection, ClaimsPrincipal user = null)
        {
            _featureCollectionList[featureCollection.Id] = featureCollection;
        }

        public void AddFeature(Guid collectionId, IFeature feature, ClaimsPrincipal user = null)
        {
            _featureCollectionList[collectionId].Features.Add(feature);
        }

        public void UpdateFeature(Guid collectionId, IFeature updatedFeature, ClaimsPrincipal user = null)
        {
            var id = updatedFeature.AttributeValues["id"];
            var feature = _featureCollectionList[collectionId].Features.Single(f => f.AttributeValues["id"] == id);
            var index = _featureCollectionList[collectionId].Features.IndexOf(feature);
            _featureCollectionList[collectionId].Features[index] = updatedFeature;
        }

        public void RemoveFeature(Guid collectionId, string featureId, ClaimsPrincipal user = null)
        {
            var feature = _featureCollectionList[collectionId].Features.Single(f => (string)f.AttributeValues["id"] == featureId);
            _featureCollectionList[collectionId].Features.Remove(feature);
        }

        public void RemoveFeatures(Guid collectionId, IEnumerable<string> featureIds, ClaimsPrincipal user = null)
        {
            foreach (var fId in featureIds) {
                RemoveFeature(collectionId, fId);
            }
        }

        public bool ContainsFeature(Guid collectionId, string featureId, ClaimsPrincipal user = null)
        {
            return _featureCollectionList[collectionId].Features.Any(f => (string)f.AttributeValues["id"] == featureId);
        }

        public override Maybe<IGeometry> GetEnvelope(Guid id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return Maybe.Empty<IGeometry>();
        }

        public override Maybe<IGeometry> GetFootprint(Guid id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            return Maybe.Empty<IGeometry>();
        }

        public override Maybe<IGeometry> GetFootprint(IEnumerable<Guid> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            return Maybe.Empty<IGeometry>();
        }

        public override IEnumerable<Guid> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            var query = new Query<FeatureCollection<Guid>>(filter);
            var collections = GetAll();
            return collections.AsQueryable().Where(query.ToExpression()).Select(f => f.Id);
        }

        public IEnumerable<string> GetFeatureIds(Guid collectionId, IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            var query = new Query<IFeature>(filter);
            var features = Get(collectionId, filter, false).Value.Features.AsQueryable();
            var ids = features.Select(f => f.AttributeValues["id"].ToString());
            return ids;
        }

        public Maybe<IFeature> GetFeature(Guid collectionId, string featureId, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = Get(collectionId, false, outSpatialReference);
            var collection = maybe.HasValue ? maybe.Value : null;
            if (collection == null)
            {
                return Maybe.Empty<IFeature>();
            }

            var features = collection.Features.AsQueryable();
            var feature = features.Where(f => f.AttributeValues["id"].ToString() == featureId)
                .Select(x => x).FirstOrDefault();
            return feature == null ? Maybe.Empty<IFeature>() : feature.ToMaybe();
        }

        public Maybe<FeatureInfo> GetFeatureInfo(Guid collectionId, string featureId, bool associations = false, ClaimsPrincipal user = null)
        {
            var maybe = Get(collectionId, false, null);
            var collection = maybe.HasValue ? maybe.Value : null;
            if (collection == null)
            {
                return Maybe.Empty<FeatureInfo>();
            }

            var features = collection.Features.AsQueryable();
            var feature = features.Where(f => f.AttributeValues["id"].ToString() == featureId)
                .Select(x => x).FirstOrDefault();
            if (feature != null) {
                var featureInfo = new FeatureInfo();
                foreach (var attribute in feature.AttributeValues) { featureInfo.AttributeValues.Add(attribute.Key, attribute.Value); }
                foreach (var association in feature.Associations) { featureInfo.Associations.Add(association); }
                return featureInfo.ToMaybe();
            }
            return Maybe.Empty<FeatureInfo>();
        }

        public override bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            return _featureCollectionList.Any(e => e.Value.Group == group);
        }

        public override IEnumerable<FeatureCollection<Guid>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            return _featureCollectionList.Where(f => f.Value.Group == group).Select(f => f.Value).ToList();
        }

        public override IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group).Select(f => f.FullName).ToList();
        }

        public override IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return _featureCollectionList.Select(f => f.Value.FullName).ToList();
        }

        public void AddAttribute(Guid collectionId, IAttribute attribute, ClaimsPrincipal user = null)
        {
            _featureCollectionList[collectionId].Attributes.Add(attribute);
        }

        public void UpdateAttribute(Guid collectionId, IAttribute attribute, ClaimsPrincipal user = null)
        {
            var index = _featureCollectionList[collectionId].Attributes.IndexOf(attribute);
            _featureCollectionList[collectionId].Attributes[index] = attribute;
        }

        public void RemoveAttribute(Guid collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
                _featureCollectionList[collectionId].Attributes.RemoveAt(attributeIndex);
        }

        public Maybe<IAttribute> GetAttribute(Guid collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
            var attributes = _featureCollectionList[collectionId].Attributes;
            return attributeIndex >= attributes.Count ? Maybe.Empty<IAttribute>() : attributes[attributeIndex].ToMaybe();
        }

        public Maybe<IAttribute> GetAttribute(Guid collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            var attribute = _featureCollectionList[collectionId].Attributes.Where(a => a.Name == attributeName).FirstOrDefault();
            return attribute == null ? Maybe.Empty<IAttribute>() : attribute.ToMaybe();
        }

        public virtual int GetAttributeIndexByName(Guid collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            return Get(collectionId).Value.Attributes.ToList().FindIndex(a => a.Name == attributeName);
        }

        public void UpdateAttributeValue(Guid collectionId, string featureId, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var featureIds = new List<string>() { featureId };
            UpdateAttributeValue(collectionId, featureIds, attributeIndex, value);
        }

        public void UpdateAttributeValue(Guid collectionId, IEnumerable<string> featureIds, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var maybe = Get(collectionId);
            var collection = maybe.Value;
            var attributeName = collection.Attributes[attributeIndex].Name;
            foreach(var feature in collection.Features)
            {
                var id = feature.AttributeValues["id"];
                if (featureIds.Contains(id)) {
                    feature.AttributeValues[attributeName] = value;
                }
            }
            Update(collection);
        }

        public void UpdateAttributeValue(Guid collectionId, IEnumerable<QueryCondition> filter, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var ids = GetFeatureIds(collectionId, filter);
            UpdateAttributeValue(collectionId, ids, attributeIndex, value);
        }

        public void UpdateAttributeValues(Guid collectionId, string featureId, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            var ids = new List<string>() { featureId };
            UpdateAttributeValues(collectionId, ids, attributes);
        }

        public void UpdateAttributeValues(Guid collectionId, IEnumerable<string> featureIds, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            var maybe = Get(collectionId);
            var collection = maybe.Value;
            
            foreach (var feature in collection.Features)
            {
                var id = feature.AttributeValues["id"];
                if (featureIds.Contains(id))
                {
                    foreach (var attributeIndex in attributes.Keys) {
                        var attributeName = collection.Attributes[attributeIndex].Name;
                        feature.AttributeValues[attributeName] = attributes[attributeIndex];
                    }
                }
            }
            Update(collection);
        }

        public void UpdateAttributeValues(Guid collectionId, IEnumerable<QueryCondition> filter, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            var ids = GetFeatureIds(collectionId, filter);
            UpdateAttributeValues(collectionId, ids, attributes);
        }
    }
}