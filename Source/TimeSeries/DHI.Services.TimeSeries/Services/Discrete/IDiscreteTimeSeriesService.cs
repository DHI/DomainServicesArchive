namespace DHI.Services.TimeSeries
{
    using System;
    using System.Security.Claims;

    /// <summary>
    ///     Interface IDiscreteTimeSeriesService
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series values.</typeparam>
    public interface IDiscreteTimeSeriesService<TId, TValue> : ITimeSeriesService<TId, TValue>, IDiscreteService<TimeSeries<TId, TValue>, TId> where TValue : struct
    {
        /// <summary>
        ///     Sets some time series values for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="data">The time series data.</param>
        /// <param name="user">The user.</param>
        void SetValues(TId id, ITimeSeriesData<TValue> data, ClaimsPrincipal user = null);

        /// <summary>
        ///     Occurs when values are set.
        /// </summary>
        event EventHandler<EventArgs<(TId id, ITimeSeriesData<TValue> data, string userName)>> ValuesSet;
    }
}