namespace DHI.Services.GIS.Test
{
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using Maps;
    using Spatial;
    using Xunit;
    using SkiaSharp;

    public class ImagesTest
    {
        [Fact]
        public void StitchIsOk()
        {
            using var image = SKBitmap.Decode(@"..\..\..\Data\TRMM_2000.png");
            var tileImages = new List<TileImage>
            {
                _GetTileImage(image, new Tile(new BoundingBox(0, 0, 10, 10), 2, 6)),
                _GetTileImage(image, new Tile(new BoundingBox(10, 0, 20, 10), 2, 7)),
                _GetTileImage(image, new Tile(new BoundingBox(0, 10, 10, 20), 3, 6)),
                _GetTileImage(image, new Tile(new BoundingBox(10, 10, 20, 20), 3, 7))
            };
            var bitmap = Images.Stitch(tileImages);
            Assert.Equal(bitmap.Width, 2* image.Width);
            Assert.Equal(bitmap.Height, 2 * image.Height);
            bitmap.Dispose();

            tileImages = new List<TileImage>
            {
                _GetTileImage(image, new Tile(new BoundingBox(0, 0, 10, 10), 0, 0)),
                _GetTileImage(image, new Tile(new BoundingBox(0, 10, 10, 20), 1, 0)),
                _GetTileImage(image, new Tile(new BoundingBox(0, 20, 10, 30), 2, 0)),
            };
            bitmap = Images.Stitch(tileImages);
            Assert.Equal(bitmap.Width, image.Width);
            Assert.Equal(bitmap.Height, 3 * image.Height);
            bitmap.Dispose();

            tileImages = new List<TileImage>
            {
                _GetTileImage(image, new Tile(new BoundingBox(0, 0, 10, 10), 5, 7)),
                _GetTileImage(image, new Tile(new BoundingBox(10, 0, 20, 10), 5, 8)),
                _GetTileImage(image, new Tile(new BoundingBox(20, 0, 30, 10), 5, 9)),
            };
            bitmap = Images.Stitch(tileImages);
            Assert.Equal(bitmap.Width, 3 * image.Width);
            Assert.Equal(bitmap.Height, image.Height);
            bitmap.Dispose();

            tileImages = new List<TileImage>
            {
                _GetTileImage(image, new Tile(new BoundingBox(0, 0, 10, 10), 5, 7)),
            };

            bitmap = Images.Stitch(tileImages);
            Assert.Equal(bitmap.Width, image.Width);
            Assert.Equal(bitmap.Height, image.Height);
            bitmap.Dispose();
        }

        [Fact]
        public void ComposeMapIsOk()
        {
            using var image = SKBitmap.Decode(@"..\..\..\Data\TRMM_2000.png");
            using var bitmap = Images.ComposeMap(image, new BoundingBox(0, 0, 12, 12), new BoundingBox(6, 6, 18, 18), new SKSizeI(image.Width, image.Height));
        }

        private static TileImage _GetTileImage(SKBitmap image, Tile tile)
        {
            var bitmap = image.Copy();
            using var graphics = new SKCanvas(bitmap);
            using var paint = new SKPaint()
            {
                Color = SKColors.Black
            };
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), size: 50);

            graphics.DrawText(tile.ToString(), image.Width / 2f, image.Height / 2f, font, paint);

            return new TileImage(bitmap, tile);
        }
    }
}