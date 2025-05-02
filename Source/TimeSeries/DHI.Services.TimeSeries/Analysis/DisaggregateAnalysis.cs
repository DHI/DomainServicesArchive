namespace DHI.Services.TimeSeries
{
    using System.Collections.Generic;
    using System.Linq;

    public static class DisaggregateAnalysis
    {
        /// <summary>
        ///     Disaggregates an accumulated time series backward
        ///     
        ///     Takes a time series of double values and disaggregates it backward, 
        ///     capturing points where the values decrease or the magnitude of the decrease. 
        ///     It does so by comparing adjacent data points and creating new data points accordingly.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <returns>The disaggregated time series data.</returns>
        public static ITimeSeriesData<double> DisaggregateBackward(this ITimeSeriesData<double> data)
        {
            var points = new SortedSet<DataPoint<double>>();
            var list = data.ToSortedSet().ToList();
            for (var i = 0; i < list.Count - 1; i++)
            {
                if (list[i + 1].Value < list[i].Value)
                {
                    points.Add(new DataPoint<double>(list[i].DateTime, list[i + 1].Value));
                }
                else
                {
                    points.Add(new DataPoint<double>(list[i].DateTime, list[i + 1].Value - list[i].Value));
                }
            }

            return new TimeSeriesData<double>(points);
        }

        /// <summary>
        ///     Disaggregates an accumulated time series forward
        ///     
        ///     Takes a time series of double values and disaggregates it in the forward direction, 
        ///     capturing points where the values decrease or the magnitude of the increase. 
        ///     It does so by comparing adjacent data points and creating new data points accordingly.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <returns>The disaggregated time series data.</returns>
        public static ITimeSeriesData<double> DisaggregateForward(this ITimeSeriesData<double> data)
        {
            var points = new SortedSet<DataPoint<double>>();
            var list = data.ToSortedSet().ToList();
            for (var i = 1; i < list.Count; i++)
            {
                if (list[i].Value < list[i - 1].Value)
                {
                    points.Add(new DataPoint<double>(list[i].DateTime, list[i].Value));
                }
                else
                {
                    points.Add(new DataPoint<double>(list[i].DateTime, list[i].Value - list[i - 1].Value));
                }
            }

            return new TimeSeriesData<double>(points);
        }
    }
}