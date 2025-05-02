namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface ITimeSeriesService
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series value.</typeparam>
    public interface ITimeSeriesService<TId, TValue> : ICoreTimeSeriesService<TId, TValue> where TValue : struct
    {
        /// <summary>
        ///     Gets a time series with values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>TimeSeries&lt;TId, TValue&gt;.</returns>
        TimeSeries<TId, TValue> GetWithValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets a time series with values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>TimeSeries&lt;TId, TValue&gt;.</returns>
        TimeSeries<TId, TValue> GetWithValues(TId id, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets a time series with values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>Dictionary&lt;TId, TimeSeries&lt;TId, TValue&gt;&gt;.</returns>
        IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(TId[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets a time series with values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>Dictionary&lt;TId, TimeSeries&lt;TId, TValue&gt;&gt;.</returns>
        IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(TId[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the date times for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>SortedSet&lt;DateTime&gt;.</returns>
        SortedSet<DateTime> GetDateTimes(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the first date time for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        DateTime? GetFirstDateTime(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the first value for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        DataPoint<TValue> GetFirstValue(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the first value for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, DataPoint&lt;TValue&gt;&gt;.</returns>
        IDictionary<TId, DataPoint<TValue>> GetFirstValue(TId[] ids, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the first value after the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        DataPoint<TValue> GetFirstValueAfter(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the last date time for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        DateTime? GetLastDateTime(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the last value for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        DataPoint<TValue> GetLastValue(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the last value for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, DataPoint&lt;TValue&gt;&gt;.</returns>
        IDictionary<TId, DataPoint<TValue>> GetLastValue(TId[] ids, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the last value before the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        DataPoint<TValue> GetLastValueBefore(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        ITimeSeriesData<TValue> GetValues(TId id, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, ITimeSeriesData&lt;TValue&gt;&gt;.</returns>
        IDictionary<TId, ITimeSeriesData<TValue>> GetValues(TId[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the vectors in the given time interval for the time series components with the specified ids.
        /// </summary>
        /// <param name="idX">The identifier for the time series with the X-components.</param>
        /// <param name="idY">The identifier for the time series with the Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;.</returns>
        ITimeSeriesData<Vector<TValue>> GetVectors(TId idX, TId idY, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the vectors in the given time interval for the time series components with the specified ids.
        /// </summary>
        /// <param name="idX">The identifier for the time series with the X-components.</param>
        /// <param name="idY">The identifier for the time series with the Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;.</returns>
        ITimeSeriesData<Vector<TValue>> GetVectors(TId idX, TId idY, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the vectors in the given time interval for the list of time series components with the specified ids.
        /// </summary>
        /// <param name="ids">An array of tuples with the identifiers for the time series with the X- and Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;string, ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;&gt;.</returns>
        IDictionary<string, ITimeSeriesData<Vector<TValue>>> GetVectors((TId, TId)[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the Vectors in the given time interval for the list of time series components with the specified ids.
        /// </summary>
        /// <param name="ids">An array of tuples with the identifiers for the time series with the X- and Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;string, ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;&gt;.</returns>
        IDictionary<string, ITimeSeriesData<Vector<TValue>>> GetVectors((TId, TId)[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);
    }
}