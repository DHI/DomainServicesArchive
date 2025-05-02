namespace DHI.Services.GIS.Maps
{
    using System.Collections.Generic;
    using System.Linq;
    using Spatial;
    using SkiaSharp;

    public static class Images
    {
        public static SKBitmap Stitch(IEnumerable<TileImage> images)
        {
            var tileImages = images as TileImage[] ?? images.ToArray();
            var firstImage = tileImages.First();
            int imageWidth, imageHeight;
            lock (firstImage)
            {
                imageWidth = firstImage.Image.Width;
                imageHeight = firstImage.Image.Height;
            }
            var dx = tileImages.Min(i => i.Col);
            var dy = tileImages.Max(i => i.Row);
            var totalWidth = (tileImages.Max(i => i.Col) - tileImages.Min(i => i.Col) + 1) * imageWidth;
            var totalHeight = (tileImages.Max(i => i.Row) - tileImages.Min(i => i.Row) + 1) * imageHeight;

            var bitmap = new SKBitmap((int)totalWidth, (int)totalHeight);
            using (var g = new SKCanvas(bitmap))
            {
                foreach (var tileImage in tileImages)
                {
                    lock (tileImage)
                    {
                        g.DrawBitmap(tileImage.Image, (tileImage.Col - dx) * imageWidth, -((int)tileImage.Row - dy) * imageHeight);
                    }
                }
            }

            return bitmap;
        }

        public static SKBitmap ComposeMap(SKBitmap stitchedImage, BoundingBox stitchedImageBoundingBox, BoundingBox boundingBox, SKSizeI size)
        {
            var pixelSize = stitchedImageBoundingBox.Width / stitchedImage.Width;
            var x = (boundingBox.Xmin - stitchedImageBoundingBox.Xmin) / pixelSize;
            var y = (stitchedImageBoundingBox.Ymax - boundingBox.Ymax) / pixelSize;
            var width = boundingBox.Width / pixelSize;
            var height = boundingBox.Height / pixelSize;
            var sourceRectangle = SKRectI.Create((int)x, (int)y, (int)width, (int)height);
            var destinationRectangle = SKRectI.Create(0, 0, size.Width, size.Height);
            return stitchedImage.CropAndResize(sourceRectangle, destinationRectangle);
        }
    }
}