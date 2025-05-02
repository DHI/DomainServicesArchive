namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using Spatial;

    public interface IGisRepository<TId> : IRepository<FeatureCollection<TId>, TId>, IDiscreteRepository<FeatureCollection<TId>, TId>, IStreamableRepository<TId>
    {
        bool ContainsAttribute(TId id, string name, ClaimsPrincipal user = null);

        Maybe<FeatureCollection<TId>> Get(TId id, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        Maybe<FeatureCollection<TId>> Get(TId id, IEnumerable<QueryCondition> filter, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        IDictionary<TId, FeatureCollection<TId>> Get(IEnumerable<TId> ids, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        Maybe<FeatureCollectionInfo<TId>> GetInfo(TId id, ClaimsPrincipal user = null);

        IEnumerable<IGeometry> GetGeometry(TId id, string outSpatialReference = null, ClaimsPrincipal user = null);

        IEnumerable<IGeometry> GetGeometry(TId id, IEnumerable<QueryCondition> filter, string outSpatialReference = null, ClaimsPrincipal user = null);

        IEnumerable<TId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null);

        IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null);

        Maybe<IGeometry> GetEnvelope(TId id, string outSpatialReference = null, ClaimsPrincipal user = null);
        
        Maybe<IGeometry> GetFootprint(TId id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null);

        Maybe<IGeometry> GetFootprint(IEnumerable<TId> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null);
    }
}