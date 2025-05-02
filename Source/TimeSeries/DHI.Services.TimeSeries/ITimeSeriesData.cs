namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Interface ITimeSeriesData
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public interface ITimeSeriesData<TValue> where TValue : struct
    {
        /// <summary>
        ///     Gets the date times.
        /// </summary>
        /// <value>The date times.</value>
        IList<DateTime> DateTimes { get; }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>The values.</value>
        IList<TValue?> Values { get; }
    }
}