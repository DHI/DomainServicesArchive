namespace DHI.Services.Rasters.Radar
{
    using Zones;

    /// <summary>
    ///     Class RadarImageExtensions.
    /// </summary>
    public static class RadarImageExtensions
    {
        /// <summary>
        ///     Gets the rainfall intensity for the given zone using the default conversion coefficients.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="zone">The zone.</param>
        /// <returns>The intensity.</returns>
        public static double GetIntensity(this IRadarImage image, Zone zone)
        {
            return GetIntensity(image, zone, ConversionCoefficients.Default);
        }

        /// <summary>
        ///     Gets the rainfall intensity for the given zone using the specified conversion coefficients.
        /// </summary>
        /// <param name="image">the image.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="conversionCoefficients">The reflectivity-to-intensity conversion coefficients.</param>
        /// <returns>The intensity.</returns>
        public static double GetIntensity(this IRadarImage image, Zone zone, ConversionCoefficients conversionCoefficients)
        {
            var intensity = 0d;
            foreach (var pixelWeight in zone.PixelWeights)
            {
                var pixelIntensity = image.GetIntensity(pixelWeight.Pixel, conversionCoefficients);
                if (pixelIntensity != Funcs.NoData)
                {
                    intensity += pixelWeight.Weight.Value * pixelIntensity;
                }
            }

            return intensity;
        }
    }
}