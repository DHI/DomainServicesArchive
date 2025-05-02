namespace DHI.Services.GIS.Maps
{
    using System;
    using SkiaSharp;

    [Serializable]
    public class TileImage : Tile
    {
        public TileImage(SKBitmap image, Tile tile) : base(tile.BoundingBox, tile.Row, tile.Col)
        {
            Image = image;
        }

        public SKBitmap Image { get; }
    }
}