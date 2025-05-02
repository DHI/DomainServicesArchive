namespace DHI.Services.Rasters.Radar
{
    public class ConversionCoefficients
    {
        /// <summary>
        /// Gets the default conversion coefficients (IntensityOffset = 0, IntensitySlope = 1, MarshallPalmerA = 200, MarshallPalmerB = 1.6, RainIntensityUnit = RainIntensityUnit.MilliMetersPerHour).
        /// </summary>
        /// <value>The default conversion coefficients.</value>
        public static ConversionCoefficients Default => new ConversionCoefficients()
        {
            IntensityOffset = 0,
            IntensitySlope = 1,
            MarshallPalmerA = 200,
            MarshallPalmerB = 1.6,
            RainIntensityUnit = RainIntensityUnit.MilliMetersPerHour
        };

        /// <summary>
        /// Gets or sets the intensity offset.
        /// </summary>
        /// <value>The intensity offset.</value>
        public double IntensityOffset { get; set; }

        /// <summary>
        /// Gets or sets the intensity slope.
        /// </summary>
        /// <value>The intensity slope.</value>
        public double IntensitySlope { get; set; }

        /// <summary>
        /// Gets or sets the marshall palmer A coefficient.
        /// </summary>
        /// <value>The marshall palmer A coefficient.</value>
        public double MarshallPalmerA { get; set; }

        /// <summary>
        /// Gets or sets the marshall palmer B coefficient.
        /// </summary>
        /// <value>The marshall palmer B coefficient.</value>
        public double MarshallPalmerB { get; set; }

        /// <summary>
        /// Gets or sets the rain intensity unit.
        /// </summary>
        /// <value>The rain intensity unit.</value>
        public RainIntensityUnit RainIntensityUnit { get; set; }
    }
}
