namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class TimeSeriesExtensions
    {
        /// <summary>
        ///     Converts an enumerable of nullable double values to doubles.
        ///     Null values are converted to Double.NaN.
        /// </summary>
        /// <param name="values"></param>
        /// <returns>Enumerable of doubles.</returns>
        public static IEnumerable<double> ToDoubles(this IEnumerable<double?> values)
        {
            return values.Select(value => value ?? double.NaN);
        }

        /// <summary>
        ///     Gets an array of values where null values are converted to Double.NaN.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <returns>Time series data values.</returns>
        public static double[] GetDoubleValues(this ITimeSeriesData<double> data)
        {
            return data.Values.ToDoubles().ToArray();
        }

        /// <summary>
        ///     Checks if the time series data period covers the given dateTime.
        ///     The time series may or may not have a specified value at that specific dateTime.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The dateTime.</param>
        /// <returns>True if the input dateTime is covered by the time series data period.</returns>
        public static bool CoversDateTime(this ITimeSeriesData<double> data, DateTime dateTime)
        {
            return dateTime >= data.GetFirstDateTime().Value && dateTime <= data.GetLastDateTime().Value;
        }

        /// <summary>
        ///     Creates a new time series from an existing time series - optionally with the given new data.
        ///     Reuses all header information and meta data. Does not copy existing time series data to the returned time series.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="timeSeries">The time series to copy from.</param>
        /// <param name="data">The new data</param>
        /// <returns>A copy of the time series with the given data or no data.</returns>
        public static TimeSeries<string, TValue> CopyWith<TValue>(this TimeSeries<string, TValue> timeSeries, ITimeSeriesData<TValue> data = null) where TValue : struct
        {
            var newTimeSeries = new TimeSeries<string, TValue>(timeSeries.Id, timeSeries.Name, timeSeries.Group, data)
            {
                DataType = timeSeries.DataType,
                Dimension = timeSeries.Dimension,
                Quantity = timeSeries.Quantity,
                Unit = timeSeries.Unit
            };

            timeSeries.CopyMetaDataTo(newTimeSeries);
            return newTimeSeries;
        }

        /// <summary>
        ///     Create a new time series from an existing time series object with the given data point.
        ///     Reuses all header information and meta data. Does not copy existing time series data to the returned time series.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="timeSeries">The time series to copy from.</param>
        /// <param name="dataPoint">The data point.</param>
        /// <returns></returns>
        public static TimeSeries<string, TValue> CopyWith<TValue>(this TimeSeries<string, TValue> timeSeries, DataPoint<TValue> dataPoint) where TValue : struct
        {
            Guard.Against.Null(dataPoint, nameof(dataPoint));
            var data = new TimeSeriesData<TValue>(dataPoint.DateTime, dataPoint.Value);
            return timeSeries.CopyWith(data);
        }

        /// <summary>
        ///     Copy time series metadata to target time series.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="timeSeries">The time series to copy from.</param>
        /// <param name="target">The time series to copy to.</param>
        public static void CopyMetaDataTo<TValue>(this TimeSeries<string, TValue> timeSeries, TimeSeries<string, TValue> target) where TValue : struct
        {
            foreach (var metaData in timeSeries.Metadata)
            {
                target.Metadata.Add(metaData.Key, metaData.Value);
            }
        }

        /// <summary>
        ///     Converts a time series data object to a sorted set of data points.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>SortedSet&lt;DataPoint&lt;TValue&gt;&gt;.</returns>
        public static SortedSet<DataPoint<TValue>> ToSortedSet<TValue>(this ITimeSeriesData<TValue> data) where TValue : struct
        {
            var sortedSet = new SortedSet<DataPoint<TValue>>();

            for (var i = 0; i < data.DateTimes.Count; i++)
            {
                var dataPoint = new DataPoint<TValue>(data.DateTimes[i], data.Values[i]);
                sortedSet.Add(dataPoint);
            }

            return sortedSet;
        }

        /// <summary>
        ///     Converts a time series data object with flags to a sorted set of data points with flags.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <typeparam name="TFlag">The type of the flag.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>SortedSet&lt;DataPointWFlag&lt;TValue, TFlag&gt;&gt;.</returns>
        public static SortedSet<DataPointWFlag<TValue, TFlag>> ToSortedSet<TValue, TFlag>(this ITimeSeriesDataWFlag<TValue, TFlag> data) where TValue : struct
        {
            var sortedSet = new SortedSet<DataPointWFlag<TValue, TFlag>>();

            for (var i = 0; i < data.DateTimes.Count; i++)
            {
                var dataPoint = new DataPointWFlag<TValue, TFlag>(data.DateTimes[i], data.Values[i], data.Flags[i]);
                sortedSet.Add(dataPoint);
            }

            return sortedSet;
        }

        /// <summary>
        ///     Converts a time series data object to a sorted dictionary.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>SortedDictionary&lt;DateTime, System.Nullable&lt;TValue&gt;&gt;.</returns>
        public static SortedDictionary<DateTime, TValue?> ToSortedDictionary<TValue>(this ITimeSeriesData<TValue> data) where TValue : struct
        {
            var dictionary = new SortedDictionary<DateTime, TValue?>();

            for (var i = 0; i < data.DateTimes.Count; i++)
            {
                dictionary.Add(data.DateTimes[i], data.Values[i]);
            }

            return dictionary;
        }

        /// <summary>
        ///     Determines whether the time series data contains the specified date time.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns><c>true</c> if the time series data contains the specified date time; otherwise, <c>false</c>.</returns>
        public static bool ContainsDateTime<TValue>(this ITimeSeriesData<TValue> data, DateTime dateTime) where TValue : struct
        {
            return data.DateTimes.Any(d => d.Equals(dateTime));
        }

        /// <summary>
        ///     Gets the first date time (if any).
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>Maybe&lt;DateTime&gt;.</returns>
        public static Maybe<DateTime> GetFirstDateTime<TValue>(this ITimeSeriesData<TValue> data) where TValue : struct
        {
            return data.DateTimes.Count > 0 ? data.DateTimes.Min().ToMaybe() : Maybe.Empty<DateTime>();
        }

        /// <summary>
        ///     Gets the first data point (if any).
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>Maybe&lt;DataPoint&lt;TValue&gt;&gt;.</returns>
        public static Maybe<DataPoint<TValue>> GetFirst<TValue>(this ITimeSeriesData<TValue> data) where TValue : struct
        {
            var firstDateTime = data.GetFirstDateTime();
            return !firstDateTime.HasValue ? Maybe.Empty<DataPoint<TValue>>() : data.Get(firstDateTime.Value);
        }

        /// <summary>
        ///     Gets the first data point after the specified date time.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Maybe&lt;DataPoint&lt;TValue&gt;&gt;.</returns>
        public static Maybe<DataPoint<TValue>> GetFirstAfter<TValue>(this ITimeSeriesData<TValue> data, DateTime dateTime) where TValue : struct
        {
            return data.DateTimes.Any(d => d > dateTime) ? data.Get(data.DateTimes.First(d => d > dateTime)) : Maybe.Empty<DataPoint<TValue>>();
        }

        /// <summary>
        ///     Gets the last date time (if any).
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>Maybe&lt;DateTime&gt;.</returns>
        public static Maybe<DateTime> GetLastDateTime<TValue>(this ITimeSeriesData<TValue> data) where TValue : struct
        {
            return data.DateTimes.Count > 0 ? data.DateTimes.Max().ToMaybe() : Maybe.Empty<DateTime>();
        }

        /// <summary>
        ///     Gets the last data point (if any).
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <returns>Maybe&lt;DataPoint&lt;TValue&gt;&gt;.</returns>
        public static Maybe<DataPoint<TValue>> GetLast<TValue>(this ITimeSeriesData<TValue> data) where TValue : struct
        {
            var lastDateTime = data.GetLastDateTime();
            return !lastDateTime.HasValue ? Maybe.Empty<DataPoint<TValue>>() : data.Get(lastDateTime.Value);
        }

        /// <summary>
        ///     Gets the last data point before the specified date time.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Maybe&lt;DataPoint&lt;TValue&gt;&gt;.</returns>
        public static Maybe<DataPoint<TValue>> GetLastBefore<TValue>(this ITimeSeriesData<TValue> data, DateTime dateTime) where TValue : struct
        {
            return data.DateTimes.Any(d => d < dateTime) ? data.Get(data.DateTimes.Last(d => d < dateTime)) : Maybe.Empty<DataPoint<TValue>>();
        }

        /// <summary>
        ///     Gets the data point at the specified date time (if any).
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Maybe&lt;DataPoint&lt;TValue&gt;&gt;.</returns>
        public static Maybe<DataPoint<TValue>> Get<TValue>(this ITimeSeriesData<TValue> data, DateTime dateTime) where TValue : struct
        {
            var i = data.DateTimes.IndexOf(dateTime);
            return i == -1 ? Maybe.Empty<DataPoint<TValue>>() : new DataPoint<TValue>(dateTime, data.Values[i]).ToMaybe();
        }

        /// <summary>
        ///     Gets an interpolated data point.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="dataType">The type of the time series values.</param>
        /// <returns>DataPoint&lt;System.Double&gt;.</returns>
        public static DataPoint<double> GetInterpolated(this ITimeSeriesData<double> data, DateTime dateTime, TimeSeriesDataType dataType)
        {
            return data.GetInterpolatedPoint(dateTime, dataType).point;
        }

        /// <summary>
        ///     Gets an interpolated data point.
        ///     If the gap tolerance time span is given, values will only be interpolated over intervals less than or equal to the
        ///     given time span.
        ///     Otherwise, a null value will be returned.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="dataType">The type of the time series values.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <returns>System.ValueTuple&lt;DataPoint&lt;System.Double&gt;, System.Boolean&gt;.</returns>
        public static (DataPoint<double> point, bool isInterpolated) GetInterpolated(this ITimeSeriesData<double> data, DateTime dateTime, TimeSeriesDataType dataType, TimeSpan? gapTolerance)
        {
            return data.GetInterpolatedPoint(dateTime, dataType, gapTolerance);
        }

        /// <summary>
        ///     Gets the time steps using the given time steps selection type.
        /// </summary>
        /// <param name="data">The list of time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection type.</param>
        /// <returns>DateTime[].</returns>
        public static DateTime[] GetTimeSteps(this IList<ITimeSeriesData<double>> data, TimeStepsSelection timeStepsSelection)
        {
            SortedSet<DateTime> timeSteps;
            switch (timeStepsSelection)
            {
                case TimeStepsSelection.All:
                    var dateTimes = new List<DateTime>();
                    foreach (var timeSeriesData in data)
                    {
                        dateTimes.AddRange(timeSeriesData.DateTimes);
                    }

                    timeSteps = new SortedSet<DateTime>(dateTimes.Distinct().ToList());
                    break;
                case TimeStepsSelection.CommonOnly:
                    var dateTimeLists = data.Select(r => r.DateTimes.ToList()).ToList();
                    var intersection = dateTimeLists.Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToList());
                    timeSteps = new SortedSet<DateTime>(intersection.ToList());
                    break;
                case TimeStepsSelection.FirstOnly:
                    timeSteps = new SortedSet<DateTime>(data.First().DateTimes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeStepsSelection), timeStepsSelection, null);
            }

            return timeSteps.ToArray();
        }

        /// <summary>
        ///     Gets the time series data within the specified time interval.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="includeFrom">if set to <c>true</c> include interval start.</param>
        /// <param name="includeTo">if set to <c>true</c> include interval end.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        public static ITimeSeriesData<TValue> Get<TValue>(this ITimeSeriesData<TValue> data, DateTime from, DateTime to, bool includeFrom = true, bool includeTo = true) where TValue : struct
        {
            if (data.DateTimes.Count == 0 || data.DateTimes.Last() < from || data.DateTimes.First() > to)
            {
                return new TimeSeriesData<TValue>();
            }

            var first = includeFrom ? data.DateTimes.First(d => d >= from) : data.DateTimes.First(d => d > from);
            var iFirst = data.DateTimes.IndexOf(first);

            var last = includeTo ? data.DateTimes.Last(d => d <= to) : data.DateTimes.Last(d => d < to);
            var iLast = data.DateTimes.IndexOf(last);

            var timeSeriesData = new TimeSeriesData<TValue>(data.DateTimes.Skip(iFirst).Take(iLast - iFirst + 1).ToList(), data.Values.Skip(iFirst).Take(iLast - iFirst + 1).ToList());
            return timeSeriesData;
        }

        /// <summary>
        ///     Gets time series data multiplied with the specified factor.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="factor">The factor.</param>
        /// <returns>ITimeSeriesData&lt;System.Double&gt;.</returns>
        public static ITimeSeriesData<double> GetScaled(this ITimeSeriesData<double> data, double factor)
        {
            if (factor.Equals(1d))
            {
                return data;
            }

            var timeSeriesData = new TimeSeriesData<double>(data.DateTimes, data.Values.Select(value => value * factor).ToArray());
            return timeSeriesData;
        }

        /// <summary>
        ///     Appends (at the end) the specified value at the specified date time.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        public static void Append<TValue>(this ITimeSeriesData<TValue> data, DateTime dateTime, TValue? value) where TValue : struct
        {
            data.DateTimes.Add(dateTime);
            data.Values.Add(value);
        }

        /// <summary>
        ///     Appends (at the end) the specified value at the specified date time with the specified flag.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <typeparam name="TFlag">The type of the time series flags.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="flag">The flag</param>
        public static void Append<TValue, TFlag>(this ITimeSeriesDataWFlag<TValue, TFlag> data, DateTime dateTime, TValue? value, TFlag flag) where TValue : struct
        {
            data.DateTimes.Add(dateTime);
            data.Values.Add(value);
            data.Flags.Add(flag);
        }

        /// <summary>
        ///     Inserts (in the beginning) the specified value at the specified date time.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        public static void Insert<TValue>(this ITimeSeriesData<TValue> data, DateTime dateTime, TValue? value) where TValue : struct
        {
            data.DateTimes.Insert(0, dateTime);
            data.Values.Insert(0, value);
        }

        /// <summary>
        ///     Inserts (in the beginning) the specified value at the specified date time with the specified flag.
        /// </summary>
        /// <typeparam name="TValue">The type of the time series values.</typeparam>
        /// <typeparam name="TFlag">The type of the time series flags.</typeparam>
        /// <param name="data">The time series data.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="flag">The flag</param>
        public static void Insert<TValue, TFlag>(this ITimeSeriesDataWFlag<TValue, TFlag> data, DateTime dateTime, TValue? value, TFlag flag) where TValue : struct
        {
            data.DateTimes.Insert(0, dateTime);
            data.Values.Insert(0, value);
            data.Flags.Insert(0, flag);
        }

        public static bool ContainsSameData(this ITimeSeriesData<double> data, ITimeSeriesData<double> otherData)
        {
            if (data.DateTimes.Count != otherData.DateTimes.Count)
                return false;

            var dataSet = data.ToSortedSet();
            var otherDataSet = otherData.ToSortedSet();

            for (int i = 0; i < dataSet.Count; i++)
            {
                var dataPoint = dataSet.ElementAt(i);
                var otherDataPoint = otherDataSet.ElementAt(i);

                if (dataPoint.DateTime != otherDataPoint.DateTime)
                    return false;

                if (!dataPoint.Value.GetValueOrDefault().Equals(otherDataPoint.Value.GetValueOrDefault()))
                    return false;
            }

            return true;
        }

        internal static (DataPoint<double> point, bool isInterpolated) GetInterpolatedPoint(this ITimeSeriesData<double> data, DateTime dateTime, TimeSeriesDataType dataType, TimeSpan? gapTolerance = null)
        {
            var maybe = Get(data, dateTime);
            if (maybe.HasValue && maybe.Value.Value != null)
            {
                return (maybe.Value, false);
            }

            // Find last value before that is not null
            var lastBefore = GetLastBefore(data, dateTime);
            while (lastBefore.HasValue && lastBefore.Value.Value == null)
            {
                lastBefore = GetLastBefore(data, lastBefore.Value.DateTime);
            }

            // Find first value after that is not null
            var firstAfter = GetFirstAfter(data, dateTime);
            while (firstAfter.HasValue && firstAfter.Value.Value == null)
            {
                firstAfter = GetFirstAfter(data, firstAfter.Value.DateTime);
            }

            if (lastBefore.HasValue && firstAfter.HasValue && (gapTolerance is null || firstAfter.Value.DateTime - lastBefore.Value.DateTime <= gapTolerance))
            {
                return (dataType.Interpolate(lastBefore.Value, firstAfter.Value, dateTime), true);
            }

            return (new DataPoint<double>(dateTime, null), false);
        }

        /// <summary>
        /// Returns true if time series is equidistant.
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <returns></returns>
        public static bool IsEquidistant(this TimeSeries<string, double> timeSeries)
        {
            var time = timeSeries.Data?.DateTimes;

            if (time.Count > 1)
            {
                var previousTimeStep = time[1] - time[0];
                for (int i = 1; i < time.Count - 1; i++)
                {
                    var timeStep = time[i + 1] - time[i];
                    if (timeStep.CompareTo(previousTimeStep) != 0)
                        return false;

                    previousTimeStep = timeStep;
                }
            }
            return true;
        }
    }
}