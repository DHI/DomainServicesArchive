namespace DHI.Services.Rasters
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public readonly struct ColorGradient
    {
        private readonly double _maxThresholdValue;
        private readonly double _minThresholdValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorGradient" /> class.
        /// </summary>
        /// <param name="pointColors">A sorted dictionary of points (threshold values) and their corresponding colors.</param>
        /// <param name="isLogarithmic">If set to <c>true</c> the threshold values represent a logarithmic scale.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if pointColors is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if not at least 2 points are defined.</exception>
        public ColorGradient(SortedDictionary<double, Color> pointColors, bool isLogarithmic)
        {
            if (pointColors == null)
            {
                throw new ArgumentNullException(nameof(pointColors));
            }

            if (pointColors.Count < 2)
            {
                throw new ArgumentException("At least 2 points must be given to define a color gradient", nameof(pointColors));
            }

            PointColors = pointColors;
            _minThresholdValue = PointColors.First().Key;
            _maxThresholdValue = PointColors.Last().Key;
            IsLogarithmic = isLogarithmic;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorGradient" /> class.
        /// </summary>
        /// <param name="pointColors">The point colors.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if pointColors is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if not at least 2 points are defined.</exception>
        public ColorGradient(SortedDictionary<double, Color> pointColors)
            : this(pointColors, false)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether the threshold values represent a logarithmic scale.
        /// </summary>
        /// <value><c>true</c> if the threshold values represent a logarithmic scale; otherwise, <c>false</c>.</value>
        public bool IsLogarithmic { get; }

        /// <summary>
        ///     Gets the point colors.
        /// </summary>
        /// <value>The point colors.</value>
        public SortedDictionary<double, Color> PointColors { get; }

        /// <summary>
        ///     Gets the threshold values.
        /// </summary>
        /// <value>The threshold values.</value>
        public double[] ThresholdValues => PointColors.Keys.ToArray();

        /// <summary>
        ///     Gets the color corresponding to the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A color.</returns>
        public Color GetColor(double value)
        {
            if (value < _minThresholdValue)
            {
                return Color.Transparent;
            }

            if (value > _maxThresholdValue)
            {
                return PointColors.Last().Value;
            }

            var startPoint = new KeyValuePair<double, Color>();
            var endPoint = new KeyValuePair<double, Color>();
            for (var i = 0; i < PointColors.Count - 1; i++)
            {
                if (value >= PointColors.ElementAt(i).Key && value <= PointColors.ElementAt(i + 1).Key)
                {
                    startPoint = PointColors.ElementAt(i);
                    endPoint = PointColors.ElementAt(i + 1);
                    break;
                }
            }

            var percentage = (value - startPoint.Key) / (endPoint.Key - startPoint.Key);

            var a = startPoint.Value.A * (1 - percentage) + endPoint.Value.A * percentage;
            var r = startPoint.Value.R * (1 - percentage) + endPoint.Value.R * percentage;
            var g = startPoint.Value.G * (1 - percentage) + endPoint.Value.G * percentage;
            var b = startPoint.Value.B * (1 - percentage) + endPoint.Value.B * percentage;
            var color = Color.FromArgb(Convert.ToInt32(a), Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b));

            return color;
        }

        /// <summary>
        ///     Returns a bitmap representation of the color gradient.
        /// </summary>
        /// <param name="height">The bitmap height.</param>
        /// <param name="width">The bitmap width.</param>
        /// <returns>A color gradient bitmap.</returns>
        public Bitmap ToBitmap(int height, int width)
        {
            var bitmap = new Bitmap(width, height);
            for (var row = 0; row < height; row++)
            {
                double pixelValue;
                if (IsLogarithmic)
                {
                    pixelValue = _maxThresholdValue - (_maxThresholdValue - _minThresholdValue) * Math.Log10(row) / Math.Log10(height);
                }
                else
                {
                    pixelValue = _maxThresholdValue - (_maxThresholdValue - _minThresholdValue) * row / height;
                }

                var color = GetColor(pixelValue);
                for (var col = 0; col < width; col++)
                {
                    bitmap.SetPixel(col, row, color);
                }
            }

            return bitmap;
        }

        /// <summary>
        ///     Saves a bitmap representation of the color gradient.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        public void ToBitmap(string filePath, int height, int width)
        {
            ToBitmap(height, width).Save(filePath);
        }
    }
}