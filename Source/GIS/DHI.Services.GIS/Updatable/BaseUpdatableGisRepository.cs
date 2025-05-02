namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using Spatial;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseUpdatableGisRepository<TCollectionId, TFeatureId> : BaseGisRepository<TCollectionId>, IUpdatableGisRepository<TCollectionId, TFeatureId>
    {
        public abstract void Add(FeatureCollection<TCollectionId> entity, ClaimsPrincipal user = null);

        public abstract void Remove(TCollectionId id, ClaimsPrincipal user = null);

        public abstract void Update(FeatureCollection<TCollectionId> entity, ClaimsPrincipal user = null);

        public abstract void AddFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null);

        public abstract void UpdateFeature(TCollectionId collectionId, IFeature feature, ClaimsPrincipal user = null);

        public abstract void RemoveFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null);

        public virtual void RemoveFeatures(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, ClaimsPrincipal user = null)
        {
            if (featureIds != null) {
                foreach(var fId in featureIds)
                {
                    if(ContainsFeature(collectionId, fId))
                    {
                        RemoveFeature(collectionId, fId);
                    }
                }
            }
        }

        public abstract bool ContainsFeature(TCollectionId collectionId, TFeatureId featureId, ClaimsPrincipal user = null);

        public abstract IEnumerable<TFeatureId> GetFeatureIds(TCollectionId collectionId, IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null);

        public abstract Maybe<IFeature> GetFeature(TCollectionId collectionId, TFeatureId featureId, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null);

        public abstract Maybe<FeatureInfo> GetFeatureInfo(TCollectionId collectionId, TFeatureId featureId, bool associations = false, ClaimsPrincipal user = null);
        
        public abstract void AddAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null);

        public abstract void UpdateAttribute(TCollectionId collectionId, IAttribute attribute, ClaimsPrincipal user = null);

        public abstract void RemoveAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null);

        public virtual int GetAttributeIndexByName(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            return Get(collectionId, user).Value.Attributes.ToList().FindIndex(a => a.Name == attributeName);
        }

        public Maybe<IAttribute> GetAttribute(TCollectionId collectionId, int attributeIndex, ClaimsPrincipal user = null)
        {
            var maybe = Get(collectionId, user);
            if (!maybe.HasValue) {
                return Maybe.Empty<IAttribute>();
            }
            else
            { 
                var featureCollection = maybe.Value;
                var attributes = featureCollection.Attributes;
                return attributes.Count < attributeIndex ? Maybe.Empty<IAttribute>() : attributes[attributeIndex].ToMaybe();
            }
        }

        public Maybe<IAttribute> GetAttribute(TCollectionId collectionId, string attributeName, ClaimsPrincipal user = null)
        {
            var maybe = Get(collectionId, user);
            if (!maybe.HasValue)
            {
                return Maybe.Empty<IAttribute>();
            }

            var featureCollection = maybe.Value;
            var attribute = featureCollection.Attributes.FirstOrDefault(a => a.Name == attributeName);
            return attribute?.ToMaybe() ?? Maybe.Empty<IAttribute>();
        }

        public virtual void UpdateAttributeValue(TCollectionId collectionId, TFeatureId featureId, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var featureIds = new List<TFeatureId>() { featureId };
            UpdateAttributeValue(collectionId, featureIds, attributeIndex, value, user);
        }

        public virtual void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var attributes = new Dictionary<int, object> {{attributeIndex, value}};
            UpdateAttributeValues(collectionId, featureIds, attributes, user);
        }

        public virtual void UpdateAttributeValue(TCollectionId collectionId, IEnumerable<QueryCondition> filter, int attributeIndex, object value, ClaimsPrincipal user = null)
        {
            var attributes = new Dictionary<int, object> {{attributeIndex, value}};
            UpdateAttributeValues(collectionId, filter, attributes, user);
        }

        public virtual void UpdateAttributeValues(TCollectionId collectionId, TFeatureId featureId, IDictionary<int, object> attributes, ClaimsPrincipal user = null)
        {
            var featureIds = new List<TFeatureId>() { featureId };
            UpdateAttributeValues(collectionId, featureIds, attributes, user);
        }

        public abstract void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<TFeatureId> featureIds, IDictionary<int, object> attributes, ClaimsPrincipal user = null);

        public abstract void UpdateAttributeValues(TCollectionId collectionId, IEnumerable<QueryCondition> filter, IDictionary<int, object> attributes, ClaimsPrincipal user = null);
    }
}