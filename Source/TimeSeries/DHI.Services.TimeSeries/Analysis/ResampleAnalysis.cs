namespace DHI.Services.TimeSeries
{
    using System;
    using System.Linq;

    public static class ResampleAnalysis
    {
        /// <summary>
        ///     Resamples a time series to a time series with time steps equivalent to the given time span.
        ///     The resampled values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values
        ///     within the given time span.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeSpan">The time span between time steps.</param>
        /// <param name="dataType">The time series data type.</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> Resample(this ITimeSeriesData<double> timeSeriesData, TimeSpan timeSpan, TimeSeriesDataType dataType = null)
        {
            if (timeSpan > timeSeriesData.TimeSpan())
            {
                throw new ArgumentException(
                    $"The given time span '{timeSpan}' is longer than the total time span '{timeSeriesData.TimeSpan()}'of the time series", nameof(timeSpan));
            }

            if (dataType == null)
            {
                dataType = TimeSeriesDataType.Instantaneous;
            }

            var resampled = new TimeSeriesData<double>();
            var timeStep = Equals(dataType, TimeSeriesDataType.StepAccumulated) ? timeSeriesData.DateTimes.First().Add(timeSpan) : timeSeriesData.DateTimes.First();
            while (timeStep <= timeSeriesData.DateTimes.Last())
            {
                resampled.DateTimes.Add(timeStep);
                timeStep = timeStep.Add(timeSpan);
            }

            // Add an extra time step if StepAccumulated
            if (Equals(dataType, TimeSeriesDataType.StepAccumulated))
            {
                resampled.DateTimes.Add(timeStep);
            }

            // Calculate values
            if (Equals(dataType, TimeSeriesDataType.StepAccumulated))
            {
                foreach (var dateTime in resampled.DateTimes)
                {
                    var value = timeSeriesData.Get(dateTime.Subtract(timeSpan), dateTime, includeTo: false).Sum();
                    resampled.Values.Add(value);
                }
            }
            else
            {
                foreach (var dateTime in resampled.DateTimes)
                {
                    var point = timeSeriesData.GetInterpolated(dateTime, dataType);
                    resampled.Values.Add(point.Value);
                }
            }

            return resampled;
        }

        /// <summary>
        ///     Resamples a time series to a time series with time steps equivalent to the given period.
        ///     The resampled values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values
        ///     within the given period.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="period">The period between time steps</param>
        /// <param name="dataType">The time series data type.</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> Resample(this ITimeSeriesData<double> timeSeriesData, Period period, TimeSeriesDataType dataType = null)
        {
            if (dataType == null)
            {
                dataType = TimeSeriesDataType.Instantaneous;
            }

            var resampled = new TimeSeriesData<double>();
            var timeStep = Equals(dataType, TimeSeriesDataType.StepAccumulated) ? timeSeriesData.DateTimes.First(period).Add(period) : timeSeriesData.DateTimes.First(period);
            switch (period)
            {
                case Period.Hourly:
                case Period.Daily:
                {
                    //get nearest full hour after first datetime
                    var hour = Math.Round(timeStep.Minute / 60.0);
                    timeStep = new DateTime(timeStep.Year, timeStep.Month, timeStep.Day, timeStep.Hour, 0, 0).AddHours(hour);
                    break;
                }
                case Period.Weekly:
                    timeStep = timeStep.StartOfWeek();
                    break;
                case Period.Quarterly:
                    timeStep = timeStep.StartOfQuarter();
                    break;
                case Period.Yearly:
                    timeStep = new DateTime(timeStep.Year, 1, 1);
                    break;
            }

            while (timeStep <= timeSeriesData.DateTimes.Last())
            {
                resampled.DateTimes.Add(timeStep);
                timeStep = timeStep.Add(period);
            }

            // Add an extra time step if StepAccumulated
            if (Equals(dataType, TimeSeriesDataType.StepAccumulated))
            {
                resampled.DateTimes.Add(timeStep);
            }

            // Calculate values
            if (Equals(dataType, TimeSeriesDataType.StepAccumulated))
            {
                foreach (var dateTime in resampled.DateTimes)
                {
                    var value = timeSeriesData.Get(dateTime.Subtract(period), dateTime, includeTo: false).Sum();
                    resampled.Values.Add(value);
                }
            }
            else
            {
                foreach (var dateTime in resampled.DateTimes)
                {
                    var point = timeSeriesData.GetInterpolated(dateTime, dataType);
                    resampled.Values.Add(point.Value);
                }
            }

            return resampled;
        }

        /// <summary>
        ///     Resamples a time series to a time series with time steps equivalent to the given time span.
        ///     The resampled values are found using interpolation according to the time series data type.
        ///     If the time series data type is not explicitly defined, the data type 'Instantaneous' is assumed.
        ///     However, if the time series data type is StepAccumulated, the resampled values are found as the sum of values
        ///     within the given time span.
        ///     If the time span is greater than one day it will start resampling from the first time midnight after the first time
        ///     step.
        ///     Else, if the time span is greater than hour it will resample from the first full hour after the start time.
        ///     Else, if the time span is greater than minute it will resample from the first full minute after the start time.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="timeSpan">The time span between time steps.</param>
        /// <param name="dataType">The time series data type.</param>
        /// <returns>TimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> ResampleNiceTimesteps(this ITimeSeriesData<double> timeSeriesData, TimeSpan timeSpan, TimeSeriesDataType dataType = null)
        {
            if (timeSpan > timeSeriesData.TimeSpan())
            {
                throw new ArgumentException(
                    $"The given time span '{timeSpan}' is longer than the total time span '{timeSeriesData.TimeSpan()}'of the time series", nameof(timeSpan));
            }

            if (timeSeriesData.Values.First() == null)
            {
                throw new Exception("The value of the first time step is null. This is not allowed.");
            }

            if (timeSeriesData.Values.Last() == null)
            {
                throw new Exception("The value of the last time step is null. This is not allowed.");
            }

            if (dataType == null)
            {
                dataType = TimeSeriesDataType.Instantaneous;
            }

            var resampled = new TimeSeriesData<double>();
            var timeStep = timeSeriesData.DateTimes.First();
            if (timeSpan.Days > 0)
            {
                // If we have days and its not whole days. Round down and take the next. We always want to start with whole days here.
                timeStep = new DateTime(timeStep.Year, timeStep.Month, timeStep.Day).AddDays(1);
            }
            else if (timeSpan.Hours > 0)
            {
                // If we have hours find the nearest full hour after the start
                var hours = Convert.ToInt32(timeSpan.Hours * Math.Ceiling((double)timeStep.Hour / timeSpan.Hours));
                timeStep = timeStep.Minute != 0 ? new DateTime(timeStep.Year, timeStep.Month, timeStep.Day, 0, 0, 0).AddHours(hours + 1) : new DateTime(timeStep.Year, timeStep.Month, timeStep.Day, 0, 0, 0).AddHours(hours);
            }
            else if (timeSpan.Minutes > 0)
            {
                // If we have minutes find the nearest full hour after the start
                var minutes = Convert.ToInt32(timeSpan.Minutes * Math.Ceiling((double)timeStep.Minute / timeSpan.Minutes));
                timeStep = new DateTime(timeStep.Year, timeStep.Month, timeStep.Day, timeStep.Hour, 0, 0).AddMinutes(minutes);
            }

            //Make sure the first timestep is within the data set
            if (timeStep < timeSeriesData.DateTimes.First())
            {
                timeStep = timeStep.Add(timeSpan);
            }

            // Create time steps
            while (timeStep <= timeSeriesData.DateTimes.Last())
            {
                resampled.DateTimes.Add(timeStep);
                timeStep = timeStep.Add(timeSpan);
            }

            // Add an extra time step if StepAccumulated
            if (Equals(dataType, TimeSeriesDataType.StepAccumulated))
            {
                resampled.DateTimes.Add(timeStep);
            }

            // Calculate values
            if (Equals(dataType, TimeSeriesDataType.StepAccumulated))
            {
                foreach (var dateTime in resampled.DateTimes)
                {
                    var value = timeSeriesData.Get(dateTime.Subtract(timeSpan), dateTime, includeTo: false).Sum();
                    resampled.Values.Add(value);
                }
            }
            else
            {
                foreach (var dateTime in resampled.DateTimes)
                {
                    var point = timeSeriesData.GetInterpolated(dateTime, dataType);
                    resampled.Values.Add(point.Value);
                }
            }

            return resampled;
        }
    }
}