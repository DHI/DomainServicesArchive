namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Time series data class.
    /// </summary>
    /// <typeparam name="TValue">The type of the time series data values. Must be a numeric type (int, long float, double etc.)</typeparam>
    [Serializable]
    public class TimeSeriesData<TValue> : ITimeSeriesData<TValue> where TValue : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class.
        /// </summary>
        public TimeSeriesData()
        {
            DateTimes = new List<DateTime>();
            Values = new List<TValue?>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with the given data.
        /// </summary>
        /// <param name="dateTimes">The datetimes.</param>
        /// <param name="values">The values.</param>
        public TimeSeriesData(IList<DateTime> dateTimes, IList<TValue?> values)
        {
            DateTimes = dateTimes;
            Values = values;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with the given data.
        ///     Does not accept null values.
        /// </summary>
        /// <param name="dateTimes">The datetimes.</param>
        /// <param name="values">The values.</param>
        public TimeSeriesData(IList<DateTime> dateTimes, IList<TValue> values)
            : this()
        {
            for (var i = 0; i < dateTimes.Count; i++)
            {
                DateTimes.Add(dateTimes[i]);
                Values.Add(values[i]);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with the given data.
        /// </summary>
        /// <param name="sortedSet">The sorted set of data points.</param>
        public TimeSeriesData(SortedSet<DataPoint<TValue>> sortedSet)
        {
            DateTimes = sortedSet.Select(p => p.DateTime).ToList();
            Values = sortedSet.Select(p => p.Value).ToList();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with the given data.
        /// </summary>
        /// <param name="dateTime">The datetime.</param>
        /// <param name="value">The value.</param>
        public TimeSeriesData(DateTime dateTime, TValue? value)
            : this()
        {
            DateTimes.Add(dateTime);
            Values.Add(value);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with the given data point.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        public TimeSeriesData(DataPoint<TValue> dataPoint)
            : this(dataPoint.DateTime, dataPoint.Value)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with the given data point.
        /// </summary>
        /// <param name="dataPoints">The data points.</param>
        public TimeSeriesData(SortedDictionary<DateTime, TValue> dataPoints)
            : this(dataPoints.Keys.ToList(), dataPoints.Values.ToList())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesData{TValue}" /> class primed with a constant value in all
        ///     time steps.
        /// </summary>
        /// <param name="dateTimes">The datetimes.</param>
        /// <param name="value">The constant value.</param>
        public TimeSeriesData(IList<DateTime> dateTimes, TValue value)
            : this(dateTimes, Enumerable.Repeat(value, dateTimes.Count).ToList())
        {
        }

        /// <summary>
        ///     Gets the number of data points (datetimes).
        /// </summary>
        public int Count => DateTimes.Count;

        /// <summary>
        ///     Gets or sets the datetimes.
        /// </summary>
        /// <value>The datetimes.</value>
        public IList<DateTime> DateTimes { get; protected set; }

        /// <summary>
        ///     Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public IList<TValue?> Values { get; protected set; }

        /// <summary>
        ///     True if time series data has any values.
        /// </summary>
        public bool HasValues => Count > 0;

        /// <summary>
        ///     Creates equidistant datetimes with the given time span within the given period.
        /// </summary>
        /// <param name="from">Period start time.</param>
        /// <param name="to">Period end time.</param>
        /// <param name="timeSpan">Time span between samples.</param>
        public static IEnumerable<DateTime> CreateEquidistantDateTimes(DateTime from, DateTime to, TimeSpan timeSpan)
        {
            for (var dateTime = from; dateTime <= to; dateTime = dateTime.Add(timeSpan))
            {
                yield return dateTime;
            }
        }
    }

    /// <inheritdoc />
    public class TimeSeriesData : TimeSeriesData<double>
    {
        public TimeSeriesData()
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(IList<DateTime> dateTimes, IList<double?> values)
            : base(dateTimes, values)
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(IList<DateTime> dateTimes, IList<double> values)
            : base(dateTimes, values)
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(SortedSet<DataPoint<double>> sortedSet)
            : base(sortedSet)
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(DateTime dateTime, double? value)
            : base(dateTime, value)
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(DataPoint<double> dataPoint)
            : base(dataPoint)
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(SortedDictionary<DateTime, double> dataPoints)
            : base(dataPoints)
        {
        }

        /// <inheritdoc />
        public TimeSeriesData(IList<DateTime> dateTimes, double value)
            : base(dateTimes, value)
        {
        }
    }
}