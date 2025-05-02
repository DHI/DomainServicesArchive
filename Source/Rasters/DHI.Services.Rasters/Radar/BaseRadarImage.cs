namespace DHI.Services.Rasters.Radar
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Abstract base class for a radar image.
    /// </summary>
    public abstract class BaseRadarImage : BaseRaster, IRadarImage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseRadarImage" /> class.
        /// </summary>
        protected BaseRadarImage()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseRadarImage" /> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="name">The name.</param>
        protected BaseRadarImage(DateTime dateTime, string name) : base(dateTime, name)
        {
        }

        /// <summary>
        ///     Gets or sets the type of the pixel value.
        /// </summary>
        /// <value>The type of the pixel value.</value>
        public PixelValueType PixelValueType { get; set; }

        /// <summary>
        ///     Gets or sets the time of forecast offset.
        /// </summary>
        /// <value>The time of forecast offset.</value>
        public double TimeOfForecastOffset { get; set; }

        /// <summary>
        ///     Gets or sets the radar image type.
        /// </summary>
        /// <value>The type.</value>
        public RadarImageType Type { get; set; }

        /// <summary>
        /// Populates the matrix from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public abstract override void FromStream(Stream stream);

        /// <summary>
        ///     Corrects the pixel values according to the correction factors in the given correction matrix.
        /// </summary>
        /// <param name="correctionMatrix">The correction matrix.</param>
        public void Correct(Matrix correctionMatrix)
        {
            if (!correctionMatrix.HasValues)
            {
                throw new ArgumentException("The correction matrix is not properly populated with pixel values.", nameof(correctionMatrix));
            }

            if (correctionMatrix.Size != Size)
            {
                throw new ArgumentException(
                    $"The correction matrix size '{correctionMatrix.Size}' is not equal to raster size '{Size}'.", nameof(correctionMatrix));
            }

            if (!HasValues)
            {
                throw new Exception("The raster is not properly populated with pixel values.");
            }

            for (var i = 0; i < Values.Count; i++)
            {
                Values[i] = Values[i] * correctionMatrix.Values[i];
            }
        }

        /// <summary>
        ///     Gets the rainfall intensity in the specified pixel using default conversion coefficients.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>The intensity.</returns>
        /// <exception cref="System.ArgumentException">Thrown if not the correct pixel definition type.</exception>
        public virtual float GetIntensity(Pixel pixel)
        {
            return GetIntensity(pixel, ConversionCoefficients.Default);
        }

        /// <summary>
        ///     Gets the rainfall intensity in the specified pixel using the specified conversion coefficients.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <param name="coefficients">The conversion coefficients.</param>
        /// <returns>The intensity.</returns>
        /// <exception cref="System.ArgumentException">Thrown if not the correct pixel definition type.</exception>
        public virtual float GetIntensity(Pixel pixel, ConversionCoefficients coefficients)
        {
            return PixelValueType.Value switch
            {
                0 => Funcs.ReflectivityToIntensity(GetValue(pixel), coefficients),
                1 => GetValue(pixel),
                _ => throw new NotSupportedException($"{PixelValueType} is not supported")
            };
        }

        /// <summary>
        ///     Returns a bitmap representation of the radar image using a default color gradient.
        /// </summary>
        /// <returns>A Bitmap.</returns>
        public override Bitmap ToBitmap()
        {
            return PixelValueType.Value switch
            {
                0 => ToBitmap(ColorGradientType.ReflectivityDefault.ColorGradient),
                1 => ToBitmap(ColorGradientType.IntensityDefault.ColorGradient),
                _ => throw new NotSupportedException($"{PixelValueType} is not supported")
            };
        }

        /// <summary>
        ///     Gets a radar image with pixel values converted to intensities using default conversion coefficients (no
        ///     adjustment).
        /// </summary>
        /// <returns>A radar image.</returns>
        public IRadarImage ToIntensity()
        {
            return ToIntensity(ConversionCoefficients.Default);
        }

        /// <summary>
        ///     Gets a radar image with pixel values converted to intensities using the specified conversion coefficients.
        /// </summary>
        /// <param name="coefficients">The conversion coefficients.</param>
        /// <returns>A radar image.</returns>
        /// <exception cref="System.Exception">Thrown if radar image values are already of type intensity.</exception>
        public IRadarImage ToIntensity(ConversionCoefficients coefficients)
        {
            if (!Equals(PixelValueType, PixelValueType.Reflectivity))
            {
                throw new Exception(
                    $"Radar image values are already of type Intensity. Always check on {typeof(PixelValueType)} before calling ToIntensity().");
            }

            // Make a shallow clone, i.e a clone which copies only properties, not values.
            var image = Clone(false);
            // Loop through the values of the original image, convert to intensity and add to the cloned image.
            for (var i = 0; i < Values.Count; i++)
            {
                image.Values.Add(Funcs.ReflectivityToIntensity(Values[i], coefficients));
            }

            image.PixelValueType = PixelValueType.Intensity;
            image.PixelValueUnit = coefficients.RainIntensityUnit.ToString();

            return image;
        }

        /// <summary>
        ///     Gets the minimum pixel value. NoData values are ignored
        /// </summary>
        /// <value>The minimum pixel value.</value>
        public override float MinValue => Values.Any() ? Values.Where(v => v != Funcs.NoData).Min() : float.NaN;

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <param name="deepClone">
        ///     true (default) creates a deep clone, which includes copying the values of the radar image.
        ///     false creates a shallow clone, which only clones the properties of the radar image.
        /// </param>
        /// <returns>A clone of BaseRadarImage.</returns>
        protected BaseRadarImage Clone(bool deepClone = true)
        {
            var image = (BaseRadarImage)Activator.CreateInstance(GetType());
            image.Id = DateTime;
            if (Name != null)
            {
                image.Name = Name;
            }

            image.PixelValueType = PixelValueType;
            image.TimeOfForecastOffset = TimeOfForecastOffset;
            image.Type = Type;

            if (PixelValueUnit != null)
            {
                image.PixelValueUnit = PixelValueUnit;
            }

            image.PixelSize = PixelSize;
            image.Size = Size;
            image.GeoCenter = GeoCenter;
            image.GeoProjectionString = GeoProjectionString;
            image.GeoLowerLeft = GeoLowerLeft;
            image.GeoLowerRight = GeoLowerRight;
            image.GeoUpperLeft = GeoUpperLeft;
            image.GeoUpperRight = GeoUpperRight;

            // only copy image values if deepClone == true 
            if (!deepClone)
            {
                return image;
            }

            foreach (var value in Values)
            {
                image.Values.Add(value);
            }

            return image;
        }
    }
}