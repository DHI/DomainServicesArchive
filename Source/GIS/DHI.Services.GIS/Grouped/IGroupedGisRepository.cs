namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IGroupedGisRepository<TId> : IGisRepository<TId>, IGroupedRepository<FeatureCollection<TId>>
    {
        IDictionary<string, string[]> GetGeometryTypes(string group, ClaimsPrincipal user = null);

        IEnumerable<FeatureCollectionInfo<TId>> GetInfoByGroup(string group, ClaimsPrincipal user = null);
    }
}