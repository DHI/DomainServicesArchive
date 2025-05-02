namespace DHI.Services.GIS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using Spatial;

    public class GroupedGisService<TId> : BaseGroupedDiscreteService<FeatureCollection<TId>, TId>, IGroupedGisService<TId>
    {
        private readonly GisService<TId> _gisService;
        private readonly IGroupedGisRepository<TId> _gisRepository;

        public GroupedGisService(IGroupedGisRepository<TId> repository)
            : base(repository)
        {
            _gisService = new GisService<TId>(repository);
            _gisRepository = repository;
        }

        public FeatureCollection<TId> Get(TId id, IEnumerable<QueryCondition> filter, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(id, filter, associations, outSpatialReference, user);
        }

        public FeatureCollection<TId> Get(TId id, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(id, associations, outSpatialReference, user);
        }

        public FeatureCollectionInfo<TId> GetInfo(TId id, ClaimsPrincipal user = null)
        {
            return _gisService.GetInfo(id, user);
        }

        public IEnumerable<FeatureCollectionInfo<TId>> GetInfo(string group, ClaimsPrincipal user = null)
        {
            return _gisRepository.GetInfoByGroup(group, user);
        }

        public IDictionary<TId, FeatureCollection<TId>> Get(TId[] ids, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.Get(ids, associations, outSpatialReference, user);
        }

        public GeometryCollection GetGeometry(TId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometry(id, outSpatialReference, user);
        }

        public GeometryCollection GetGeometry(TId id, IEnumerable<QueryCondition> filter, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometry(id, filter, outSpatialReference, user);
        }

        public IGeometry GetEnvelope(TId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetEnvelope(id, outSpatialReference, user);
        }

        public IGeometry GetFootprint(TId id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetFootprint(id, outSpatialReference, simplifyDistance, user);
        }

        public IGeometry GetFootprint(IEnumerable<TId> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            return _gisService.GetFootprint(ids, outSpatialReference, simplifyDistance, user);
        }

        public IEnumerable<TId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            return _gisService.GetIds(filter, user);
        }

        public IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null)
        {
            return _gisService.GetGeometryTypes(user);
        }

        public IDictionary<string, string[]> GetGeometryTypes(string group, ClaimsPrincipal user = null)
        {
            return _gisRepository.GetGeometryTypes(group, user);
        }
        
        public (Stream, string fileType, string fileName) GetStream(TId id, ClaimsPrincipal user = null)
        {
            var (maybe, fileType, fileName) = _gisRepository.GetStream(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Feature collection with id '{id}' was not found.");
            }

            return (maybe.Value, fileType, fileName);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IGroupedGisRepository<TId>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IGroupedGisRepository<TId>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IGroupedGisRepository<TId>>(path, searchPattern);
        }
    }

    public class GroupedGisService : GroupedGisService<string>
    {
        public GroupedGisService(IGroupedGisRepository<string> repository) : base(repository)
        {
        }
    }
}