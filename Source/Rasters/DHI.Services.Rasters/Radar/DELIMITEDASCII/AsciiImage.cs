namespace DHI.Services.Rasters.Radar.DELIMITEDASCII
{
    using System;
    using System.Drawing;
    using System.IO;

    /// <summary>
    ///     Delimited ascii image implementation of the BaseRadarImage abstract class
    /// </summary>
    public class AsciiImage : BaseRadarImage
    {
        /// <summary>
        ///     Creates an ascii image from a file
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns></returns>
        public new static AsciiImage CreateNew(string filePath)
        {
            var asciiImage = new AsciiImage();
            asciiImage.FromFile(filePath);
            asciiImage.Name = Path.GetFileName(filePath);
            return asciiImage;
        }

        /// <summary>
        ///     Creates and ascii image from a stream
        /// </summary>
        /// <param name="stream">The file stream</param>
        /// <returns>An ascii image</returns>
        public new static AsciiImage CreateNew(Stream stream)
        {
            var asciiImage = new AsciiImage();
            asciiImage.FromStream(stream);
            return asciiImage;
        }

        public override void FromStream(Stream stream)
        {
            var asciiImageReader = new AsciiImageReader(stream);
            Size = new Size(asciiImageReader.ColumnCount, asciiImageReader.RowCount);
            PixelValueType = PixelValueType.Intensity;
            Id = DateTime.MinValue;
            var fileValues = asciiImageReader.Values;

            // Set values
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    var position = x + Size.Width * y;
                    Values.Add(fileValues[position]);
                }
            }
        }
    }
}
