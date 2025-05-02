namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Spatial;

    public class ZoomLevels
    {
        private readonly Dictionary<int, List<Tile>> _zoomLevels;
        private readonly BoundingBox _boundingBox;

        public ZoomLevels(BoundingBox boundingBox, int numberOfLevels = 5)
        {
            _zoomLevels = new Dictionary<int, List<Tile>>();
            _boundingBox = boundingBox;
            for (var level = 0; level < numberOfLevels; level++)
            {
                var numberOfTilesX = Math.Pow(2, level);
                var numberOfTilesY = numberOfTilesX;
                var tileWidth = _boundingBox.Width / numberOfTilesX;
                var tileHeight = _boundingBox.Height / numberOfTilesY;
                var tiles = new List<Tile>();
                var ymin = _boundingBox.Ymin;
                for (uint row = 0; row < numberOfTilesY; row++)
                {
                    var ymax = ymin + tileHeight;
                    var xmin = _boundingBox.Xmin;
                    for (uint col = 0; col < numberOfTilesX; col++)
                    {
                        var xmax = xmin + tileWidth;
                        var tileBoundingBox = new BoundingBox(xmin, ymin, xmax, ymax);
                        tiles.Add(new Tile(tileBoundingBox, row, col));
                        xmin += tileWidth;
                    }

                    ymin += tileHeight;
                }

                _zoomLevels.Add(level, tiles);
            }
        }

        public int Count => _zoomLevels.Count;

        public int GetLevel(BoundingBox boundingBox)
        {
            var n = Math.Log(_boundingBox.Width / boundingBox.Width) / Math.Log(2);
            return n < 0 ? 0 : Math.Min((int)Math.Floor(n), Count - 1);
        }

        public Tile[] GetTiles(int level)
        {
            return _zoomLevels[level].ToArray();
        }

        public (int level, Tile[] tiles) GetTiles(BoundingBox boundingBox)
        {
            var level = GetLevel(boundingBox);
            var tiles = GetTiles(level).Where(tile => tile.BoundingBox.Intersects(boundingBox)).ToArray();
            return (level, tiles);
        }
    }
}