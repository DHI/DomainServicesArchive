using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Meshes.Test")]

namespace DHI.Services.Meshes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Spatial;
    using TimeSeries;

    /// <summary>
    ///     Abstract base class for a mesh repository.
    ///     Implements the <see cref="IMeshRepository{TId}" /> interface
    /// </summary>
    /// <typeparam name="TId">The type of the mesh identifier.</typeparam>
    /// <seealso cref="IMeshRepository{TId}" />
    public abstract class BaseMeshRepository<TId> : BaseDiscreteRepository<MeshInfo<TId>, TId>, IMeshRepository<TId>
    {
        /// <inheritdoc />
        public abstract IEnumerable<DateTime> GetDateTimes(TId id, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public abstract ITimeSeriesData<double> GetValues(TId id, string item, Point point, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public abstract Dictionary<string, ITimeSeriesData<double>> GetValues(TId id, Point point, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public abstract ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public abstract ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Polygon polygon, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public virtual IEnumerable<ITimeSeriesData<double>> GetAggregatedValues(TId id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            return polygons.Select(polygon => GetAggregatedValues(id, aggregationType, item, polygon, dateRange, user));
        }

        /// <inheritdoc />
        public virtual ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Period period, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            var data = GetAggregatedValues(id, aggregationType, item, dateRange, user);
            return GetGroupedValues(aggregationType, period, data);
        }

        /// <inheritdoc />
        public virtual ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Polygon polygon, Period period, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            var data = GetAggregatedValues(id, aggregationType, item, polygon, dateRange, user);
            return GetGroupedValues(aggregationType, period, data);
        }

        /// <inheritdoc />
        public abstract Maybe<double> GetAggregatedValue(TId id, AggregationType aggregationType, string item, DateTime dateTime, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public abstract Maybe<double> GetAggregatedValue(TId id, AggregationType aggregationType, string item, Polygon polygon, DateTime dateTime, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public virtual IEnumerable<Maybe<double>> GetAggregatedValues(TId id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateTime dateTime, ClaimsPrincipal? user = null)
        {
            return polygons.Select(polygon => GetAggregatedValue(id, aggregationType, item, polygon, dateTime, user));
        }

        /// <inheritdoc />
        public abstract (Mesh mesh, float[] elementData) GetMeshData(TId id, string item, DateTime? dateTime = null, ClaimsPrincipal? user = null);

        internal static ITimeSeriesData<double> GetGroupedValues(AggregationType aggregationType, Period period, ITimeSeriesData<double> data)
        {
            var sortedSet = new SortedSet<DataPoint<double>>();
            foreach (var group in data.GroupBy(period))
            {
                if (aggregationType.Equals(AggregationType.Maximum))
                {
                    sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Max(r => r.Value)));
                }
                else if (aggregationType.Equals(AggregationType.Minimum))
                {
                    sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Min(r => r.Value)));
                }
                else if (aggregationType.Equals(AggregationType.Average))
                {
                    sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Average(r => r.Value)));
                }
                else if (aggregationType.Equals(AggregationType.Sum))
                {
                    sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Sum(r => r.Value)));
                }
            }

            return new TimeSeriesData<double>(sortedSet);
        }
    }
}