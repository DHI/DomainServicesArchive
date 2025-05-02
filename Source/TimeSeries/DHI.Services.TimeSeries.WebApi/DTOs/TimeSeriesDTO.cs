namespace DHI.Services.TimeSeries.WebApi
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for a time series resource representation.
    /// </summary>
    public class TimeSeriesDTO
    {
        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the dimension.
        /// </summary>
        public string Dimension { get; set; }

        /// <summary>
        /// Gets or sets the fullname.
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets the time series data.
        /// </summary>
        public TimeSeriesData<double> Data { get; set; }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        ///     Converts the DTO to a TimeSeries object.
        /// </summary>
        public TimeSeries<string, double> ToTimeSeries()
        {
            var fullName = DHI.Services.FullName.Parse(FullName);
            var timeSeries = new TimeSeries<string, double>(fullName.ToString(), fullName.Name, fullName.Group, Data);
            if (DataType != null)
            {
                timeSeries.DataType = Enumeration.FromDisplayName<TimeSeriesDataType>(DataType);
            }

            timeSeries.Dimension = Dimension;
            timeSeries.Quantity = Quantity;
            timeSeries.Unit = Unit;
            return timeSeries;
        }
    }
}