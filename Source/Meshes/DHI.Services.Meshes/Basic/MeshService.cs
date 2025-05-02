namespace DHI.Services.Meshes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using GIS;
    using Logging;
    using Microsoft.Extensions.Logging;
    using Spatial;
    using TimeSeries;
    using Attribute = Spatial.Attribute;

    public class MeshService<TId> : BaseDiscreteService<MeshInfo<TId>, TId>, IMeshService<TId>
    {
        private readonly IMeshRepository<TId> _repository;

        public MeshService(IMeshRepository<TId> repository) : base(repository)
        {
            _repository = repository;
        }

        public MeshService(IMeshRepository<TId> repository, ILogger logger) : base(repository, logger)
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
            return values.Select(maybe => maybe | null).Select(v => (double?) v);
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

            return BuildFeatureCollection(mesh, elementData, id, item, thresholdValuesArray);
        }

        public static FeatureCollection<string> BuildFeatureCollection(Mesh mesh, float[] elemData, TId id, string item, double[] thresholdValues)
        {
            var featureCollection = new FeatureCollection<string>($"{id}-{item}", $"{id}-{item}");
            featureCollection.Attributes.Add(new Attribute("Value", typeof(double), 1));
            mesh.GenerateNodeTable();
            var elements = new ElementCentres(mesh.X, mesh.Y, mesh.Z, mesh.ElementTable);
            var meshConnection = new MeshConnection(mesh);
            var boundaryElements = meshConnection.GetBoundaryElements();

            elemData = SetBoundaryElements(boundaryElements, elemData, thresholdValues.Min());

            var nodeData = mesh.CalculateNodeValues(elements.Xe, elements.Ye, elemData);
            var meshToContours = new Mesh2Contours(mesh, elemData, nodeData, thresholdValues.ToList(), meshConnection);

            for (var i = meshToContours.Contours.Count - 1; i >= 0; i--)
            {
                var nPoints = meshToContours.Contours[i].X.Length;

                var positions = new List<Position>();
                for (var j = 0; j < nPoints; j++)
                {
                    positions.Add(new Position(meshToContours.Contours[i].X[j], meshToContours.Contours[i].Y[j]));
                }

                var polygon = new Polygon();
                polygon.Coordinates.Add(positions);

                var feature = new Feature(polygon);
                feature.AttributeValues.Add("Value", meshToContours.Contours[i].Value);
                featureCollection.Features.Add(feature);
            }

            return featureCollection;
        }

        private static float[] SetBoundaryElements(List<int> boundaryElements, float[] elemData, double val)
        {
            foreach (var boundaryElement in boundaryElements)
            {
                elemData[boundaryElement] = (float)val;
            }

            return elemData;
        }
    }

    public class MeshService : MeshService<string>, IMeshService
    {
        public MeshService(IMeshRepository<string> repository) : base(repository)
        {
        }

        public MeshService(IMeshRepository<string> repository, ILogger logger) : base(repository, logger)
        {
        }
    }
}