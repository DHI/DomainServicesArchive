namespace DHI.Services.GIS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Spatial;

    public abstract class BaseGisRepository<TId> : BaseDiscreteRepository<FeatureCollection<TId>, TId>, IGisRepository<TId>
    {
        public virtual bool ContainsAttribute(TId id, string name, ClaimsPrincipal user = null)
        {
            return _GetFeatureCollection(id, null, user).Attributes.Any(attribute => attribute.Name.Equals(name));
        }

        public abstract Maybe<FeatureCollection<TId>> Get(TId id, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        public abstract Maybe<FeatureCollection<TId>> Get(TId id, IEnumerable<QueryCondition> filter, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        public virtual IDictionary<TId, FeatureCollection<TId>> Get(IEnumerable<TId> ids, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, FeatureCollection<TId>>();
            foreach (var id in ids)
            {
                var maybe = Get(id, associations, outSpatialReference, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }

        public virtual (Maybe<Stream>, string fileType, string fileName) GetStream(TId id, ClaimsPrincipal user = null)
        {
            throw new NotSupportedException("This repository does not support streaming of feature collections.");
        }

        public virtual Maybe<FeatureCollectionInfo<TId>> GetInfo(TId id, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, false, user: user);
            return maybe.HasValue ? maybe.Value.GetInfo().ToMaybe() : Maybe.Empty<FeatureCollectionInfo<TId>>();
        }

        public abstract Maybe<IGeometry> GetEnvelope(TId id, string outSpatialReference = null, ClaimsPrincipal user = null);

        public abstract Maybe<IGeometry> GetFootprint(TId id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null);

        public abstract Maybe<IGeometry> GetFootprint(IEnumerable<TId> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null);

        public virtual IEnumerable<IGeometry> GetGeometry(TId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _GetFeatureCollection(id, outSpatialReference, user).Features.Select(feature => feature.Geometry).ToArray();
        }

        public virtual IEnumerable<IGeometry> GetGeometry(TId id, IEnumerable<QueryCondition> filter, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, filter, false, outSpatialReference, user);
            return !maybe.HasValue ? null : maybe.Value.Features.Select(feature => feature.Geometry).ToArray();
        }

        public abstract IEnumerable<TId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null);

        public virtual IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null)
        {
            var geometryTypes = new Dictionary<string, string[]>();
            var featureCollections = GetAll(user);
            foreach (var featureCollection in featureCollections)
            {
                var types = featureCollection.Features.Select(f => f.Geometry.Type).Distinct().ToArray();
                geometryTypes.Add(featureCollection.FullName, types);
            }

            return geometryTypes;
        }

        private FeatureCollection<TId> _GetFeatureCollection(TId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = Get(id, false, outSpatialReference, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Feature collection with id '{id}' was not found.");
            }

            return maybe.Value;
        }
    }
}