namespace DHI.Services.TimeSeries
{
    using System;
    using System.Linq;

    public static class GapFilAnalysis
    {
        /// <summary>
        ///     Replaces null values with interpolated values according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        /// </summary>
        /// <param name="timeSeriesData">Time series data to be gap-filled</param>
        /// <param name="dataType">The time series data type.</param>
        /// <returns>ITimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> GapFill(this ITimeSeriesData<double> timeSeriesData, TimeSeriesDataType dataType = null)
        {
            if (dataType == null)
            {
                dataType = TimeSeriesDataType.Instantaneous;
            }

            var timeSeriesDataFilled = new TimeSeriesData<double>();
            for (var i = 0; i < timeSeriesData.DateTimes.Count; i++)
            {
                timeSeriesDataFilled.Append(timeSeriesData.DateTimes[i], timeSeriesData.Values[i]);

                if (!timeSeriesData.Values[i].HasValue)
                {
                    timeSeriesDataFilled.Values[i] = timeSeriesData.GetInterpolated(timeSeriesData.DateTimes[i], dataType).Value;
                }
            }

            return timeSeriesDataFilled;
        }

        /// <summary>
        ///     Fills the time series with the given value within the given interval (startTime to endTime) at time steps with the
        ///     given time span.
        ///     If a time step already exists, no value is inserted.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="value">The value.</param>
        /// <returns>A value tuple with the gap-filled time series data and a count of skipped time steps and inserted values.</returns>
        public static (ITimeSeriesData<double> data, int SkippedCount, int InsertedCount) GapFill(this ITimeSeriesData<double> data, DateTime startTime, DateTime endTime, TimeSpan timeSpan, double? value)
        {
            if (endTime <= startTime)
            {
                throw new ArgumentException($"End time '{endTime}' must be larger than the start time '{startTime}'.", nameof(endTime));
            }

            var points = data.ToSortedSet();
            var insertedCount = 0;
            var skippedCount = 0;
            var dateTime = startTime;
            while (dateTime < endTime)
            {
                if (points.FirstOrDefault(r => r.DateTime == dateTime) != null)
                {
                    skippedCount++;
                }
                else
                {
                    points.Add(new DataPoint<double>(dateTime, value));
                    insertedCount++;
                }

                dateTime = dateTime.Add(timeSpan);
            }

            return (new TimeSeriesData<double>(points), skippedCount, insertedCount);
        }
    }
}