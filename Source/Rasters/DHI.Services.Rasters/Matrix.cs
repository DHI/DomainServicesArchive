namespace DHI.Services.Rasters
{
    using Authorization;
    using Radar;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class Matrix.
    /// </summary>
    public class Matrix : BaseEntity<DateTime>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Matrix" /> class.
        /// </summary>
        public Matrix()
        {
            Values = new List<float>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Matrix" /> class.
        /// </summary>
        /// <param name="dateTime">The date/time.</param>
        /// <param name="values">The values.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="permissions">The permissions</param>
        [JsonConstructor]
        public Matrix(DateTime dateTime, IList<float> values = null, IDictionary<string, object> metadata = null, IList<Permission> permissions = null)
            : base(dateTime, metadata, permissions)
        {
            Values = values ?? new List<float>();
        }

        /// <summary>
        ///     Gets the date time.
        /// </summary>
        /// <value>The date time.</value>
        public DateTime DateTime => Id;

        /// <summary>
        ///     Gets a value indicating whether this image has values.
        /// </summary>
        /// <value><c>true</c> if this instance has values; otherwise, <c>false</c>.</value>
        public bool HasValues => Values.Any() && Values.Count == Size.Width*Size.Height;

        /// <summary>
        ///     Gets the maximum pixel value.
        /// </summary>
        /// <value>The maximum pixel value.</value>
        public float MaxValue => Values.Any() ? Values.Max() : float.NaN;

        /// <summary>
        ///     Gets the minimum positive value.
        /// </summary>
        /// <value>The minimum positive value.</value>
        public float MinPositiveValue
        {
            get { return Values.Any(v => v > 0.0) ? Values.Where(v => v > 0.0).Min() : float.NaN; }
        }

        /// <summary>
        ///     Gets the minimum pixel value.
        /// </summary>
        /// <value>The minimum pixel value.</value>
        public virtual float MinValue => Values.Any() ? Values.Min() : float.NaN;

        /// <summary>
        ///     Gets or sets the image size in pixels.
        /// </summary>
        /// <value>The size in pixels.</value>
        public Size Size { get; set; }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public IList<float> Values { get; }

        /// <summary>
        ///     Creates a new matrix instance from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Matrix.</returns>
        public static Matrix CreateNew(string filePath)
        {
            var matrix = new Matrix();
            matrix.FromFile(filePath);
            return matrix;
        }

        /// <summary>
        ///     Creates a new matrix instance from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>Matrix.</returns>
        public static Matrix CreateNew(Stream stream)
        {
            var matrix = new Matrix();
            matrix.FromStream(stream);
            return matrix;
        }

        /// <summary>
        ///     Populates the matrix from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public virtual void FromFile(string filePath)
        {
            FromStream(new MemoryStream(File.ReadAllBytes(filePath)));
        }

        /// <summary>
        ///     Populates the matrix from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public virtual void FromStream(Stream stream)
        {
            var bytes = stream.ToByteArray();

            // Read size
            var width = BitConverter.ToInt32(bytes, 0);
            var height = BitConverter.ToInt32(bytes, 4);
            Size = new Size(width, height);

            // Read DateTime
            Id = DateTime.FromBinary(BitConverter.ToInt64(bytes, 8));

            // Read values
            Values.Clear();
            for (var y = 0; y < Size.Height; y++)
            {
                for (var x = 0; x < Size.Width; x++)
                {
                    var position = 16 + (x + (Size.Width*y))*4;
                    Values.Add(BitConverter.ToSingle(bytes, position));
                }
            }
        }

        /// <summary>
        ///     Gets the value in the give pixel.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>System.Single.</returns>
        public float GetValue(Pixel pixel)
        {
            var x = pixel.Col - 1;
            var y = pixel.Row - 1;
            var index = (y*Size.Width) + x;
            return Values[index];
        }

        /// <summary>
        ///     Returns a bitmap representation of the raster image using a specific color gradient.
        /// </summary>
        /// <param name="colorGradient">The color gradient.</param>
        /// <returns>A Bitmap.</returns>
        public Bitmap ToBitmap(ColorGradient colorGradient)
        {
            var bitmap = new Bitmap(Size.Width, Size.Height);
            for (var y = 0; y < Size.Height; y++)
            {
                for (var x = 0; x < Size.Width; x++)
                {
                    var col = x + 1;
                    var row = y + 1;
                    var value = GetValue(new Pixel(col, row));
                    bitmap.SetPixel(x, y, colorGradient.GetColor(value));
                }
            }

            return bitmap;
        }

        /// <summary>
        ///     Saves the matrix to a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void ToFile(string filePath)
        {
            using var fileStream = File.Create(filePath);
            var stream = ToStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }

        /// <summary>
        ///     Saves the matrix to a stream.
        /// </summary>
        /// <returns>Stream.</returns>
        public virtual Stream ToStream()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            // Write size
            writer.Write(BitConverter.GetBytes(Size.Width));
            writer.Write(BitConverter.GetBytes(Size.Height));

            // Write DateTime (ID)
            writer.Write(DateTime.ToBinary());

            // Write values
            var byteArray = Values.SelectMany(BitConverter.GetBytes).ToArray();
            writer.Write(byteArray);

            return stream;
        }

        /// <summary>
        ///     Updates the value in the given pixel.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <param name="value">The value.</param>
        public void UpdateValue(Pixel pixel, float value)
        {
            if (!HasValues)
            {
                throw new Exception(
                    $"The number of values '{Values.Count}' is out of sync with the matrix size '{Size}'.");
            }

            if (pixel.Col > Size.Width || pixel.Row > Size.Height)
            {
                throw new ArgumentException($"Pixel '{pixel}' is not within the size of the matrix '{Size}'.");
            }

            var x = pixel.Col - 1;
            var y = pixel.Row - 1;
            var index = (y*Size.Width) + x;
            Values[index] = value;
        }

        /// <summary>
        ///     Reads a sequence of bytes from a byte array.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position.</param>
        /// <param name="length">The length.</param>
        /// <returns>Byte array.</returns>
        protected static byte[] ReadBytes(byte[] bytes, int position, int length)
        {
            var subBytes = new byte[length];
            for (var i = 0; i < length; i++)
            {
                subBytes[i] = bytes[position + i];
            }

            return subBytes;
        }
    }
}