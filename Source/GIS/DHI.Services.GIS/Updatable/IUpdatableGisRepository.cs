namespace DHI.Services.GIS
{
    using Spatial;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IUpdatableGisRepository<TCollectionId, TFeatureId> : IGisRepository<TCollectionId>,
        IUpdatableRepository<FeatureCollection<TCollectionId>, TCollectionId>
    {
        void AddFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null);

        int GetAttributeIndexByName(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null);

        void UpdateFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null);

        void RemoveFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null);

        void RemoveFeatures(TCollectionId collectionId, IEnumerable<TFeatureId> featureId, ClaimsPrincipal user = null);

        bool ContainsFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null);
        
        IEnumerable<TFeatureId> GetFeatureIds(TCollectionId collectionId, IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null);

        Maybe<IFeature> GetFeature(TCollectionId collectionId, TFeatureId featureId, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null);
        
        Maybe<FeatureInfo> GetFeatureInfo(TCollectionId collectionId, TFeatureId featureId, bool associations = false, ClaimsPrincipal user = null);
        
        void AddAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null);

        void UpdateAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null);

        void RemoveAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null);

        Maybe<IAttribute> GetAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null);

        Maybe<IAttribute> GetAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, int attributeIndex, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, int attributeIndex, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, int attributeIndex, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

    }
}