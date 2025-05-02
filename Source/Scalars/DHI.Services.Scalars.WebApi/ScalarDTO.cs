namespace DHI.Services.Scalars.WebApi
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using WebApiCore;

    /// <summary>
    ///     Data transfer object for scalar resource representation
    /// </summary>
    public class ScalarDTO
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScalarDTO" /> class.
        /// </summary>
        public ScalarDTO()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScalarDTO" /> class.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        public ScalarDTO(Scalar<string, int> scalar)
        {
            FullName = scalar.FullName;
            ValueTypeName = scalar.ValueTypeName;
            Description = scalar.Description;
            Locked = scalar.Locked;
            var maybe = scalar.GetData();
            if (maybe.HasValue)
            {
                var data = maybe.Value;
                Value = Convert.ToString(data.Value, CultureInfo.InvariantCulture);
                DateTime = data.DateTime;
                Flag = data.Flag;
            }
        }

        /// <summary>
        ///     Gets or sets the fullname.
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the value type.
        /// </summary>
        /// <value>The name of the value type.</value>
        [Required]
        public string ValueTypeName { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the scalar is locked.
        /// </summary>
        /// <value><c>true</c> if locked; otherwise, <c>false</c>.</value>
        public bool Locked { get; set; }

        /// <summary>
        ///     Gets or sets the data value.
        /// </summary>
        /// <value>The data value.</value>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the data time stamp.
        /// </summary>
        /// <value>The data time stamp.</value>
        public DateTime? DateTime { get; set; }

        /// <summary>
        ///     Gets or sets the data flag.
        /// </summary>
        /// <value>The data flag.</value>
        public int? Flag { get; set; }

        /// <summary>
        ///     Converts the DTO to a Scalar object.
        /// </summary>
        public Scalar<string, int> ToScalar()
        {
            var fullName = DHI.Services.FullName.Parse(FullName);
            var scalar = new Scalar<string, int>(fullName.ToString(), fullName.Name, ValueTypeName, fullName.Group)
            {
                Description = Description,
                Locked = Locked
            };

            if (!(Value is null || DateTime is null))
            {
                scalar.SetData(new ScalarData<int>(Value.ToObject(), (DateTime)DateTime, Flag));
            }

            return scalar;
        }
    }
}