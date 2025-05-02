namespace DHI.Services.Rasters.Radar
{
    using System;

    /// <summary>
    ///     Miscellaneous static functions.
    /// </summary>
    public static class Funcs
    {
        /// <summary>
        ///     A value representing no data (or out of range)
        /// </summary>
        public static float NoData = float.MinValue;

        /// <summary>
        ///     Calculates the adjusted intensity based on the reflectivity (dBZ) using the Marshall Palmer method.
        /// </summary>
        /// <param name="reflectivity">The reflectivity.</param>
        /// <param name="coefficients">The conversion coefficients.</param>
        /// <returns>The rainfall intensity.</returns>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if the given value is not within the normal range of reflectivity values.
        /// </exception>
        public static float ReflectivityToIntensity(double reflectivity, ConversionCoefficients coefficients)
        {
            // If a NoData flag is encountered: don't go further...
            if ((float)reflectivity == NoData)
            {
                return NoData;
            }

            if (reflectivity < -35 || reflectivity > 65)
            {
                throw new ArgumentException($"{reflectivity} is not a valid reflectivity (dBZ) value.", nameof(reflectivity));
            }

            if (reflectivity <= 0)
            {
                return 0;
            }

            // Marshall Palmer conversion
            var intensity = Math.Pow(10, (reflectivity - 10 * Math.Log10(coefficients.MarshallPalmerA)) / (coefficients.MarshallPalmerB * 10));

            // Unit adjustment
            if (coefficients.RainIntensityUnit.Equals(RainIntensityUnit.MicroMetersPerSecond))
            {
                intensity = intensity / 3.6;
            }

            // Adjustment
            intensity *= coefficients.IntensitySlope;
            intensity += coefficients.IntensityOffset;

            return (float)intensity;
        }

        /// <summary>
        ///     Calculates the adjusted intensity based on the reflectivity (dBZ) using the Marshall Palmer method with default coefficients.
        /// </summary>
        /// <param name="reflectivity">The reflectivity.</param>
        /// <returns>The rainfall intensity.</returns>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if the given value is not within the normal range of reflectivity values.
        /// </exception>
        public static double ReflectivityToIntensity(double reflectivity)
        {
            return ReflectivityToIntensity(reflectivity, ConversionCoefficients.Default);
        }
    }
}