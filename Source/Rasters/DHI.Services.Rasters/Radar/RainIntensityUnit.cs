namespace DHI.Services.Rasters.Radar
{
    /// <summary>
    ///     Class RainIntensityUnit.
    /// </summary>
    public class RainIntensityUnit : Enumeration
    {
        /// <summary>
        ///     Millimeters per hour.
        /// </summary>
        public static readonly RainIntensityUnit MilliMetersPerHour = new RainIntensityUnit(0, "mm/h");

        /// <summary>
        ///     Micrometers per second.
        /// </summary>
        public static readonly RainIntensityUnit MicroMetersPerSecond = new RainIntensityUnit(1, "mym/s");

        /// <summary>
        ///     Initializes a new instance of the <see cref="RainIntensityUnit" /> class.
        /// </summary>
        public RainIntensityUnit()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RainIntensityUnit" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayName">The display name.</param>
        public RainIntensityUnit(int value, string displayName)
            : base(value, displayName)
        {
        }
    }
}