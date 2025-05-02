namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface IUpdatableTimeSeriesService
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series value.</typeparam>
    public interface IUpdatableTimeSeriesService<TId, TValue> : IDiscreteTimeSeriesService<TId, TValue>, IUpdatableService<TimeSeries<TId, TValue>, TId> where TValue : struct
    {
        /// <summary>
        ///     Adds the specified list of time series.
        /// </summary>
        /// <param name="timeSeriesList">The time series list.</param>
        /// <param name="user">The user.</param>
        void Add(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList, ClaimsPrincipal user = null);

        /// <summary>
        ///     Updates the specified list of time series.
        /// </summary>
        /// <param name="timeSeriesList">The time series list.</param>
        /// <param name="user">The user.</param>
        void Update(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList, ClaimsPrincipal user = null);

        /// <summary>
        ///     Removes the time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="user">The user.</param>
        void Remove(IEnumerable<TId> ids, ClaimsPrincipal user = null);

        /// <summary>
        ///     Removes the time series values in the given interval.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        void RemoveValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Removes the time series values in the given interval.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        void RemoveValues(TId id, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);
    }
}