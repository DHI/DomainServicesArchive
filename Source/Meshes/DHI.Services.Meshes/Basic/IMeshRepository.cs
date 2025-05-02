namespace DHI.Services.Meshes
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Spatial;
    using TimeSeries;

    /// <summary>
    ///     Interface IMeshRepository
    /// </summary>
    /// <typeparam name="TId">The type of the mesh identifier.</typeparam>
    public interface IMeshRepository<TId> : IRepository<MeshInfo<TId>, TId>, IDiscreteRepository<MeshInfo<TId>, TId>
    {

        /// <summary>
        ///     Gets the available dateTimes.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="user">The user.</param>
        IEnumerable<DateTime> GetDateTimes(TId id, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns time series data for the specified item at the given location (point).
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The item.</param>
        /// <param name="point">The location.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        ITimeSeriesData<double> GetValues(TId id, string item, Point point, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns a dictionary of time series data for all items at the given location (point).
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="point">The location.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        /// <returns>A dictionary of time series data (with item as the key)</returns>
        Dictionary<string, ITimeSeriesData<double>> GetValues(TId id, Point point, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns aggregated time series data for the entire mesh.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns aggregated time series data within the specified polygon.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="polygon">The polygon.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Polygon polygon, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns aggregated time series data within the specified list of polygons
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="polygons">List of polygons.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        IEnumerable<ITimeSeriesData<double>> GetAggregatedValues(TId id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateRange dateRange, ClaimsPrincipal? user = null);


        /// <summary>
        ///     Returns aggregated time series data for the entire mesh - grouped by period.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="period">The aggregation period.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Period period, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns aggregated time series data within the specified polygon - grouped by period.
        /// </summary>
        /// <remarks>
        ///     The aggregation is done in both the spatial and temporal dimension.
        /// </remarks>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="polygon">The polygon.</param>
        /// <param name="period">The aggregation period.</param>
        /// <param name="dateRange">The time interval.</param>
        /// <param name="user">The user.</param>
        ITimeSeriesData<double> GetAggregatedValues(TId id, AggregationType aggregationType, string item, Polygon polygon, Period period, DateRange dateRange, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns the aggregated value for the entire mesh - at the specified datetime.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="dateTime">The dateTime.</param>
        /// <param name="user">The user.</param>
        Maybe<double> GetAggregatedValue(TId id, AggregationType aggregationType, string item, DateTime dateTime, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns the aggregated value within the specified polygon - at the specified datetime.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="polygon">The polygon.</param>
        /// <param name="dateTime">The dateTime</param>
        /// <param name="user">The user.</param>
        Maybe<double> GetAggregatedValue(TId id, AggregationType aggregationType, string item, Polygon polygon, DateTime dateTime, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns the aggregated value within the specified list of polygons - at the specified datetime.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="item">The item.</param>
        /// <param name="polygons">List of polygons.</param>
        /// <param name="dateTime">The dateTime</param>
        /// <param name="user">The user.</param>
        IEnumerable<Maybe<double>> GetAggregatedValues(TId id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateTime dateTime, ClaimsPrincipal? user = null);

        /// <summary>
        ///     Returns the mesh data.
        /// </summary>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The item.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        (Mesh mesh, float[] elementData) GetMeshData(TId id, string item, DateTime? dateTime = null, ClaimsPrincipal? user = null);

    }
}
