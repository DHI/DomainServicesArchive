namespace DHI.Services.TimeSeries
{
    using System;
    using System.Linq;

    public static class TimeStepTrendAnalysis
    {
        /// <summary>
        ///     TimeStepTrend provides the difference in values from one time step to next (value[i+1]-value[i] -
        ///     Forward,value[i]-value[i-1] - Backward).
        ///     Positive values indicate upward trend looking forward, negative values indicate downward trend.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeStepTrendType">Determines the direction of trend lookup, forward or backward.</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> TimeStepTrend(this ITimeSeriesData<double> timeSeriesData, TimeStepTrendType timeStepTrendType)
        {
            var timeStepTrend = new TimeSeriesData<double>();

            switch (timeStepTrendType)
            {
                case TimeStepTrendType.Forward:
                    for (var i = 0; i < timeSeriesData.DateTimes.Count - 1; i++)
                    {
                        var dateTime = timeSeriesData.DateTimes[i];
                        var valueCurrent = timeSeriesData.Values[i];
                        var valueNext = timeSeriesData.Values[i + 1];
                        double? value = null;
                        if (valueCurrent.HasValue && valueNext.HasValue)
                        {
                            value = valueNext.Value - valueCurrent.Value;
                        }

                        timeStepTrend.Append(dateTime, value);
                    }

                    timeStepTrend.Append(timeSeriesData.DateTimes.Last(), null);
                    break;
                case TimeStepTrendType.Backwards:
                    timeStepTrend.Append(timeSeriesData.DateTimes.First(), null);

                    for (var i = 1; i < timeSeriesData.DateTimes.Count; i++)
                    {
                        var dateTime = timeSeriesData.DateTimes[i];
                        var valueCurrent = timeSeriesData.Values[i];
                        var valuePrevious = timeSeriesData.Values[i - 1];
                        double? value = null;
                        if (valueCurrent.HasValue && valuePrevious.HasValue)
                        {
                            value = valuePrevious.Value - valueCurrent.Value;
                        }

                        timeStepTrend.Append(dateTime, value);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeStepTrendType), timeStepTrendType, null);
            }

            return timeStepTrend;
        }
    }
}