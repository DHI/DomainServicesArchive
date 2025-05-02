namespace DHI.Services.Rasters.Radar
{
    public class RadarImageType : Enumeration
    {
        /// <summary>
        /// An observation.
        /// </summary>
        public static readonly RadarImageType Observation = new(0, "Observation");

        /// <summary>
        /// A forecast.
        /// </summary>
        public static readonly RadarImageType Forecast = new(1, "Forecast");

        /// <summary>
        /// Initializes a new instance of the <see cref="RadarImageType"/> class.
        /// </summary>
        public RadarImageType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadarImageType" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayName">The display name.</param>
        protected RadarImageType(int value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
