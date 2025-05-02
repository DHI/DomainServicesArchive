namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Time series data extension methods for creation of a duration curve.
    /// </summary>
    public static class DurationCurveAnalysis
    {
        /// <summary>
        ///     Duration curve.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="durationInHours">The duration in hours.</param>
        /// <param name="numberOfIntervals">The number of value intervals.</param>
        /// <param name="minNumberOfValues">
        ///     The minimum number of values accepted.
        ///     If time series data does not contain enough values (!= null) an exception is thrown.
        /// </param>
        public static SortedDictionary<double, double> DurationCurve(this ITimeSeriesData<double> timeSeriesData, double durationInHours, int numberOfIntervals = 10, int minNumberOfValues = 100)
        {
            var minValue = timeSeriesData.Minimum();
            var maxValue = timeSeriesData.Maximum();
            if (!minValue.HasValue || !maxValue.HasValue || maxValue == minValue)
            {
                throw new Exception("No values in the time series data, or the values are constant.");
            }

            if (timeSeriesData.Values.Count(v => v != null) < minNumberOfValues)
            {
                throw new Exception($"Less than {minNumberOfValues} time series data values that are not null.");
            }

            var durationCount = new Dictionary<Interval<double>, int>();
            var windows = new Dictionary<Interval<double>, Window<double>>();

            // Establish threshold values. 
            var valueSpan = maxValue.Value - minValue.Value;
            var thresholdValues = new ThresholdValues(minValue.Value - 0.01*valueSpan, maxValue.Value + 0.01*valueSpan, numberOfIntervals - 1);
            var intervals = thresholdValues.Intervals.ToList();

            // Establish Windows
            var currentTime = default(DateTime);
            foreach (var interval in intervals)
            {
                var window = new Window<double>(interval);
                window.ActiveChanged += (s, e) =>
                {
                    if (window.Active)
                    {
                        // Initialize
                        window.StartTime = currentTime;
                    }
                    else
                    {
                        // Accumulate window duration count
                        var timeSpan = currentTime - window.StartTime;
                        var count = (int)(timeSpan.TotalHours/durationInHours);
                        if (count >= 1)
                        {
                            if (durationCount.ContainsKey(window.Interval))
                            {
                                durationCount[window.Interval] = durationCount[window.Interval] + count;
                            }
                            else
                            {
                                durationCount.Add(window.Interval, count);
                            }
                        }
                    }
                };

                windows.Add(interval, window);
            }

            // Run analysis
            var previousInterval = new Interval<double>(double.MinValue, double.MinValue);
            foreach (var dataPoint in timeSeriesData.ToSortedSet())
            {
                currentTime = dataPoint.DateTime;
                var currentInterval = thresholdValues.GetInterval(dataPoint.Value);
                if (currentInterval != null)
                {
                    // skip time step if same interval as previous
                    if (currentInterval != previousInterval)
                    {
                        previousInterval = currentInterval;
                        windows[currentInterval].Active = true;

                        // All below are active
                        foreach (var interval in intervals.Where(i => i.Start < currentInterval.Start))
                        {
                            windows[interval].Active = true;
                        }

                        // All above are active
                        foreach (var interval in intervals.Where(i => i.Start > currentInterval.Start))
                        {
                            windows[interval].Active = false;
                        }
                    }
                }
            }

            // Finalize properly
            foreach (var keyValuePair in windows)
            {
                keyValuePair.Value.Active = false;
            }

            // Establish duration curve
            var durationCurve = new SortedDictionary<double, double>();
            var totalTimeSpan = timeSeriesData.TimeSpan();
            foreach (var interval in intervals.Where(i => Math.Abs(i.Start - double.MinValue) > 1e-10)) // Skip first interval
            {
                var lowerValue = interval.Start;
                var notExceededFraction = 0d;
                if (durationCount.ContainsKey(interval))
                {
                    var count = durationCount[interval];
                    var totalDurationInTicks = count*durationInHours*3600*10e6;
                    var durationTimeSpan = new TimeSpan((long)totalDurationInTicks);
                    notExceededFraction = durationTimeSpan.TotalHours/totalTimeSpan.TotalHours;
                }

                durationCurve.Add(lowerValue, notExceededFraction);
            }

            return durationCurve;
        }

        /// <summary>
        ///     Duration curve, with set static probabilities, to measure extreme values
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        public static SortedDictionary<double, double> DurationCurve(this ITimeSeriesData<double> timeSeriesData)
        {
            var probabilities = new []
            {
                0.0, 0.0001, 0.001, 0.005, 0.01, 0.02, 0.05, 0.08,
                0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5,
                0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9,
                0.92, 0.95, 0.98, 0.99, 0.995, 0.999, 0.9999, 1.0 - 1e-10
            };

            var sortedValues = timeSeriesData.Values.Where(v => v.HasValue).OrderBy(v => v).ToList();
            var durationCurve = new SortedDictionary<double, double>();
            foreach (var probability in probabilities.OrderByDescending(d => d))
            {
                var rank = (1 - probability) * sortedValues.Count;
                var ordinalRank = (int)Math.Ceiling(rank);
                var value = ordinalRank == 0 ? sortedValues.ElementAt(ordinalRank) : sortedValues.ElementAt(ordinalRank - 1);
                durationCurve.Add(probability, (double)value);
            }

            return durationCurve;
        }
    }
}