namespace DHI.Services.GIS
{
    using Spatial;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IUpdatableGisService<TCollectionId, TFeatureId> : IGisService<TCollectionId>, IUpdatableService<FeatureCollection<TCollectionId>, TCollectionId>
    {
        void AddFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null);

        void UpdateFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null);

        void RemoveFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null);
        
        void RemoveFeatures(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, ClaimsPrincipal user = null);

        IEnumerable<TFeatureId> GetFeatureIds(TCollectionId collectionId, IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null);

        IFeature GetFeature(TCollectionId collectionId, TFeatureId featureId, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null);
        FeatureInfo GetFeatureInfo(TCollectionId collectionId, TFeatureId featureId, bool associations = false, ClaimsPrincipal user = null);

        void AddAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null);

        void UpdateAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null);

        void RemoveAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null);

        void RemoveAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null);

        IAttribute GetAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null);

        IAttribute GetAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null);

        int GetAttributeIndexByName(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, int attributeIndex, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, int attributeIndex, object value, ClaimsPrincipal user = null);
        
        void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, int attributeIndex, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, string attributeName, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, string attributeName, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, string attributeName, object value, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<string, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<string, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<string, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

        void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

        void AddAssociation(TCollectionId collectionId, TFeatureId featureId, IAssociation association, ClaimsPrincipal user = null);
        
        void RemoveAssociation(TCollectionId collectionId, TFeatureId featureId, IAssociation association, ClaimsPrincipal user = null);

    }
}