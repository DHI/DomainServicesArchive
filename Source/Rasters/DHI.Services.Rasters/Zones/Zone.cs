namespace DHI.Services.Rasters.Zones
{
    using DHI.Services.Authorization;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class Zone.
    /// </summary>
    public class Zone : BaseNamedEntity<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Zone" /> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="zoneType">Type of the zone.</param>
        [JsonConstructor]
        public Zone(string id, string name, ZoneType zoneType = null)
            : base(id, name)
        {
            PixelWeights = new HashSet<PixelWeight>();
            Type = zoneType ?? ZoneType.Polygon;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Zone" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="metadata"></param>
        /// <param name="permissions"></param>
        /// <param name="zoneType"></param>
        public Zone(string id, string name, IDictionary<string, object> metadata, IList<Permission> permissions, ZoneType zoneType = null)
            : base(id, name, metadata, permissions)
        {
            PixelWeights = new HashSet<PixelWeight>();
            Type = zoneType ?? ZoneType.Polygon;
        }

        /// <summary>
        ///     Gets the pixel weights.
        /// </summary>
        /// <value>The pixel weights.</value>
        public HashSet<PixelWeight> PixelWeights { get; internal set; }

        /// <summary>
        ///     Gets or sets the size of the image.
        /// </summary>
        /// <value>The size of the image.</value>
        public Size ImageSize { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the sum of pixel weights are 1.
        /// </summary>
        /// <value><c>true</c> if the sum of pixel weights are 1; otherwise, <c>false</c>.</value>
        public bool PixelWeightsAreValid => Math.Round(PixelWeightTotal, 3).Equals(1.0);

        /// <summary>
        ///     Gets the sum of pixel weights.
        /// </summary>
        /// <value>The sum of pixel weights.</value>
        public double PixelWeightTotal
        {
            get { return PixelWeights.Any() ? PixelWeights.Sum(pw => pw.Weight.Value) : 0.0; }
        }

        /// <summary>
        ///     Gets or sets the zone type.
        /// </summary>
        /// <value>The zone type.</value>
        public ZoneType Type { get; internal set; }

        /// <summary>
        ///     Returns a bitmap representation of the zone.
        /// </summary>
        /// <returns>Bitmap.</returns>
        public Bitmap ToBitmap(Color zoneColor, Color backgroundColor)
        {
            var bitmap = new Bitmap(ImageSize.Width, ImageSize.Height);
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, backgroundColor);
                }
            }

            foreach (var pixel in PixelWeights.Select(pixelWeight => pixelWeight.Pixel))
            {
                bitmap.SetPixel(pixel.Col - 1, pixel.Row - 1, zoneColor);
            }

            return bitmap;
        }

        public Bitmap ToBitmap()
        {
            return ToBitmap(Color.Blue, Color.LightSteelBlue);
        }

        public bool ShouldSerializePixelWeightsAreValid()
        {
            return false;
        }

        public bool ShouldSerializePixelWeightTotal()
        {
            return false;
        }

        public bool ShouldSerializePixelWeights()
        {
            return PixelWeights.Any();
        }
    }
}