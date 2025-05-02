namespace DHI.Services.Rasters
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    ///     Interface IRaster
    /// </summary>
    public interface IRaster : INamedEntity<DateTime>
    {
        /// <summary>
        ///     Gets the date time.
        /// </summary>
        /// <value>The date time.</value>
        DateTime DateTime { get; }

        /// <summary>
        ///     Gets or sets the geographic projection string.
        /// </summary>
        /// <value>The geographic projection string.</value>
        string GeoProjectionString { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to the center.
        /// </summary>
        /// <value>The geographic coordinates to the center.</value>
        PointF GeoCenter { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to lower left corner.
        /// </summary>
        /// <value>The geographic coordinates to lower left corner.</value>
        PointF GeoLowerLeft { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to upper left corner.
        /// </summary>
        /// <value>The geographic coordinates to upper left corner.</value>
        PointF GeoUpperLeft { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to upper right corner.
        /// </summary>
        /// <value>The geographic coordinates to upper right corner.</value>
        PointF GeoUpperRight { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to lower right corner.
        /// </summary>
        /// <value>The geographic coordinates to lower right corner.</value>
        PointF GeoLowerRight { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this raster has any pixel values set.
        /// </summary>
        /// <value><c>true</c> if this instance has values; otherwise, <c>false</c>.</value>
        bool HasValues { get; }

        /// <summary>
        ///     Gets the maximum pixel value.
        /// </summary>
        /// <value>The maximum pixel value.</value>
        float MaxValue { get; }

        /// <summary>
        ///     Gets the minimum positive value.
        /// </summary>
        /// <value>The minimum positive value.</value>
        float MinPositiveValue { get; }

        /// <summary>
        ///     Gets the minimum pixel value.
        /// </summary>
        /// <value>The minimum pixel value.</value>
        float MinValue { get; }

        /// <summary>
        ///     Gets the size of the pixel.
        /// </summary>
        /// <value>The size of the pixel.</value>
        SizeF PixelSize { get; }

        /// <summary>
        ///     Gets or sets the pixel value unit.
        /// </summary>
        /// <value>The pixel value unit.</value>
        string PixelValueUnit { get; set; }

        /// <summary>
        ///     Gets the size of the raster.
        /// </summary>
        /// <value>The size.</value>
        Size Size { get; }

        /// <summary>
        ///     Gets the geographic coordinates to the corners of the specified pixel.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>A list of geographic coordinates to the corners of the specified pixel.</returns>
        IList<PointF> GetGeoCoordinates(Pixel pixel);

        /// <summary>
        ///     Gets the value of a specified pixel.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>The pixel value.</returns>
        float GetValue(Pixel pixel);

        /// <summary>
        ///     Returns a bitmap representation of the raster.
        /// </summary>
        /// <returns>A Bitmap.</returns>
        Bitmap ToBitmap();

        /// <summary>
        ///     Returns a bitmap representation of the raster using a specific color gradient.
        /// </summary>
        /// <param name="colorGradient">The color gradient.</param>
        /// <returns>A Bitmap.</returns>
        Bitmap ToBitmap(ColorGradient colorGradient);
    }
}