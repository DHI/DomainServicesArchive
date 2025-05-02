namespace DHI.Services.TimeSeries
{
    using System.Collections.Generic;

    /// <summary>Class SortedSetExtensions.</summary>
    public static class SortedSetExtensions
    {
        /// <summary>
        ///     Converts a sorted set of time series data points to a TimeSeriesData object.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="sortedSet">The sorted set.</param>
        /// <returns>TimeSeriesData&lt;TValue&gt;.</returns>
        public static TimeSeriesData<TValue> ToTimeSeriesData<TValue>(this SortedSet<DataPoint<TValue>> sortedSet) where TValue : struct
        {
            return new TimeSeriesData<TValue>(sortedSet);
        }

        /// <summary>
        ///     Converts a sorted set of time series data points with flags to a TimeSeriesDataWFlag object.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <typeparam name="TFlag">The type of the flag.</typeparam>
        /// <param name="sortedSet">The sorted set.</param>
        /// <returns>TimeSeriesDataWFlag&lt;TValue, TFlag&gt;.</returns>
        public static TimeSeriesDataWFlag<TValue, TFlag> ToTimeSeriesDataWFlag<TValue, TFlag>(this SortedSet<DataPointWFlag<TValue, TFlag>> sortedSet) where TValue : struct
        {
            return new TimeSeriesDataWFlag<TValue, TFlag>(sortedSet);
        }
    }
}