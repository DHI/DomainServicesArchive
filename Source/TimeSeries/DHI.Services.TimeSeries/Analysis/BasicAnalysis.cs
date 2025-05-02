namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Time series data extension methods for various basic analysis.
    /// </summary>
    public static class BasicAnalysis
    {
        /// <summary>
        ///     Calculates the total time span covered by the times series data.
        /// </summary>
        /// <typeparam name="TValue">The numeric type of the time series data values.</typeparam>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>TimeSpan.</returns>
        public static TimeSpan TimeSpan<TValue>(this ITimeSeriesData<TValue> timeSeriesData) where TValue : struct
        {
            if (!timeSeriesData.DateTimes.Any())
            {
                return default;
            }

            return timeSeriesData.DateTimes.Max() - timeSeriesData.DateTimes.Min();
        }

        /// <summary>
        ///     Calculates the sum of the time series data values.
        /// </summary>
        /// <typeparam name="TValue">The numeric type of the time series data values.</typeparam>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The sum of time series data values.</returns>
        public static TValue? Sum<TValue>(this ITimeSeriesData<TValue> timeSeriesData) where TValue : struct
        {
            return timeSeriesData.Values.Aggregate<TValue?, dynamic>(0, (current, value) => current + (value ?? (dynamic)0));
        }

        /// <summary>
        ///     Calculates the sum of the time series data values.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The sum of time series data values.</returns>
        public static double? Sum(this ITimeSeriesData<double> timeSeriesData)
        {
            return timeSeriesData.Values.Sum();
        }

        /// <summary>
        ///     Calculates the sum of the time series data values.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The sum of time series data values.</returns>
        public static float? Sum(this ITimeSeriesData<float> timeSeriesData)
        {
            return timeSeriesData.Values.Sum();
        }

        /// <summary>
        ///     Calculates the minimum value of the time series data.
        /// </summary>
        /// <typeparam name="TValue">The numeric type of time series data values.</typeparam>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The minimum time series data value.</returns>
        public static TValue? Minimum<TValue>(this ITimeSeriesData<TValue> timeSeriesData) where TValue : struct, IComparable<TValue>
        {
            return timeSeriesData.Values.Min();
        }

        /// <summary>
        ///     Calculates the minimum value of the time series data.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The minimum time series data value.</returns>
        public static double? Minimum(this ITimeSeriesData<double> timeSeriesData)
        {
            return timeSeriesData.Values.Min();
        }

        /// <summary>
        ///     Calculates the minimum value of the time series data.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The minimum time series data value.</returns>
        public static float? Minimum(this ITimeSeriesData<float> timeSeriesData)
        {
            return timeSeriesData.Values.Min();
        }

        /// <summary>
        ///     Calculates the maximum value of the time series data.
        /// </summary>
        /// <typeparam name="TValue">The numeric type of time series data values.</typeparam>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The maximum time series data value.</returns>
        public static TValue? Maximum<TValue>(this ITimeSeriesData<TValue> timeSeriesData) where TValue : struct, IComparable<TValue>
        {
            return timeSeriesData.Values.Max();
        }

        /// <summary>
        ///     Calculates the maximum value of the time series data.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The maximum time series data value.</returns>
        public static double? Maximum(this ITimeSeriesData<double> timeSeriesData)
        {
            return timeSeriesData.Values.Max();
        }

        /// <summary>
        ///     Calculates the maximum value of the time series data.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The maximum time series data value.</returns>
        public static float? Maximum(this ITimeSeriesData<float> timeSeriesData)
        {
            return timeSeriesData.Values.Max();
        }

        /// <summary>
        ///     Calculates the average value of the time series data.
        /// </summary>
        /// <typeparam name="TValue">The numeric type of time series data values.</typeparam>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The average time series data value.</returns>
        public static TValue? Average<TValue>(this ITimeSeriesData<TValue> timeSeriesData) where TValue : struct
        {
            dynamic count = timeSeriesData.Values.Count(v => v != null);
            return count > 0 ? timeSeriesData.Sum() / count : 0;
        }

        /// <summary>
        ///     Calculates the average value of the time series data.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The average time series data value.</returns>
        public static double? Average(this ITimeSeriesData<double> timeSeriesData)
        {
            var count = timeSeriesData.Values.Count(v => v != null);
            return count > 0 ? timeSeriesData.Sum() / count : 0;
        }

        /// <summary>
        ///     Calculates the average value of the time series data between a start and an end time.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="startTime">The time to average from</param>
        /// <param name="endTime">The time to average to</param>
        /// <returns>The average time series data value.</returns>
        public static double? Average(this ITimeSeriesData<double> timeSeriesData, DateTime startTime, DateTime endTime)
        {
            var values = timeSeriesData.ToSortedSet().Where(point => point.DateTime >= startTime && point.DateTime <= endTime && point.Value.HasValue).ToArray();
            return values.Any() ? values.Average(point => point.Value) : null;
        }

        /// <summary>
        ///     Calculates the average value of the time series data.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <returns>The average time series data value.</returns>
        public static float? Average(this ITimeSeriesData<float> timeSeriesData)
        {
            var count = timeSeriesData.Values.Count(v => v != null);
            return count > 0 ? timeSeriesData.Sum() / count : 0;
        }

        /// <summary>
        ///     Calculates the moving average over a given window.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="window">The window length.</param>
        /// <returns>Moving average time series data.</returns>
        public static TimeSeriesData<double> MovingAverage(this ITimeSeriesData<double> timeSeriesData, int window)
        {
            var result = new TimeSeriesData<double>();

            double? total = 0;
            var nullsInWindow = new Queue<bool>();
            for (var i = 0; i < timeSeriesData.DateTimes.Count; i++)
            {
                var currentValue = timeSeriesData.Values[i];
                nullsInWindow.Enqueue(currentValue == null);
                if (currentValue != null)
                {
                    total += currentValue;
                }

                // Start throwing values off
                if (i >= window)
                {
                    var valueFallingOut = timeSeriesData.Values[i - window];
                    if (valueFallingOut != null)
                    {
                        total -= valueFallingOut;
                    }

                    nullsInWindow.Dequeue();
                }

                // Start adding average values
                if (i >= window - 1)
                {
                    var average = total / (window - nullsInWindow.Count(n => n));
                    result.Append(timeSeriesData.DateTimes[i], average);
                }
            }

            return result;
        }

        /// <summary>
        ///     Calculates the moving average over a time span. The moving average type determines if data before, after or in
        ///     the middle should be used
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeSpan">The time span between time steps.</param>
        /// <param name="movingAverageType">The rolling average type</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        [Obsolete("Use the MovingAverage method with MovingAggregateType instead. This method might be removed in a future version.")]
        public static TimeSeriesData<double> MovingAverage(this ITimeSeriesData<double> timeSeriesData, TimeSpan timeSpan, MovingAverageType movingAverageType)
        {
            if (timeSeriesData.Values.First() == null)
            {
                throw new Exception("The value of the first time step is null. This is not allowed.");
            }

            if (timeSeriesData.Values.Last() == null)
            {
                throw new Exception("The value of the last time step is null. This is not allowed.");
            }

            var values = new TimeSeriesData<double>();

            foreach (var dateTime in timeSeriesData.DateTimes)
            {
                switch (movingAverageType)
                {
                    case MovingAverageType.Backwards:
                        values.Append(dateTime, timeSeriesData.Average(dateTime.Subtract(timeSpan), dateTime));
                        break;
                    case MovingAverageType.Forward:
                        values.Append(dateTime, timeSeriesData.Average(dateTime, dateTime.Add(timeSpan)));
                        break;
                    case MovingAverageType.Middle:
                        values.Append(dateTime, timeSeriesData.Average(dateTime.AddDays(-timeSpan.TotalDays / 2), dateTime.AddDays(timeSpan.TotalDays / 2)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(movingAverageType), movingAverageType, null);
                }
            }

            return values;
        }

        /// <summary>
        ///     Calculates the moving average over a time span. The moving average type determines if data before, after or in
        ///     the middle should be used
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeSpan">The time span between time steps.</param>
        /// <param name="movingAggregationType">The rolling average type</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> MovingAverage(this ITimeSeriesData<double> timeSeriesData, TimeSpan timeSpan, MovingAggregationType movingAggregationType)
        {
            if (timeSeriesData.Values.First() == null)
            {
                throw new Exception("The value of the first time step is null. This is not allowed.");
            }

            if (timeSeriesData.Values.Last() == null)
            {
                throw new Exception("The value of the last time step is null. This is not allowed.");
            }

            var values = new TimeSeriesData<double>();

            foreach (var dateTime in timeSeriesData.DateTimes)
            {
                switch (movingAggregationType)
                {
                    case MovingAggregationType.Backwards:
                        values.Append(dateTime, timeSeriesData.Average(dateTime.Subtract(timeSpan), dateTime));
                        break;
                    case MovingAggregationType.Forward:
                        values.Append(dateTime, timeSeriesData.Average(dateTime, dateTime.Add(timeSpan)));
                        break;
                    case MovingAggregationType.Middle:
                        values.Append(dateTime, timeSeriesData.Average(dateTime.AddDays(-timeSpan.TotalDays / 2), dateTime.AddDays(timeSpan.TotalDays / 2)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(movingAggregationType), movingAggregationType, null);
                }
            }

            return values;
        }

        /// <summary>
        ///     Calculates the moving minimum over a given time span. The moving aggregation type determines if data before, after or
        ///     in the middle should be used.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeSpan">The time span between time steps.</param>
        /// <param name="movingAggregationType">The rolling aggregation type</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> MovingMinimum(this ITimeSeriesData<double> timeSeriesData, TimeSpan timeSpan, MovingAggregationType movingAggregationType)
        {
            if (timeSeriesData.Values.First() == null)
            {
                throw new Exception("The value of the first time step is null. This is not allowed.");
            }

            if (timeSeriesData.Values.Last() == null)
            {
                throw new Exception("The value of the last time step is null. This is not allowed.");
            }

            var values = new TimeSeriesData<double>();

            foreach (var dateTime in timeSeriesData.DateTimes)
            {
                switch (movingAggregationType)
                {
                    case MovingAggregationType.Backwards:
                        values.Append(dateTime, timeSeriesData.Get(dateTime.Subtract(timeSpan), dateTime).Minimum());
                        break;
                    case MovingAggregationType.Forward:
                        values.Append(dateTime, timeSeriesData.Get(dateTime, dateTime.Add(timeSpan)).Minimum());
                        break;
                    case MovingAggregationType.Middle:
                        values.Append(dateTime, timeSeriesData.Get(dateTime.AddDays(-timeSpan.TotalDays / 2), dateTime.AddDays(timeSpan.TotalDays / 2)).Minimum());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(movingAggregationType), movingAggregationType, null);
                }
            }

            return values;
        }

        /// <summary>
        ///     Calculates the moving maximum over a given time span. The moving aggregation type determines if data before, after or
        ///     in the middle should be used.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeSpan">The time span between time steps.</param>
        /// <param name="movingAggregationType">The rolling aggregation type</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> MovingMaximum(this ITimeSeriesData<double> timeSeriesData, TimeSpan timeSpan, MovingAggregationType movingAggregationType)
        {
            if (timeSeriesData.Values.First() == null)
            {
                throw new Exception("The value of the first time step is null. This is not allowed.");
            }

            if (timeSeriesData.Values.Last() == null)
            {
                throw new Exception("The value of the last time step is null. This is not allowed.");
            }

            var values = new TimeSeriesData<double>();

            foreach (var dateTime in timeSeriesData.DateTimes)
            {
                switch (movingAggregationType)
                {
                    case MovingAggregationType.Backwards:
                        values.Append(dateTime, timeSeriesData.Get(dateTime.Subtract(timeSpan), dateTime).Maximum());
                        break;
                    case MovingAggregationType.Forward:
                        values.Append(dateTime, timeSeriesData.Get(dateTime, dateTime.Add(timeSpan)).Maximum());
                        break;
                    case MovingAggregationType.Middle:
                        values.Append(dateTime, timeSeriesData.Get(dateTime.AddDays(-timeSpan.TotalDays / 2), dateTime.AddDays(timeSpan.TotalDays / 2)).Maximum());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(movingAggregationType), movingAggregationType, null);
                }
            }

            return values;
        }

        /// <summary>
        ///     Return the value below which the input percentage of all values in time series data fall.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="percentile">The percentile.</param>
        /// <returns>The value below which the percentage of input values fall.</returns>
        public static double? PercentileValue(this ITimeSeriesData<double> timeSeriesData, int percentile)
        {
            if (percentile <= 0 || percentile > 100)
            {
                throw new ArgumentException("The percentile should be higher than 0 and lower than 100.");
            }

            if (timeSeriesData.Values.All(v => v == null))
            {
                return null;
            }

            var sortedValues = timeSeriesData.Values.Where(v => v.HasValue).OrderBy(v => v).ToList();
            var rank = percentile / 100.0 * sortedValues.Count;
            var ordinalRank = (int)Math.Ceiling(rank);
            return sortedValues.ElementAt(ordinalRank - 1);
        }

        /// <summary>
        ///     Returns new time series data with regular interval and values from the original data when available.
        ///     Values are copied from the original time series data when the new time step coincides precisely with a time step in
        ///     the original data.
        ///     The fill value is used at points that have no coincident point in the original data.
        ///     Points that are in the original data but do not coincide with any of the time steps in the new data are ignored.
        ///     No interpolation is performed.
        ///     If the original data is empty and no start time is specified, the result will have no entry.
        ///     If the original data is empty and a start time is specified, the result will have a single entry.
        /// </summary>
        /// <param name="data">The time series data</param>
        /// <param name="timeSpan">Regular interval to be used in the new data</param>
        /// <param name="fillValue">Value to use at time steps that do not coincide with any time step in the original data.</param>
        /// <param name="startTime">
        ///     First time for the new data. If null (default), the first time available in the original data
        ///     will be used.
        /// </param>
        /// <param name="endTime">
        ///     Last time for the new data. If null (default), the last time available in the original data will
        ///     be used. If the specified end time cannot be reached using a regular time step from the first time in the new data,
        ///     the last time in the new data will be the last time that can be reached using a regular time step before the
        ///     specified end time.
        /// </param>
        public static ITimeSeriesData<TValue> ToEquidistant<TValue>(this ITimeSeriesData<TValue> data, TimeSpan timeSpan, TValue? fillValue = null, DateTime? startTime = null, DateTime? endTime = null) where TValue : struct
        {
            var dataFilled = new TimeSeriesData<TValue>();

            if (startTime == null)
            {
                var maybeFirst = data.GetFirstDateTime();
                if (maybeFirst.HasValue)
                {
                    startTime = maybeFirst.Value;
                }
            }

            if (endTime == null)
            {
                var maybeLast = data.GetLastDateTime();
                if (maybeLast.HasValue)
                {
                    endTime = maybeLast.Value;
                }
            }

            if (data.DateTimes.Any())
            {
                var t = startTime.Value;
                while (t <= endTime)
                {
                    var maybe = data.Get(t);
                    var v = maybe.HasValue ? maybe.Value.Value : fillValue;
                    dataFilled.Append(t, v);
                    t = t.Add(timeSpan);
                }
            }
            else
            {
                return CreateNewTimeSeriesDataWithRegularTimeStepAndConstantValue(startTime, endTime, timeSpan, fillValue);
            }

            return dataFilled;
        }

        /// <summary>
        ///     Groups and calculates the sum of time series data values over a period.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <returns>A time series with values summarized over the given period.</returns>
        public static ITimeSeriesData<double> Sum(this ITimeSeriesData<double> data, Period period)
        {
            var sortedSet = new SortedSet<DataPoint<double>>();
            foreach (var group in data.GroupBy(period))
            {
                sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Sum(r => r.Value)));
            }

            return new TimeSeriesData<double>(sortedSet);
        }

        /// <summary>
        ///     Groups and calculates the average of time series data values over a period.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <returns>A time series with average values over the given period.</returns>
        public static ITimeSeriesData<double> Average(this ITimeSeriesData<double> data, Period period)
        {
            var sortedSet = new SortedSet<DataPoint<double>>();
            foreach (var group in data.GroupBy(period))
            {
                sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Average(r => r.Value)));
            }

            return new TimeSeriesData<double>(sortedSet);
        }

        /// <summary>
        ///     Groups and calculates the minimum of time series data values over a period.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <returns>A time series with minimum values over the given period.</returns>
        public static ITimeSeriesData<double> Minimum(this ITimeSeriesData<double> data, Period period)
        {
            var sortedSet = new SortedSet<DataPoint<double>>();
            foreach (var group in data.GroupBy(period))
            {
                sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Min(r => r.Value)));
            }

            return new TimeSeriesData<double>(sortedSet);
        }

        /// <summary>
        ///     Groups and calculates the maximum of time series data values over a period.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <returns>A time series with maximum values over the given period.</returns>
        public static ITimeSeriesData<double> Maximum(this ITimeSeriesData<double> data, Period period)
        {
            var sortedSet = new SortedSet<DataPoint<double>>();
            foreach (var group in data.GroupBy(period))
            {
                sortedSet.Add(new DataPoint<double>(group.Key, group.Where(r => r.Value.HasValue).Max(r => r.Value)));
            }

            return new TimeSeriesData<double>(sortedSet);
        }

        /// <summary>
        ///     Calculates the average value in each time step for the list of time series data.
        /// </summary>
        /// <param name="dataList">The list of time series data.</param>
        /// <returns>A time series with the average value in each time step.</returns>
        public static ITimeSeriesData<double> Average(this IList<ITimeSeriesData<double>> dataList)
        {
            var dateTimes = new List<DateTime>();
            foreach (var list in dataList)
            {
                dateTimes.AddRange(list.DateTimes);
            }

            dateTimes = dateTimes.Distinct().OrderBy(r => r).ToList();

            var dictionaryData = new SortedDictionary<DateTime, double>();
            var dictionaryCount = new SortedDictionary<DateTime, int>();
            foreach (var dateTime in dateTimes)
            {
                dictionaryData.Add(dateTime, 0);
                dictionaryCount.Add(dateTime, 0);
            }

            var sortedLists = dataList.Select(r => r.ToSortedSet());
            foreach (var list in sortedLists)
            {
                foreach (var point in list)
                {
                    if (point.Value.HasValue)
                    {
                        dictionaryData[point.DateTime] += point.Value.Value;
                        dictionaryCount[point.DateTime] += 1;
                    }
                }
            }

            var dictionary = new SortedDictionary<DateTime, double>();
            foreach (var data in dictionaryData)
            {
                dictionary.Add(data.Key, data.Value / dictionaryCount[data.Key]);
            }

            return new TimeSeriesData<double>(dictionary.Select(r => r.Key).ToList(), dictionary.Select(r => r.Value).Cast<double?>().ToList());
        }

        /// <summary>
        ///     Calculates the sum in each time step for the list of time series data.
        /// </summary>
        /// <param name="dataList">The list of time series data.</param>
        /// <returns>A time series with the sum in each time step.</returns>
        public static ITimeSeriesData<double> Sum(this IList<ITimeSeriesData<double>> dataList)
        {
            var dateTimes = new List<DateTime>();
            foreach (var list in dataList)
            {
                dateTimes.AddRange(list.DateTimes);
            }

            dateTimes = dateTimes.Distinct().OrderBy(r => r).ToList();

            var dictionary = new SortedDictionary<DateTime, double>();
            foreach (var dateTime in dateTimes)
            {
                dictionary.Add(dateTime, 0);
            }

            var sortedLists = dataList.Select(r => r.ToSortedSet());
            foreach (var list in sortedLists)
            {
                foreach (var point in list)
                {
                    if (point.Value.HasValue)
                    {
                        dictionary[point.DateTime] += point.Value.Value;
                    }
                }
            }

            return new TimeSeriesData<double>(dictionary.Select(r => r.Key).ToList(), dictionary.Select(r => r.Value).Cast<double?>().ToList());
        }

        /// <summary>
        ///     Calculates the minimum value in each time step for the given list of time series data.
        /// </summary>
        /// <param name="dataList">The list of time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <param name="deleteValue">A value inserted in the case of a gap tolerance violation.</param>
        /// <param name="timeSeriesDataType">Type of the time series data.</param>
        /// <returns>A time series with the minimum value in each time step.</returns>
        public static (ITimeSeriesData<double> data, int interpolatedCount) Min(this IList<ITimeSeriesData<double>> dataList, TimeStepsSelection timeStepsSelection = TimeStepsSelection.All,
            TimeSpan? gapTolerance = null,
            double? deleteValue = null,
            TimeSeriesDataType timeSeriesDataType = null)
        {
            if (timeSeriesDataType is null)
            {
                timeSeriesDataType = TimeSeriesDataType.Instantaneous;
            }

            var points = new SortedSet<DataPoint<double>>();

            var timeSteps = dataList.GetTimeSteps(timeStepsSelection);
            var interpolated = 0;
            foreach (var dateTime in timeSteps)
            {
                var list = new List<double>();
                foreach (var data in dataList)
                {
                    var (point, isInterpolated) = data.GetInterpolated(dateTime, timeSeriesDataType, gapTolerance);
                    interpolated += isInterpolated ? 1 : 0;
                    if (point.Value.HasValue)
                    {
                        list.Add(point.Value.Value);
                    }
                }

                points.Add(new DataPoint<double>(dateTime, list.Count > 0 ? list.Min() : deleteValue));
            }

            return (new TimeSeriesData<double>(points), interpolated);
        }

        /// <summary>
        ///     Calculates the maximum value in each time step for the given list of time series data.
        /// </summary>
        /// <param name="dataList">The list of time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <param name="deleteValue">A value inserted in the case of a gap tolerance violation.</param>
        /// <param name="timeSeriesDataType">Type of the time series data.</param>
        /// <returns>A time series with the maximum value in each time step.</returns>
        public static (ITimeSeriesData<double> data, int interpolatedCount) Max(this IList<ITimeSeriesData<double>> dataList, TimeStepsSelection timeStepsSelection = TimeStepsSelection.All,
            TimeSpan? gapTolerance = null,
            double? deleteValue = null,
            TimeSeriesDataType timeSeriesDataType = null)
        {
            if (timeSeriesDataType is null)
            {
                timeSeriesDataType = TimeSeriesDataType.Instantaneous;
            }

            var points = new SortedSet<DataPoint<double>>();

            var timeSteps = dataList.GetTimeSteps(timeStepsSelection);
            var interpolated = 0;
            foreach (var dateTime in timeSteps)
            {
                var list = new List<double>();
                foreach (var data in dataList)
                {
                    var (point, isInterpolated) = data.GetInterpolated(dateTime, timeSeriesDataType, gapTolerance);
                    interpolated += isInterpolated ? 1 : 0;
                    if (point.Value.HasValue)
                    {
                        list.Add(point.Value.Value);
                    }
                }

                points.Add(new DataPoint<double>(dateTime, list.Count > 0 ? list.Max() : deleteValue));
            }

            return (new TimeSeriesData<double>(points), interpolated);
        }

        /// <summary>
        ///     Multiplies some time series values with some other time series values in the same time steps.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <returns>
        ///     A value tuple with the multiplied time series data and a count of found corresponding times and not found
        ///     corresponding times.
        /// </returns>
        public static (ITimeSeriesData<double> data, int foundCount, int notFoundCount) MultiplyWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other)
        {
            return data.OperationWith(other, Operation.Multiply);
        }

        /// <summary>
        ///     Adds some time series values with some other time series values in the same time steps.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <returns>
        ///     A value tuple with the added time series data and a count of found corresponding times and not found
        ///     corresponding times.
        /// </returns>
        public static (ITimeSeriesData<double> data, int foundCount, int notFoundCount) AddWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other)
        {
            return data.OperationWith(other, Operation.Add);
        }

        /// <summary>
        ///     Subtract some time series values with some other time series values in the same time steps.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <returns>
        ///     A value tuple with the subtracted time series data and a count of found corresponding times and not found
        ///     corresponding times.
        /// </returns>
        public static (ITimeSeriesData<double> data, int foundCount, int notFoundCount) SubtractWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other)
        {
            return data.OperationWith(other, Operation.Subtract);
        }

        /// <summary>
        ///     Divide some time series values with some other time series values in the same time steps.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <returns>
        ///     A value tuple with the divided time series data and a count of found corresponding times and not found
        ///     corresponding times.
        /// </returns>
        public static (ITimeSeriesData<double> data, int foundCount, int notFoundCount) DivideWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other)
        {
            return data.OperationWith(other, Operation.Divide);
        }

        /// <summary>
        ///     Multiplies time series data with time series data from another time series.
        ///     The time steps selection decides whether the resulting time axis should include the time steps from the first
        ///     time series only, the common time steps only, or all time steps.
        ///     If the gap tolerance time span is given, values will only be interpolated over intervals less than or equal to the
        ///     given time span. Otherwise, the delete value will be inserted.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <param name="deleteValue">A value inserted in the case of a gap tolerance violation.</param>
        /// <param name="timeSeriesDataType">Type of the time series data.</param>
        /// <returns>A value tuple with the resulting time series data and counts of the number of interpolations.</returns>
        public static (ITimeSeriesData<double> data, int interpolatedCount, int InterpolatedCountOther) MultiplyWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other, TimeStepsSelection timeStepsSelection,
            TimeSpan? gapTolerance = null,
            double? deleteValue = null,
            TimeSeriesDataType timeSeriesDataType = null)
        {
            if (timeSeriesDataType is null)
            {
                timeSeriesDataType = TimeSeriesDataType.Instantaneous;
            }

            return data.OperationWith(other, (v1, v2) => v1 * v2, timeStepsSelection, gapTolerance, deleteValue, timeSeriesDataType);
        }

        /// <summary>
        ///     Adds time series data with time series data from another time series.
        ///     The time steps selection decides whether the resulting time axis should include the time steps from the first
        ///     time series only, the common time steps only, or all time steps.
        ///     If the gap tolerance time span is given, values will only be interpolated over intervals less than or equal to the
        ///     given time span. Otherwise, the delete value will be inserted.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <param name="deleteValue">A value inserted in the case of a gap tolerance violation.</param>
        /// <param name="timeSeriesDataType">Type of the time series data.</param>
        /// <returns>A value tuple with the resulting time series data and counts of the number of interpolations.</returns>
        public static (ITimeSeriesData<double> data, int interpolatedCount, int InterpolatedCountOther) AddWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other, TimeStepsSelection timeStepsSelection,
            TimeSpan? gapTolerance = null,
            double? deleteValue = null,
            TimeSeriesDataType timeSeriesDataType = null)
        {
            if (timeSeriesDataType is null)
            {
                timeSeriesDataType = TimeSeriesDataType.Instantaneous;
            }

            return data.OperationWith(other, (v1, v2) => v1 + v2, timeStepsSelection, gapTolerance, deleteValue, timeSeriesDataType);
        }

        /// <summary>
        ///     Subtract time series data with time series data from another time series.
        ///     The time steps selection decides whether the resulting time axis should include the time steps from the first
        ///     time series only, the common time steps only, or all time steps.
        ///     If the gap tolerance time span is given, values will only be interpolated over intervals less than or equal to the
        ///     given time span. Otherwise, the delete value will be inserted.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <param name="deleteValue">A value inserted in the case of a gap tolerance violation.</param>
        /// <param name="timeSeriesDataType">Type of the time series data.</param>
        /// <returns>A value tuple with the resulting time series data and counts of the number of interpolations.</returns>
        public static (ITimeSeriesData<double> data, int interpolatedCount, int InterpolatedCountOther) SubtractWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other, TimeStepsSelection timeStepsSelection,
            TimeSpan? gapTolerance = null,
            double? deleteValue = null,
            TimeSeriesDataType timeSeriesDataType = null)
        {
            if (timeSeriesDataType is null)
            {
                timeSeriesDataType = TimeSeriesDataType.Instantaneous;
            }

            return data.OperationWith(other, (v1, v2) => v1 - v2, timeStepsSelection, gapTolerance, deleteValue, timeSeriesDataType);
        }

        /// <summary>
        ///     Divides time series data with time series data from another time series.
        ///     The time steps selection decides whether the resulting time axis should include the time steps from the first
        ///     time series only, the common time steps only, or all time steps.
        ///     If the gap tolerance time span is given, values will only be interpolated over intervals less than or equal to the
        ///     given time span. Otherwise, the delete value will be inserted.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <param name="timeStepsSelection">The time steps selection.</param>
        /// <param name="gapTolerance">The gap tolerance.</param>
        /// <param name="deleteValue">A value inserted in the case of a gap tolerance violation.</param>
        /// <param name="timeSeriesDataType">Type of the time series data.</param>
        /// <returns>A value tuple with the resulting time series data and counts of the number of interpolations.</returns>
        public static (ITimeSeriesData<double> data, int interpolatedCount, int InterpolatedCountOther) DivideWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other, TimeStepsSelection timeStepsSelection,
            TimeSpan? gapTolerance = null,
            double? deleteValue = null,
            TimeSeriesDataType timeSeriesDataType = null)
        {
            if (timeSeriesDataType is null)
            {
                timeSeriesDataType = TimeSeriesDataType.Instantaneous;
            }

            return data.OperationWith(other, (v1, v2) => v1 / v2, timeStepsSelection, gapTolerance, deleteValue, timeSeriesDataType);
        }

        /// <summary>
        ///     Replaces all values below or above the given limits with the given value.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="below">The lower limit.</param>
        /// <param name="above">The upper limit.</param>
        /// <param name="value">The replacement value (can be null)</param>
        /// <returns>A value tuple with the resulting time series and a count of the number of replacements.</returns>
        public static (ITimeSeriesData<double> data, int replacedCount) Replace(this ITimeSeriesData<double> data, double? value, double? below = null, double? above = null)
        {
            if (above <= below)
            {
                throw new ArgumentException($"Upper limit '{above}' must be larger than the lower limit '{below}'.", nameof(above));
            }

            if (below is null && above is null)
            {
                throw new ArgumentException("Either below or above must be given. They cannot both be null.");
            }

            var points = new SortedSet<DataPoint<double>>();
            var replacedCount = 0;
            foreach (var point in data.ToSortedSet())
            {
                if (point.Value.HasValue && (point.Value < below || point.Value > above))
                {
                    points.Add(new DataPoint<double>(point.DateTime, value));
                    replacedCount++;
                }
                else
                {
                    points.Add(new DataPoint<double>(point.DateTime, point.Value));
                }
            }

            return (new TimeSeriesData<double>(points), replacedCount);
        }

        /// <summary>
        ///     Replaces any occurence of the given value old value with the new value.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>A value tuple with the resulting time series and a count of the number of replacements.</returns>
        public static (ITimeSeriesData<double> data, int replacedCount) Replace(this ITimeSeriesData<double> data, double oldValue, double? newValue)
        {
            var points = new SortedSet<DataPoint<double>>();
            var replacedCount = 0;
            foreach (var point in data.ToSortedSet())
            {
                if (Equals(point.Value, oldValue))
                {
                    points.Add(new DataPoint<double>(point.DateTime, newValue));
                    replacedCount++;
                }
                else
                {
                    points.Add(new DataPoint<double>(point.DateTime, point.Value));
                }
            }

            return (new TimeSeriesData<double>(points), replacedCount);
        }

        /// <summary>
        ///     Allows for operations Add, Subtract, Multiply and Divide of some time series values with some other time series
        ///     values in the same time steps.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <param name="operation">Operation to perform</param>
        /// <returns>
        ///     A value tuple with the processed time series data and a count of found corresponding times and not found
        ///     corresponding times.
        /// </returns>
        internal static (ITimeSeriesData<double> data, int foundCount, int notFoundCount) OperationWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other, Operation operation)
        {
            var points = new SortedSet<DataPoint<double>>();
            var otherPoints = other.ToSortedSet();
            var foundCount = 0;
            var notFoundCount = 0;
            foreach (var point in data.ToSortedSet())
            {
                var lookedUp = otherPoints.FirstOrDefault(r => r.DateTime == point.DateTime);
                if (lookedUp != null)
                {
                    switch (operation)
                    {
                        case Operation.Add:
                            points.Add(new DataPoint<double>(point.DateTime, point.Value + (lookedUp.Value ?? 0)));
                            break;
                        case Operation.Subtract:
                            points.Add(new DataPoint<double>(point.DateTime, point.Value - (lookedUp.Value ?? 0)));
                            break;
                        case Operation.Multiply:
                            points.Add(new DataPoint<double>(point.DateTime, point.Value * (lookedUp.Value ?? 1)));
                            break;
                        case Operation.Divide:
                            points.Add(new DataPoint<double>(point.DateTime, point.Value / (lookedUp.Value ?? 1)));
                            break;
                    }

                    foundCount++;
                }
                else
                {
                    notFoundCount++;
                }
            }

            return (new TimeSeriesData<double>(points), foundCount, notFoundCount);
        }

        internal static (ITimeSeriesData<double> data, int interpolated1Count, int interpolated2Count) OperationWith(this ITimeSeriesData<double> data1,
            ITimeSeriesData<double> data2,
            Func<double, double, double> operation,
            TimeStepsSelection timeStepsSelection,
            TimeSpan? gapTolerance,
            double? deleteValue,
            TimeSeriesDataType timeSeriesDataType)
        {
            var points = new SortedSet<DataPoint<double>>();
            var interpolated1Count = 0;
            var interpolated2Count = 0;

            var timeSteps = new List<ITimeSeriesData<double>> {data1, data2}.GetTimeSteps(timeStepsSelection);
            foreach (var dateTime in timeSteps)
            {
                var (point1, isInterpolated1) = data1.GetInterpolated(dateTime, timeSeriesDataType, gapTolerance);
                var (point2, isInterpolated2) = data2.GetInterpolated(dateTime, timeSeriesDataType, gapTolerance);

                if (point1.Value is null || point2.Value is null)
                {
                    points.Add(new DataPoint<double>(dateTime, deleteValue));
                }
                else
                {
                    points.Add(new DataPoint<double>(dateTime, operation((double)point1.Value, (double)point2.Value)));
                }

                if (isInterpolated1)
                {
                    interpolated1Count++;
                }

                if (isInterpolated2)
                {
                    interpolated2Count++;
                }
            }

            return (new TimeSeriesData<double>(points), interpolated1Count, interpolated2Count);
        }

        /// <summary>
        ///     Create a new time series data with regular time step and filled with a constant value.
        ///     If start and end time are not specified, the result will have no data points.
        ///     If only start or only end time are specified, result will have a single data point with the specified time.
        /// </summary>
        /// <param name="from">Start time and starting point for all intervals.</param>
        /// <param name="to">End time, no data will be added after this time.</param>
        /// <param name="timeSpan">Interval</param>
        /// <param name="value">Use this value for all time steps in the new time series data</param>
        /// <returns>Time series data with regular time step and filled with a constant value</returns>
        private static ITimeSeriesData<TValue> CreateNewTimeSeriesDataWithRegularTimeStepAndConstantValue<TValue>(DateTime? from, DateTime? to, TimeSpan timeSpan, TValue? value = null) where TValue : struct
        {
            var timeSeriesData = new TimeSeriesData<TValue>();

            if (from != null && to != null)
            {
                var t = from.Value;
                while (t <= to)
                {
                    timeSeriesData.Append(t, value);
                    t = t.Add(timeSpan);
                }
            }
            else
            {
                if (from != null)
                {
                    timeSeriesData.Append(from.Value, value);
                }

                if (to != null)
                {
                    timeSeriesData.Append(to.Value, value);
                }
            }

            return timeSeriesData;
        }
    }
}