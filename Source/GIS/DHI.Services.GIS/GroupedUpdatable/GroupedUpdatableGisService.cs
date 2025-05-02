namespace DHI.Services.GIS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using Spatial;

    public class GroupedUpdatableGisService<TCollectionId, TFeatureId> : BaseGroupedUpdatableDiscreteService<FeatureCollection<TCollectionId>, TCollectionId>,
        IGroupedUpdatableGisService<TCollectionId, TFeatureId>
    {
        private readonly UpdatableGisService<TCollectionId, TFeatureId> _gisService;
        private readonly IGroupedGisRepository<TCollectionId> _gisRepository;

        public GroupedUpdatableGisService(IGroupedUpdatableGisRepository<TCollectionId, TFeatureId> repository)
            : base(repository)
        {
            _gisService = new UpdatableGisService<TCollectionId, TFeatureId>(repository);
            _gisRepository = repository;
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IGroupedUpdatableGisRepository<TCollectionId, TFeatureId>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IGroupedUpdatableGisRepository<TCollectionId, TFeatureId>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IGroupedUpdatableGisRepository<TCollectionId, TFeatureId>>(path, searchPattern);
        }

        public FeatureCollection<TCollectionId> Get(TCollectionId id, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(id, associations, outSpatialReference, user);
        }

        public FeatureCollectionInfo<TCollectionId> GetInfo(TCollectionId id, ClaimsPrincipal user = null)
        {
            return _gisService.GetInfo(id, user);
        }

        public IEnumerable<FeatureCollectionInfo<TCollectionId>> GetInfo(string group, ClaimsPrincipal user = null)
        {
            return _gisRepository.GetInfoByGroup(group, user);
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

        public IEnumerable<TCollectionId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            return _gisService.GetIds(filter, user);
        }

        public void AddFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null)
        {
            _gisService.AddFeature(collectionId, feature, user);
        }

        public void UpdateFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null)
        {
            _gisService.UpdateFeature(collectionId, feature, user);
        }

        public void RemoveFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null)
        {
            _gisService.RemoveFeature(collectionId, featureId, user);
        }

        public void RemoveFeatures(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, ClaimsPrincipal user = null)
        {
            _gisService.RemoveFeatures(collectionId, featureIds, user);
        }

        public IEnumerable<TFeatureId> GetFeatureIds(TCollectionId collectionId, IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            return _gisService.GetFeatureIds(collectionId, filter, user);
        }

        public IFeature GetFeature(TCollectionId collectionId, TFeatureId featureId, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetFeature(collectionId, featureId, associations, outSpatialReference, user);
        }
        public FeatureInfo GetFeatureInfo(TCollectionId collectionId, TFeatureId featureId, bool associations = false, ClaimsPrincipal user = null)
        {
            return _gisService.GetFeatureInfo(collectionId, featureId, associations, user);
        }
        public void AddAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null)
        {
            _gisService.AddAttribute(collectionId, attribute, user);
        }

        public void UpdateAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttribute(collectionId, attribute, user);
        }

        public void RemoveAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
            _gisService.RemoveAttribute(collectionId, attributeIndex, user);
        }

        public void RemoveAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            _gisService.RemoveAttribute(collectionId, attributeName, user);
        }

        public IAttribute GetAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
            return _gisService.GetAttribute(collectionId, attributeIndex, user);
        }

        public IAttribute GetAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            return _gisService.GetAttribute(collectionId, attributeName, user);
        }

        public int GetAttributeIndexByName(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            return _gisService.GetAttributeIndexByName(collectionId, attributeName, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValue(collectionId, featureId, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValue(collectionId, featureIds, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValue(collectionId, filter, attributeIndex, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, string attributeName, object value, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValue(collectionId, featureId, attributeName, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, string attributeName, object value, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValue(collectionId, featureIds, attributeName, value, user);
        }

        public void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, string attributeName, object value, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValue(collectionId, filter, attributeName, value, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValues(collectionId, featureId, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValues(collectionId, featureIds, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<string, object> attributes, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValues(collectionId, filter, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValues(collectionId, featureId, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValues(collectionId, featureIds, attributes, user);
        }

        public void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            _gisService.UpdateAttributeValues(collectionId, filter, attributes, user);
        }

        public IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometryTypes(user);
        }
        
        public (Stream, string fileType, string fileName) GetStream(TCollectionId id, ClaimsPrincipal user = null)
        {
            var (maybe, fileType, fileName) = _gisRepository.GetStream(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Feature collection with id '{id}' was not found.");
            }

            return (maybe.Value, fileType, fileName);
        }

        public IDictionary<string, string[]> GetGeometryTypes(string group, ClaimsPrincipal user = null)
        {
            return _gisRepository.GetGeometryTypes(group, user);
        }

        public void AddAssociation(TCollectionId collectionId, TFeatureId featureId, IAssociation association, ClaimsPrincipal user = null)
        {
            _gisService.AddAssociation(collectionId, featureId, association, user);
        }

        public void RemoveAssociation(TCollectionId collectionId, TFeatureId featureId, IAssociation association, ClaimsPrincipal user = null)
        {
            _gisService.RemoveAssociation(collectionId, featureId, association, user);
        }
    }

    public class GroupedUpdatableGisService : GroupedUpdatableGisService<string, Guid>
    {
        public GroupedUpdatableGisService(IGroupedUpdatableGisRepository<string, Guid> repository) : base(repository)
        {
        }
    }
}