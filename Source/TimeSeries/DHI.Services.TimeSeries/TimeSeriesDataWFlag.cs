namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Time series data with flags.
    /// </summary>
    /// <typeparam name="TValue">The type of the time series data values. Must be a numeric type (int, long float, double etc.)</typeparam>
    /// <typeparam name="TFlag">The type of the time series data flags.</typeparam>
    public class TimeSeriesDataWFlag<TValue, TFlag> : TimeSeriesData<TValue>, ITimeSeriesDataWFlag<TValue, TFlag> where TValue : struct
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesDataWFlag{TValue, TFlag}" /> class.
        /// </summary>
        public TimeSeriesDataWFlag()
        {
            Flags = new List<TFlag>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesDataWFlag{TValue, TFlag}" /> class.
        /// </summary>
        /// <param name="dateTimes">The date times.</param>
        /// <param name="values">The values.</param>
        /// <param name="flags">The flags.</param>
        public TimeSeriesDataWFlag(IList<DateTime> dateTimes, IList<TValue?> values, IList<TFlag> flags) : base(dateTimes, values)
        {
            Flags = flags;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesDataWFlag{TValue, TFlag}" /> class.
        /// </summary>
        /// <param name="sortedSet">The sorted set of data points with flags.</param>
        public TimeSeriesDataWFlag(SortedSet<DataPointWFlag<TValue, TFlag>> sortedSet)
        {
            DateTimes = sortedSet.Select(p => p.DateTime).ToList();
            Values = sortedSet.Select(p => p.Value).ToList();
            Flags = sortedSet.Select(p => p.Flag).ToList();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesDataWFlag{TValue, TFlag}" /> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="flag">The flag.</param>
        public TimeSeriesDataWFlag(DateTime dateTime, TValue? value, TFlag flag) : this()
        {
            DateTimes.Add(dateTime);
            Values.Add(value);
            Flags.Add(flag);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesDataWFlag{TValue, TFlag}" /> class.
        /// </summary>
        /// <param name="dataPointWFlag">The data point with flag.</param>
        public TimeSeriesDataWFlag(DataPointWFlag<TValue, TFlag> dataPointWFlag)
            : this(dataPointWFlag.DateTime, dataPointWFlag.Value, dataPointWFlag.Flag)
        {
        }

        /// <summary>
        ///     Gets the flags.
        /// </summary>
        /// <value>The flags.</value>
        public IList<TFlag> Flags { get; }

        /// <summary>
        ///     Appends the specified value at the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="flag">The flag.</param>
        public void Append(DateTime dateTime, TValue? value, TFlag flag)
        {
            this.Append(dateTime, value);
            Flags.Add(flag);
        }
    }

    /// <inheritdoc />
    public class TimeSeriesDataWFlag<TFlag> : TimeSeriesDataWFlag<double, TFlag>
    {
        /// <inheritdoc />
        public TimeSeriesDataWFlag()
        {
        }

        /// <inheritdoc />
        public TimeSeriesDataWFlag(IList<DateTime> dateTimes, IList<double?> values, IList<TFlag> flags)
            : base(dateTimes, values, flags)
        {
        }

        /// <inheritdoc />
        public TimeSeriesDataWFlag(SortedSet<DataPointWFlag<double, TFlag>> sortedSet)
            : base(sortedSet)
        {
        }

        /// <inheritdoc />
        public TimeSeriesDataWFlag(DateTime dateTime, double? value, TFlag flag)
            : base(dateTime, value, flag)
        {
        }

        /// <inheritdoc />
        public TimeSeriesDataWFlag(DataPointWFlag<double, TFlag> dataPointWFlag)
            : base(dataPointWFlag)
        {
        }
    }
}