namespace DHI.Services.Rasters.Zones
{
    using System;

    /// <summary>
    ///     Class Weight.
    /// </summary>
    public class Weight
    {
        private double _value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Weight" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if value is not between 0 and 1.</exception>
        public Weight(double value)
        {
            Value = value;
        }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if value is not between 0 and 1.</exception>
        public double Value
        {
            get => _value;

            private set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "A weight value must be between 0 and 1");
                }

                _value = value;
            }
        }
    }
}