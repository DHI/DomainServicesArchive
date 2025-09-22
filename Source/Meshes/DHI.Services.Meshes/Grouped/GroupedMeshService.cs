namespace DHI.Services.Meshes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;
    using Spatial;
    using TimeSeries;

    public class GroupedMeshService<TId> : BaseGroupedDiscreteService<MeshInfo<TId>, TId>, IGroupedMeshService<TId>
    {
        private readonly IGroupedMeshRepository<TId> _repository;

        public GroupedMeshService(IGroupedMeshRepository<TId> repository) : base(repository)
        {
            _repository = repository;
        }

        public GroupedMeshService(IGroupedMeshRepository<TId> repository, ILogger logger) : base(repository, logger)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public IEnumerable<DateTime> GetDateTimes(TId id, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            return _repository.GetDateTimes(id, user);
        }

        /// <inheritdoc/>
        public ITimeSeriesData<double> GetValues(TId id, string item, Point point, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(point, nameof(point));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetValues(id, item, point, dateRange, user);
        }

        /// <inheritdoc/>
        public Dictionary<string, ITimeSeriesData<double>> GetValues(TId id, Point point, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(point, nameof(point));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetValues(id, point, dateRange, user);
        }

        /// <inheritdoc/>
        public ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetAggregatedValues(id, aggregationType, item, dateRange, user);
        }

        /// <inheritdoc/>
        public ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Polygon polygon, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(polygon, nameof(polygon));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetAggregatedValues(id, aggregationType, item, polygon, dateRange, user);
        }

        /// <inheritdoc/>
        public IEnumerable<ITimeSeriesData<double>> GetAggregatedValues(TId id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            var polygonArray = polygons as Polygon[] ?? polygons.ToArray();
            Guard.Against.Null(polygonArray, nameof(polygons));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetAggregatedValues(id, aggregationType, item, polygonArray, dateRange, user);
        }

        /// <inheritdoc/>
        public ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Period period, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(period, nameof(period));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetAggregatedValues(id, aggregationType, item, period, dateRange, user);
        }

        /// <inheritdoc/>
        public ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Polygon polygon, Period period, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(polygon, nameof(polygon));
            Guard.Against.Null(period, nameof(period));
            Guard.Against.Null(dateRange, nameof(dateRange));

            return _repository.GetAggregatedValues(id, aggregationType, item, polygon, period, dateRange, user);
        }

        /// <inheritdoc/>
        public double? GetAggregatedValue(TId id, AggregationType aggregationType, string item, DateTime dateTime, ClaimsPrincipal? user = null)
        {

            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(dateTime, nameof(dateTime));

            var maybe = _repository.GetAggregatedValue(id, aggregationType, item, dateTime, user);
            return maybe | null;
        }

        /// <inheritdoc/>
        public double? GetAggregatedValue(TId id, AggregationType aggregationType, string item, Polygon polygon, DateTime dateTime, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            Guard.Against.Null(polygon, nameof(polygon));
            Guard.Against.Null(dateTime, nameof(dateTime));

            var maybe = _repository.GetAggregatedValue(id, aggregationType, item, polygon, dateTime, user);
            return maybe | null;
        }

        /// <inheritdoc/>
        public IEnumerable<double?> GetAggregatedValues(TId id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateTime dateTime, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            Guard.Against.Null(aggregationType, nameof(aggregationType));
            Guard.Against.NullOrEmpty(item, nameof(item));
            var polygonArray = polygons as Polygon[] ?? polygons.ToArray();
            Guard.Against.Null(polygonArray, nameof(polygons));
            Guard.Against.Null(dateTime, nameof(dateTime));

            var values = _repository.GetAggregatedValues(id, aggregationType, item, polygonArray, dateTime, user);
            return values.Select(maybe => maybe | null).Select(v => (double?)v);
        }

        /// <inheritdoc/>
        public IFeatureCollection GetContours(TId id, string item, IEnumerable<double> thresholdValues, DateTime? dateTime = null, ClaimsPrincipal? user = null)
        {
            if (!Exists(Guard.Against.Null(id, nameof(id)), user))
            {
                throw new KeyNotFoundException($"Mesh with id '{id}' was not found.");
            }

            if (dateTime is not null && !GetDateTimes(id, user).Contains((DateTime)dateTime))
            {
                throw new ArgumentException($"DateTime {dateTime} was not found.", nameof(dateTime));
            }

            Guard.Against.NullOrEmpty(item, nameof(item));
            var thresholdValuesArray = thresholdValues as double[] ?? thresholdValues.ToArray();
            Guard.Against.NullOrEmpty(thresholdValuesArray, nameof(thresholdValues));

            var (mesh, elementData) = _repository.GetMeshData(id, item, dateTime);

            return MeshService<TId>.BuildFeatureCollection(mesh, elementData, id, item, thresholdValuesArray);
        }
    }

    public class GroupedMeshService : GroupedMeshService<string>, IMeshService, IGroupedMeshService
    {
        public GroupedMeshService(IGroupedMeshRepository<string> repository) : base(repository)
        {
        }

        public GroupedMeshService(IGroupedMeshRepository<string> repository, ILogger logger) : base(repository, logger)
        {
        }
    }
}