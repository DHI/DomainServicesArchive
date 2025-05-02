namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IGroupedGisService<TId> : IGisService<TId>, IGroupedService<FeatureCollection<TId>>
    {
        IDictionary<string, string[]> GetGeometryTypes(string group, ClaimsPrincipal user = null);

        IEnumerable<FeatureCollectionInfo<TId>> GetInfo(string group, ClaimsPrincipal user = null);
    }
}