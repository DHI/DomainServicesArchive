namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Forecasted time series data class.
    /// </summary>
    /// <typeparam name="TValue">The type of the time series data values. Must be a numeric type (int, long float, double etc.)</typeparam>
    /// <seealso cref="DHI.Services.TimeSeries.TimeSeriesData{TValue}" />
    public class TimeSeriesDataForecasted<TValue> : TimeSeriesData<TValue> where TValue : struct
    {
        /// <summary>
        ///     Gets or sets the forecasted date times.
        /// </summary>
        public IEnumerable<DateTime> DateTimesForecasted { get; protected set; }
    }

    /// <inheritdoc />
    public class TimeSeriesDataForecasted : TimeSeriesDataForecasted<double>
    {
    }
}