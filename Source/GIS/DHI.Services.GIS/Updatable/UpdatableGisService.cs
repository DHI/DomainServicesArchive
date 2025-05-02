namespace DHI.Services.GIS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Spatial;

    public class UpdatableGisService<TCollectionId, TFeatureId> : BaseUpdatableDiscreteService<FeatureCollection<TCollectionId>, TCollectionId>, IUpdatableGisService<TCollectionId, TFeatureId>
    {
        private readonly GisService<TCollectionId> _gisService;
        private readonly IUpdatableGisRepository<TCollectionId, TFeatureId> _repository;

        public UpdatableGisService(IUpdatableGisRepository<TCollectionId, TFeatureId> repository) : base(repository)
        {
            _repository = repository;
            _gisService = new GisService<TCollectionId>(repository);
        }

        public FeatureCollection<TCollectionId> Get(TCollectionId id, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(id, associations, outSpatialReference, user);
        }

        public FeatureCollectionInfo<TCollectionId> GetInfo(TCollectionId id, ClaimsPrincipal user = null)
        {
            return _gisService.GetInfo(id, user);
        }

        public FeatureCollection<TCollectionId> Get(TCollectionId id, IEnumerable<QueryCondition> filter, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(id, filter, associations, outSpatialReference, user);
        }

        public IDictionary<TCollectionId, FeatureCollection<TCollectionId>> Get(TCollectionId[] ids, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(ids, associations, outSpatialReference, user);
        }

        public GeometryCollection GetGeometry(TCollectionId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometry(id, outSpatialReference, user);
        }

        public GeometryCollection GetGeometry(TCollectionId id, IEnumerable<QueryCondition> filter, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometry(id, filter, outSpatialReference, user);
        }

        public void AddFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            var featureId = _GetIdFromAttributes(feature);
            if (_repository.ContainsFeature(collectionId, featureId, user))
            {
                throw new ArgumentException($"The feature with id '{featureId}' in collection with id '{collectionId}' already exists.");
            }

            _repository.AddFeature(collectionId, feature, user);
        }

        public void UpdateFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            if (!feature.AttributeValues.ContainsKey("id"))
            {
                throw new ArgumentException("The given feature does not contain the mandatory 'id' attribute.", nameof(feature));
            }

            var featureId = _GetIdFromAttributes(feature);
            if (!_repository.ContainsFeature(collectionId, featureId, user))
            {
                throw new KeyNotFoundException($"The feature with id '{featureId}' in collection with id '{collectionId}' was not found.");
            }

            _repository.UpdateFeature(collectionId, feature, user);
        }

        public void RemoveFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            if (!_repository.ContainsFeature(collectionId, featureId, user))
            {
                throw new KeyNotFoundException($"The feature with id '{featureId}' in collection with id '{collectionId}' was not found.");
            }

            _repository.RemoveFeature(collectionId, featureId, user);
        }

        public void RemoveFeatures(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            _repository.RemoveFeatures(collectionId, featureIds, user);
        }

        public IGeometry GetEnvelope(TCollectionId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetEnvelope(id, outSpatialReference, user);
        }

        public IGeometry GetFootprint(TCollectionId id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetFootprint(id, outSpatialReference, simplifyDistance, user);
        }

        public IGeometry GetFootprint(IEnumerable<TCollectionId> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetFootprint(ids, outSpatialReference, simplifyDistance, user);
        }

        public IEnumerable<TFeatureId> GetFeatureIds(TCollectionId collectionId, IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            return _repository.GetFeatureIds(collectionId, filter, user);
        }

        public IFeature GetFeature(TCollectionId collectionId, TFeatureId featureId, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetFeature(collectionId, featureId, associations, outSpatialReference, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The feature with id '{featureId}' in feature collection with id '{collectionId}' was not found.");
            }
            return maybe.Value;
        }

        public FeatureInfo GetFeatureInfo(TCollectionId collectionId, TFeatureId featureId, bool associations = false, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetFeatureInfo(collectionId, featureId, associations, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The feature with id '{featureId}' in feature collection with id '{collectionId}' was not found.");
            }
            return maybe.Value;
        }

        public (Stream, string fileType, string fileName) GetStream(TCollectionId id, ClaimsPrincipal user = null)
        {
            var (maybe, fileType, fileName) = _repository.GetStream(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Feature collection with id '{id}' was not found.");
            }

            return (maybe.Value, fileType, fileName);
        }

        public IEnumerable<TCollectionId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            return _gisService.GetIds(filter, user);
        }

        public IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometryTypes(user);
        }

        public void AddAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null)
        {
            _repository.AddAttribute(collectionId, attribute, user);
        }

        public void UpdateAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null)
        {
            _repository.UpdateAttribute(collectionId, attribute, user);
        }

        public void RemoveAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
            _repository.RemoveAttribute(collectionId, attributeIndex, user);
        }

        public void RemoveAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            var attributeIndex = _GetAttributeIndexByNameAndThrowIfNotFound(collectionId, attributeName, user);
            _repository.RemoveAttribute(collectionId, attributeIndex, user);
        }

        public IAttribute GetAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetAttribute(collectionId, attributeIndex, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Attribute {attributeIndex} in feature collection '{collectionId}' was not found.");
            }
            return maybe.Value;
        }

        public IAttribute GetAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetAttribute(collectionId, attributeName, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Attribute {attributeName} in feature collection '{collectionId}' was not found.");
            }
            return maybe.Value;
        }

        public void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            _repository.UpdateAttributeValue(collectionId, featureId, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var attribute = GetAttribute(collectionId, attributeIndex, user);
            _repository.UpdateAttributeValue(collectionId, featureIds, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var attribute = GetAttribute(collectionId, attributeIndex, user);
            _repository.UpdateAttributeValue(collectionId, filter, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, string attributeName, object value, ClaimsPrincipal user = null)
        {
            var attributeIndex = _GetAttributeIndexByNameAndThrowIfNotFound(collectionId, attributeName, user);
            UpdateAttributeValue(collectionId, featureId, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, string attributeName, object value, ClaimsPrincipal user = null)
        {
            var attributeIndex = _GetAttributeIndexByNameAndThrowIfNotFound(collectionId, attributeName, user);
            UpdateAttributeValue(collectionId, featureIds, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, string attributeName, object value, ClaimsPrincipal user = null)
        {
            var attributeIndex = _GetAttributeIndexByNameAndThrowIfNotFound(collectionId, attributeName, user);
            UpdateAttributeValue(collectionId, filter, attributeIndex, value, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            var attributeIndexesAndValues = _AttributeNamesAndValuesToAttributeIndexesAndValues(collectionId, attributes);
            UpdateAttributeValues(collectionId, featureId, attributeIndexesAndValues, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            var attributeIndexesAndValues = _AttributeNamesAndValuesToAttributeIndexesAndValues(collectionId, attributes);
            UpdateAttributeValues(collectionId, featureIds, attributeIndexesAndValues, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            var attributeIndexesAndValues = _AttributeNamesAndValuesToAttributeIndexesAndValues(collectionId, attributes);
            UpdateAttributeValues(collectionId, filter, attributeIndexesAndValues, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            var featureIds = new List<TFeatureId> {featureId};
            UpdateAttributeValues(collectionId, featureIds, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            _repository.UpdateAttributeValues(collectionId, featureIds, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            _repository.UpdateAttributeValues(collectionId, filter, attributes, user);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IUpdatableGisRepository<TCollectionId, TFeatureId>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IUpdatableGisRepository<TCollectionId, TFeatureId>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IUpdatableGisRepository<TCollectionId, TFeatureId>>(path, searchPattern);
        }

        private void _ThrowExceptionIfInvalidCollectionId(TCollectionId collectionId)
        {
            if (!Exists(collectionId))
            {
                throw new KeyNotFoundException($"The feature collection with id '{collectionId}' was not found.");
            }
        }

        // TODO: This should be moved into repository in the PostgreSQL provider
        private TFeatureId _GetIdFromAttributes(IFeature feature)
        {
            if (!feature.AttributeValues.Keys.Contains("id"))
            {
                throw new ArgumentException();
            }
            var id = feature.AttributeValues["id"];
            if (typeof(TFeatureId) == typeof(Guid))
            {
                object guid = new Guid(id.ToString());
                return (TFeatureId)guid;
            }

            return (TFeatureId)feature.AttributeValues["id"];
        }

        public int GetAttributeIndexByName(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            return _repository.GetAttributeIndexByName(collectionId, attributeName, user);
        }

        public void AddAssociation(TCollectionId collectionId, TFeatureId featureId, IAssociation association, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            var feature = GetFeature(collectionId, featureId, true, user: user);
            feature.Associations.Add(association);
            _repository.UpdateFeature(collectionId, feature, user);

        }

        public void RemoveAssociation(TCollectionId collectionId, TFeatureId featureId, IAssociation association, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidCollectionId(collectionId);
            var feature = GetFeature(collectionId, featureId, true, user: user);
            feature.Associations.Remove(association);
            _repository.UpdateFeature(collectionId, feature, user);
        }

        private int _GetAttributeIndexByNameAndThrowIfNotFound(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            var attributeIndex = GetAttributeIndexByName(collectionId, attributeName, user);
            if (attributeIndex < 0)
            {
                throw new KeyNotFoundException($"Attribute {attributeName} not found in feature collection {collectionId}");
            }

            return attributeIndex;
        }

        private IDictionary<int, object> _AttributeNamesAndValuesToAttributeIndexesAndValues(TCollectionId collectionId, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            var attributeIndexesAndValues = new Dictionary<int, object>();
            foreach (var attributeName in attributes.Keys)
            {
                var attributeIndex = _GetAttributeIndexByNameAndThrowIfNotFound(collectionId, attributeName, user);
                attributeIndexesAndValues.Add(attributeIndex, attributes[attributeName]);
            }

            return attributeIndexesAndValues;
        }
    }

    public class UpdatableGisService : UpdatableGisService<string, Guid>
    {
        public UpdatableGisService(IUpdatableGisRepository<string, Guid> repository) : base(repository)
        {
        }
    }
}