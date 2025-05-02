namespace DHI.Services.Rasters.Radar.X00
{
    using System;
    using System.Drawing;
    using System.IO;
    using Radar;

    /// <summary>
    ///     Radar image supporting DHI LAWR X00 format.
    /// </summary>
    public class RadarImage : BaseRadarImage
    {
        /// <summary>
        ///     Creates a radar image from a X00 file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A radar image.</returns>
        public new static RadarImage CreateNew(string filePath)
        {
            var radarImage = new RadarImage();
            radarImage.FromFile(filePath);
            return radarImage;
        }

        /// <summary>
        ///     Creates a radar image from a X00 stream.
        /// </summary>
        /// <param name="stream">The X00 stream.</param>
        /// <returns>A radar image.</returns>
        public new static RadarImage CreateNew(Stream stream)
        {
            var radarImage = new RadarImage();
            radarImage.FromStream(stream);
            return radarImage;
        }

        /// <summary>
        ///     Populates the image from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public override void FromStream(Stream stream)
        {
            var bytes = stream.ToByteArray();
            var header = new RadarImageHeader(bytes);

            // Set meta data
            Name = header.RadarName;
            Id = header.DateTime;
            switch (header.ImageType)
            {
                case "O":
                    Type = RadarImageType.Observation;
                    break;

                case "P":
                    Type = RadarImageType.Forecast;
                    break;

                default:
                    throw new NotSupportedException($"{header.ImageType} is not supported");
            }

            switch (header.Signal)
            {
                case "X":
                    PixelValueType = PixelValueType.Reflectivity;
                    break;

                case "D":
                    PixelValueType = PixelValueType.Intensity;
                    break;

                default:
                    throw new NotSupportedException($"{header.Signal} is not supported");
            }

            PixelValueUnit = header.StoreQuantity;
            TimeOfForecastOffset = header.MmPredict;
            PixelSize = new Size(header.PixelSize, header.PixelSize);
            Size = new Size(header.EastUppb, header.NorthUppb);

            // Set values
            var valueLength = Equals(PixelValueType, PixelValueType.Reflectivity) ? 1 : 8;
            for (var y = 0; y < Size.Height; y++)
            {
                for (var x = 0; x < Size.Width; x++)
                {
                    var position = RadarImageHeader.ByteLength + (x + (Size.Width * y)) * valueLength;
                    var valueBytes = ReadBytes(bytes, position, valueLength);
                    var value = PixelValueType.ToValue(valueBytes);
                    if (Equals(PixelValueType, PixelValueType.Reflectivity))
                    {
                        value = (value - (float)header.StoreOffset) * (float)header.StoreSlope + (float)header.StoreOrd;
                    }

                    Values.Add(value);
                }
            }
        }

        /// <summary>
        ///     Gets a radar image with pixel values converted to intensities using default conversion coefficients (no
        ///     adjustment).
        /// </summary>
        /// <returns>A radar image.</returns>
        /// <exception cref="System.Exception">Thrown if radar image values are already of type intensity.</exception>
        public new RadarImage ToIntensity()
        {
            return (RadarImage)base.ToIntensity();
        }

        /// <summary>
        ///     Gets a radar image with pixel values converted to intensities using the specified conversion coefficients.
        /// </summary>
        /// <param name="coefficients">The conversion coefficients.</param>
        /// <returns>A radar image.</returns>
        /// <exception cref="System.Exception">Thrown if radar image values are already of type intensity.</exception>
        public new RadarImage ToIntensity(ConversionCoefficients coefficients)
        {
            return (RadarImage)base.ToIntensity(coefficients);
        }
    }
}