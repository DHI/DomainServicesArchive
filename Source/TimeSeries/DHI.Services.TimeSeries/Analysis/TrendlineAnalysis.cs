namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;

    public static class TrendlineAnalysis
    {
        /// <summary>
        ///     Calculates and returns a linear trend line.
        /// </summary>
        /// <param name="data">The time series data.</param>
        public static (double slope, double offset, ITimeSeriesData<double>) LinearTrendline(this ITimeSeriesData<double> data)
        {
            var x = 0.0;
            var sumXY = 0.0;
            var sumX = 0.0;
            var sumX2 = 0.0;
            var sumY = 0.0;
            var n = 0;
            for (var i = 0; i < data.Values.Count; i++)
            {
                if (data.Values[i].HasValue)
                {
                    x = (data.DateTimes[i] - data.DateTimes[0]).TotalDays;
                    sumX += x;
                    sumY += data.Values[i].Value;
                    sumX2 += Math.Pow(x, 2);
                    sumXY += x * data.Values[i].Value;
                    n++;
                }
            }

            var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - Math.Pow(sumX, 2));
            var offset = (sumY - slope * sumX) / n;
            return (slope, offset, new TimeSeriesData<double>(new List<DateTime> {data.GetFirstDateTime().Value, data.GetLastDateTime().Value}, new List<double> {offset, slope * x + offset}));
        }
    }
}