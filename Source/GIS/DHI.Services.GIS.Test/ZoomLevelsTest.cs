namespace DHI.Services.GIS.Test
{
    using System.Linq;
    using AutoFixture.Xunit2;
    using Maps;
    using Spatial;
    using Xunit;

    public class ZoomLevelsTest
    {
        [Theory, AutoData]
        public void ZoomLevelsCreateIsOk(double xmin, double ymin, double dx, double dy)
        {
            var boundingBox = new BoundingBox(xmin, ymin, xmin + dx, ymin + dy);
            var zoomLevels = new ZoomLevels(boundingBox);

            Assert.Equal(5, zoomLevels.Count);
        }

        [Fact]
        public void GetLevelIsOk()
        {
            var zoomLevels = new ZoomLevels(new BoundingBox(0, 0, 12, 12));

            Assert.Equal(0, zoomLevels.GetLevel(new BoundingBox(-1000, -1000, 1000, 1000)));
            Assert.Equal(0, zoomLevels.GetLevel(new BoundingBox(-1, -1, 13, 13)));
            Assert.Equal(1, zoomLevels.GetLevel(new BoundingBox(1, 4, 5, 11)));
            Assert.Equal(1, zoomLevels.GetLevel(new BoundingBox(8, 7, 13, 11)));
            Assert.Equal(3, zoomLevels.GetLevel(new BoundingBox(10, 4, 11, 5)));
            Assert.Equal(4, zoomLevels.GetLevel(new BoundingBox(1, 1, 1.01, 1.01)));
        }

        [Theory, AutoData]
        public void GetTilesByLevelIsOk(double xmin, double ymin, double dx, double dy)
        {
            var boundingBox = new BoundingBox(xmin, ymin, xmin + dx, ymin + dy);
            var zoomLevels = new ZoomLevels(boundingBox, 4);
            var tiles = zoomLevels.GetTiles(2);

            Assert.Equal(4, zoomLevels.Count);
            Assert.Equal(16, tiles.Length);

            var tile = tiles[6];
            Assert.Equal(1u, tile.Row);
            Assert.Equal(2u, tile.Col);
            Assert.Equal(xmin + dx / 2, tile.BoundingBox.Xmin);
            Assert.Equal(ymin + dy / 4, tile.BoundingBox.Ymin);
            Assert.Equal(tile.BoundingBox.Xmin + dx / 4, tile.BoundingBox.Xmax);
            Assert.Equal(tile.BoundingBox.Ymin + dy / 4, tile.BoundingBox.Ymax);
        }

        [Fact]
        public void GetTilesByBoundingBoxIsOk()
        {
            var zoomLevels = new ZoomLevels(new BoundingBox(0, 0, 12, 12));

            var (level, intersectingTiles) = zoomLevels.GetTiles(new BoundingBox(-1, -1, 13, 13));
            Assert.Equal(0, level);
            Assert.Single(intersectingTiles);
            Assert.Equal("(0, 0)", intersectingTiles[0].ToString());
            Assert.Equal(new BoundingBox(0, 0, 12, 12), intersectingTiles.GetEnvelope());

            (level, intersectingTiles) = zoomLevels.GetTiles(new BoundingBox(1, 4, 5, 11));
            Assert.Equal(1, level);
            Assert.Equal(2, intersectingTiles.Length);
            Assert.Equal("(0, 0)", intersectingTiles[0].ToString());
            Assert.Equal("(1, 0)", intersectingTiles[1].ToString());
            Assert.Equal(new BoundingBox(0, 0, 6, 12), intersectingTiles.GetEnvelope());

            (level, intersectingTiles) = zoomLevels.GetTiles(new BoundingBox(8, 7, 13, 11));
            Assert.Equal(1, level);
            Assert.Single(intersectingTiles);
            Assert.Equal("(1, 1)", intersectingTiles[0].ToString());
            Assert.Equal(new BoundingBox(6, 6, 12, 12), intersectingTiles.GetEnvelope());

            (level, intersectingTiles) = zoomLevels.GetTiles(new BoundingBox(10, 4, 11, 5));
            Assert.Equal(3, level);
            Assert.Equal(4, intersectingTiles.Length);
            Assert.Equal("(2, 6)", intersectingTiles[0].ToString());
            Assert.Equal("(2, 7)", intersectingTiles[1].ToString());
            Assert.Equal("(3, 6)", intersectingTiles[2].ToString());
            Assert.Equal("(3, 7)", intersectingTiles[3].ToString());
            Assert.Equal(new BoundingBox(9, 3, 12, 6), intersectingTiles.GetEnvelope());

            (level, intersectingTiles) = zoomLevels.GetTiles(new BoundingBox(1, 1, 1.01, 1.01));
            Assert.Equal(4, level);
            Assert.Single(intersectingTiles);
            Assert.Equal("(1, 1)", intersectingTiles[0].ToString());
            Assert.Equal(new BoundingBox(0.75, 0.75, 1.5, 1.5), intersectingTiles.GetEnvelope());


            (level, intersectingTiles) = zoomLevels.GetTiles(new BoundingBox(-10, -10, -5, -5));
            Assert.Equal(1, level);
            Assert.False(intersectingTiles.Any());
            Assert.Null(intersectingTiles.GetEnvelope());
        }
    }
}