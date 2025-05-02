namespace DHI.Services.TimeSeries.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for a time series data resource representation.
    /// </summary>
    public class TimeSeriesDataDTO
    {
        /// <summary>
        ///     Gets or sets the datetimes.
        /// </summary>
        [Required]
        public IList<DateTime> DateTimes { get; set; }

        /// <summary>
        ///     Gets or sets the values.
        /// </summary>
        [Required]
        public IList<double?> Values { get; set; }

        /// <summary>
        ///     Gets or sets the quality flags.
        /// </summary>
        public IList<object> Flags { get; set; }

        /// <summary>
        ///     Converts the DTO to a TimeSeriesData or TimeSeriesDataWFlag object.
        /// </summary>
        public TimeSeriesData<double> ToTimeSeriesData()
        {
            return Flags == null ? new TimeSeriesData<double>(DateTimes, Values) : new TimeSeriesDataWFlag<double, object>(DateTimes, Values, Flags);
        }
    }
}