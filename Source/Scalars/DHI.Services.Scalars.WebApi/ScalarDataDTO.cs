namespace DHI.Services.Scalars.WebApi
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using WebApiCore;

    /// <summary>
    ///     Data transfer object for scalar data resource representation
    /// </summary>
    public class ScalarDataDTO
    {
        /// <summary>
        ///     Gets or sets the data value.
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the data time stamp.
        /// </summary>
        [Required]
        public DateTime DateTime { get; set; }

        /// <summary>
        ///     Gets or sets the data flag.
        /// </summary>
        public int? Flag { get; set; }

        /// <summary>
        ///     Converts the DTO to a ScalarData object.
        /// </summary>
        public ScalarData<int> ToScalarData()
        {
            return new ScalarData<int>(Value.ToObject(), DateTime, Flag);
        }
    }
}