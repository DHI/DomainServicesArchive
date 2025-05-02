namespace DHI.Services.Rasters
{
    using DHI.Services.Authorization;
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public abstract class BaseRaster : Matrix, IRaster
    {
        private string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseRaster" /> class.
        /// </summary>
        protected BaseRaster()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseRaster" /> class.
        /// </summary>
        /// <param name="dateTime">The dateTime.</param>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="permissions">The permissions.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected BaseRaster(DateTime dateTime, string name, IList<float> values = null, IDictionary<string, object> metadata = null, IList<Permission> permissions = null)
            : base(dateTime, values, metadata, permissions)
        {
            Guard.Against.NullOrEmpty(name, nameof(name));
            _name = name;
        }

        /// <summary>
        ///     Gets or sets the geographic projection string.
        /// </summary>
        /// <value>The geographic projection string.</value>
        public string GeoProjectionString { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to the center.
        /// </summary>
        /// <value>The geographic coordinates to the center.</value>
        public PointF GeoCenter { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to lower left corner.
        /// </summary>
        /// <value>The geographic coordinates to lower left corner.</value>
        public PointF GeoLowerLeft { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to upper left corner.
        /// </summary>
        /// <value>The geographic coordinates to upper left corner.</value>
        public PointF GeoUpperLeft { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to upper right corner.
        /// </summary>
        /// <value>The geographic coordinates to upper right corner.</value>
        public PointF GeoUpperRight { get; set; }

        /// <summary>
        ///     Gets or sets the geographic coordinates to lower right corner.
        /// </summary>
        /// <value>The geographic coordinates to lower right corner.</value>
        public PointF GeoLowerRight { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name
        {
            get => _name;

            protected set => _name = value;
        }

        /// <summary>
        ///     Gets or sets the size of a pixel.
        /// </summary>
        /// <value>The size of a pixel.</value>
        public SizeF PixelSize { get; set; }

        /// <summary>
        ///     Gets or sets the pixel value unit.
        /// </summary>
        /// <value>The pixel value unit.</value>
        public string PixelValueUnit { get; set; }

        /// <summary>
        ///     Gets the geographical coordinates to the corners of the specified pixel.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>
        ///     A list of geographical coordinates to the corners of the specified pixel. First point in the list is lower
        ///     left corner. Subsequent points are given in a clockwise direction.
        /// </returns>
        /// <exception cref="System.Exception">Thrown if the geographical center of raster is not defined.</exception>
        public IList<PointF> GetGeoCoordinates(Pixel pixel)
        {
            if (GeoCenter.IsEmpty)
            {
                throw new Exception("Geographical center of raster is not defined");
            }

            // (x0, y0) is pixel lower left. (x1, y1) is pixel upper right.
            var coordinates = new List<PointF>();
            var x0 = GeoCenter.X + (pixel.Col - 1 - Size.Width / 2) * PixelSize.Width;
            var x1 = x0 + PixelSize.Width;
            var y1 = GeoCenter.Y + (Size.Height / 2 - (pixel.Row - 1)) * PixelSize.Height;
            var y0 = y1 - PixelSize.Height;
            coordinates.Add(new PointF(x0, y0));
            coordinates.Add(new PointF(x0, y1));
            coordinates.Add(new PointF(x1, y1));
            coordinates.Add(new PointF(x1, y0));
            coordinates.Add(new PointF(x0, y0));

            return coordinates;
        }

        /// <summary>
        ///     Returns a bitmap representation of the raster using a default color gradient.
        /// </summary>
        /// <returns>A Bitmap.</returns>
        public abstract Bitmap ToBitmap();
    }
}
