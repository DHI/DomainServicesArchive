namespace DHI.Services.GIS
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseGroupedUpdatableGisRepository<TCollectionId, TFeatureId> : BaseUpdatableGisRepository<TCollectionId, TFeatureId>, IGroupedUpdatableGisRepository<TCollectionId, TFeatureId>
    {
        public abstract bool ContainsGroup(string group, ClaimsPrincipal user = null);

        public abstract IEnumerable<FeatureCollection<TCollectionId>> GetByGroup(string group, ClaimsPrincipal user = null);

        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(featureCollection => featureCollection.FullName).ToArray();
        }

        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(featureCollection => featureCollection.FullName).ToArray();
        }

        public virtual IDictionary<string, string[]> GetGeometryTypes(string group, ClaimsPrincipal user = null)
        {
            var geometryTypes = new Dictionary<string, string[]>();
            var featureCollections = GetByGroup(group, user);
            foreach (var featureCollection in featureCollections)
            {
                var types = featureCollection.Features.Select(f => f.Geometry.Type).Distinct().ToArray();
                geometryTypes.Add(featureCollection.FullName, types);
            }

            return geometryTypes;
        }

        public virtual IEnumerable<FeatureCollectionInfo<TCollectionId>> GetInfoByGroup(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(featureCollection => featureCollection.GetInfo());
        }
    }
}