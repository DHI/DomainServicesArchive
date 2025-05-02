namespace DHI.Services.TimeSeries.Web
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for the IDs of two time series with the X- and Y-component of a variable
    /// </summary>
    public class ComponentsDTO
    {
        /// <summary>
        ///     Gets or sets the ID of the time series with the X-components.
        /// </summary>
        [Required]
        public string X { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the time series with the Y-components.
        /// </summary>
        [Required]
        public string Y { get; set; }

        /// <summary>
        ///     Converts to a value tuple.
        /// </summary>
        public (string x, string y) ToValueTuple()
        {
            return (x: X, y: Y);
        }
    }
}