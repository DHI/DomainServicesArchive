namespace DHI.Services.Rasters.Radar
{
    using System.IO;

    public interface IRadarImage : IRaster
    {
        /// <summary>
        ///     Gets or sets the type of the pixel value.
        /// </summary>
        /// <value>The type of the pixel value.</value>
        PixelValueType PixelValueType { get; set; }

        /// <summary>
        ///     Gets or sets the time of forecast offset.
        /// </summary>
        /// <value>The time of forecast offset.</value>
        double TimeOfForecastOffset { get; set; }

        /// <summary>
        ///     Gets or sets the radar image type.
        /// </summary>
        /// <value>The type.</value>
        RadarImageType Type { get; set; }

        /// <summary>
        ///     Gets the intensity in the specified pixel using default conversion coefficients.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>The intensity.</returns>
        float GetIntensity(Pixel pixel);

        /// <summary>
        ///     Gets the intensity in the specified pixel using the specified conversion coefficients.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <param name="coefficients">The conversion coefficients.</param>
        /// <returns>The intensity.</returns>
        float GetIntensity(Pixel pixel, ConversionCoefficients coefficients);

        /// <summary>
        ///     Gets a radar image with pixel values converted to intensities using default conversion coefficients (no
        ///     adjustment).
        /// </summary>
        /// <returns>A radar image.</returns>
        IRadarImage ToIntensity();

        /// <summary>
        ///     Gets a radar image with pixel values converted to intensities using the specified conversion coefficients.
        /// </summary>
        /// <param name="coefficients">The conversion coefficients.</param>
        /// <returns>A radar image.</returns>
        IRadarImage ToIntensity(ConversionCoefficients coefficients);

        /// <summary>
        ///     Corrects the pixel values according to the correction factors in the given correction matrix.
        /// </summary>
        /// <param name="correctionMatrix">The correction matrix.</param>
        void Correct(Matrix correctionMatrix);

        /// <summary>
        ///     Populates the radar image from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        void FromStream(Stream stream);

        /// <summary>
        ///     Populates the radar image from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void FromFile(string filePath);
    }
}