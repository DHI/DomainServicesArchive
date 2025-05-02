namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;

    public static class StandardDeviationAnalysis
    {
        /// <summary>
        ///     Calculates the standard deviation value of the time series data.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <returns>The standard deviation value of time series data.</returns>
        public static double? StandardDeviation(this ITimeSeriesData<double> data)
        {
            var count = 0;
            double? standardDeviation = null;
            double mean = 0;
            double variance = 0;

            foreach (var item in data.Values)
            {
                if (item.HasValue)
                {
                    count++;
                    var prevMean = mean;
                    mean += (double)(item - prevMean) / count;
                    variance += ((double)item - prevMean) * ((double)item - mean);
                }
            }
            if (count > 0)
            {
                standardDeviation = Math.Sqrt(variance / (count - 1));
            }

            return standardDeviation;
        }

        /// <summary>
        ///     Calculates the standard deviation value of the time series data.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="period">The aggregation period. The supported types are 'Hourly', 'Daily', 'Monthly' and 'Yearly'.</param>
        /// <returns>The standard deviation value of time series data.</returns>
        public static ITimeSeriesData<double> StandardDeviation(this ITimeSeriesData<double> data, Period period)
        {
            var sortedSet = new SortedSet<DataPoint<double>>();
            foreach (var group in data.GroupBy(period))
            {
                var count = 0;
                double? standardDeviation = null;
                double mean = 0;
                double variance = 0;

                foreach (var item in group)
                {
                    if (item.Value.HasValue)
                    {
                        count++;
                        var prevMean = mean;
                        mean += (double)(item.Value - prevMean) / count;
                        variance += ((double)item.Value - prevMean) * ((double)item.Value - mean);
                    }
                }
                if (count > 0)
                {
                    standardDeviation = Math.Sqrt(variance / (count - 1));
                }
                sortedSet.Add(new DataPoint<double>(group.Key, standardDeviation));
            }

            return new TimeSeriesData<double>(sortedSet);
        }
    }
}