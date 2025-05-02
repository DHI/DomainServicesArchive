namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using Spatial;

    public interface IGisService<TId> : IService<FeatureCollection<TId>, TId>, IDiscreteService<FeatureCollection<TId>, TId>, IStreamableService<TId>
    {
        FeatureCollection<TId> Get(TId id, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        FeatureCollectionInfo<TId> GetInfo(TId id, ClaimsPrincipal user = null);

        FeatureCollection<TId> Get(TId id, IEnumerable<QueryCondition> filter, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        IDictionary<TId, FeatureCollection<TId>> Get(TId[] ids, bool associations, string outSpatialReference = null, ClaimsPrincipal user = null);

        GeometryCollection GetGeometry(TId id, string outSpatialReference = null, ClaimsPrincipal user = null);

        GeometryCollection GetGeometry(TId id, IEnumerable<QueryCondition> filter, string outSpatialReference = null, ClaimsPrincipal user = null);

        IGeometry GetEnvelope(TId id, string outSpatialReference = null, ClaimsPrincipal user = null);

        IGeometry GetFootprint(TId id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null);

        IGeometry GetFootprint(IEnumerable<TId> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null);

        IEnumerable<TId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null);

        IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null);

    }
}