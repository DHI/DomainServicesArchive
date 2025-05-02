namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface ICoreTimeSeriesService
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series value.</typeparam>
    public interface ICoreTimeSeriesService<TId, TValue> : IService<TimeSeries<TId, TValue>, TId> where TValue : struct
    {
        /// <summary>
        ///     Gets the value at the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        DataPoint<TValue> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        ITimeSeriesData<TValue> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the aggregated value in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        TValue? GetAggregatedValue(TId id, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, ITimeSeriesData&lt;TValue&gt;&gt;.</returns>
        IDictionary<TId, ITimeSeriesData<TValue>> GetValues(TId[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the aggregated value in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The time series identifiers.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        IDictionary<TId, TValue?> GetAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the list of  aggregated values in the given time interval for the ensemble time series with the specified id.
        /// </summary>
        /// <param name="id">The ensemble time series identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        IList<TValue?> GetEnsembleAggregatedValues(TId id, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the list of aggregated values in the given time interval for the list of ensemble time series with the specified ids.
        /// </summary>
        /// <param name="ids">The time series identifiers.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        IDictionary<TId, IList<TValue?>> GetEnsembleAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);
    }
}