namespace DHI.Services.GIS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Maps;
    using Spatial;
    using SkiaSharp;
    using System.Reflection;

    public static class ExtensionMethods
    {
        public static SKPoint ToDrawingPoint(this Position position, float graphicHeight)
        {
            return new SKPoint(position.X.ToFloat(), (graphicHeight - position.Y - 1).ToFloat());
        }

        public static string[] Dissemble(this string s, char c)
        {
            return s.Split(new[] {c}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static SKColor ToColor(this string s)
        {
            if (SKColor.TryParse(s, out var result))
            {
                return result;
            }

            var htmlColorLower = s.ToLower();

            Type skColorType = typeof(SKColors);

            var propertyInfo = skColorType.GetFields(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(p => p.Name.ToLower() == htmlColorLower);
            if (propertyInfo != null)
            {
                return (SKColor)propertyInfo.GetValue(null);
            }

            throw new ArgumentException("Argument is not a valid color.");
        }

        public static string ToIdStandard(this DateTime dt)
        {
            return dt.ToString("yyyyMMdd_HHmmss");
        }

        public static string ToStandardString(this object o)
        {
            return Convert.ToString(o, CultureInfo.InvariantCulture);
        }

        public static SKBitmap CropAndResize(this SKBitmap originalImage, SKRectI sourceRectangle, SKRectI destinationRectangle)
        {
            var croppedImage = new SKBitmap(destinationRectangle.Width, destinationRectangle.Height);
            using (var graphics = new SKCanvas(croppedImage))
            {
                using var paint = new SKPaint()
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                };

                lock (originalImage)
                {
                    graphics.DrawBitmap(originalImage, sourceRectangle, destinationRectangle);
                }
            }

            return croppedImage;
        }

        public static BoundingBox? GetEnvelope(this IEnumerable<Tile> tiles)
        {
            BoundingBox? envelopeBoundingBox = null;
            var enumerable = tiles as Tile[] ?? tiles.ToArray();
            if (enumerable.Any())
            {
                envelopeBoundingBox = new BoundingBox(enumerable.Min(t => t.BoundingBox.Xmin), enumerable.Min(t => t.BoundingBox.Ymin), enumerable.Max(t => t.BoundingBox.Xmax), enumerable.Max(t => t.BoundingBox.Ymax));
            }

            return envelopeBoundingBox;
        }
    }
}