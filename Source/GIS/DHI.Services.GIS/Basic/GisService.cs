namespace DHI.Services.GIS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Spatial;

    public class GisService<TId> : BaseDiscreteService<FeatureCollection<TId>, TId>, IGisService<TId>
    {
        private readonly IGisRepository<TId> _repository;

        public GisService(IGisRepository<TId> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public FeatureCollection<TId> Get(TId id, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(id, associations, outSpatialReference, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The feature collection with id '{id}' was not found.");
            }

            return maybe.Value;
        }

        public (Stream, string fileType, string fileName) GetStream(TId id, ClaimsPrincipal user = null)
        {
            var (maybe, fileType, fileName) = _repository.GetStream(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Feature collection with id '{id}' was not found.");
            }

            return (maybe.Value, fileType, fileName);
        }

        public FeatureCollectionInfo<TId> GetInfo(TId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetInfo(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The feature collection with id '{id}' was not found.");
            }

            return maybe.Value;
        }

        public FeatureCollection<TId> Get(TId id, IEnumerable<QueryCondition> filter, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var queryConditions = filter as QueryCondition[] ?? filter.ToArray();
            // TODO: Update GIS Service tests
            var maybe = _repository.Get(id, queryConditions, associations, outSpatialReference, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The feature collection with id '{id}' was not found.");
            }

            return maybe.Value;
        }

        public IDictionary<TId, FeatureCollection<TId>> Get(TId[] ids, bool associations = false, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            return _repository.Get(ids, associations, outSpatialReference, user);
        }

        public IEnumerable<TId> GetIds(IEnumerable<QueryCondition> filter, ClaimsPrincipal user = null)
        {
            return _repository.GetIds(filter, user);
        }

        public IDictionary<string, string[]> GetGeometryTypes(ClaimsPrincipal user = null)
        {
            return _repository.GetGeometryTypes(user);
        }

        public IGeometry GetEnvelope(TId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetEnvelope(id, outSpatialReference, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Cannot provide envelope for feature collection with id '{id}'.");
            }

            return maybe.Value;
        }

        public IGeometry GetFootprint(TId id, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetFootprint(id, outSpatialReference, simplifyDistance, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Cannot provide footprint for feature collection with id '{id}'.");
            }

            return maybe.Value;
        }

        public IGeometry GetFootprint(IEnumerable<TId> ids, string outSpatialReference = null, double? simplifyDistance = null, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetFootprint(ids, outSpatialReference, simplifyDistance, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException("Cannot provide footprint for given feature collections.");
            }

            return maybe.Value;
        }

        public GeometryCollection GetGeometry(TId id, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidID(id);
            return new GeometryCollection(_repository.GetGeometry(id, outSpatialReference, user).ToList());
        }

        public GeometryCollection GetGeometry(TId id, IEnumerable<QueryCondition> filter, string outSpatialReference = null, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidID(id);
            var queryConditions = filter as QueryCondition[] ?? filter.ToArray();
            foreach (var condition in queryConditions)
            {
                if (condition.Item != "geometry" && !_repository.ContainsAttribute(id, condition.Item))
                {
                    throw new KeyNotFoundException($"The feature collection '{id}' does not contain attribute '{condition.Item}'.");
                }
            }

            return new GeometryCollection(_repository.GetGeometry(id, queryConditions, outSpatialReference, user).ToList());
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IGisRepository<TId>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IGisRepository<TId>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IGisRepository<TId>>(path, searchPattern);
        }

        private void _ThrowExceptionIfInvalidID(TId id)
        {
            if (!Exists(id))
            {
                throw new KeyNotFoundException($"The feature collection with id '{id}' was not found.");
            }
        }
    }

    public class GisService : GisService<string>
    {
        public GisService(IGisRepository<string> repository) : base(repository)
        {
        }
    }
}