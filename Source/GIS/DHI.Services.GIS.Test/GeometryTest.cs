namespace DHI.Services.GIS.Test
{
    using System;
    using Spatial;
    using Xunit;

    public class GeometryTest
    {
        [Theory]
        [InlineData("((40 40, 20 45)), ((20 35, 10 30), (30 20, 20 15))", 2)]
        [InlineData("(20 35, 10 30), (30 20, 20 15)", 2)]
        [InlineData("20 35, 10 30", 2)]
        public void SplitArrayIsOk(string arrayString, int length)
        {
            var array = Geometry.SplitArray(arrayString);
            Assert.Equal(length, array.Length);
        }

        [Theory]
        [InlineData("40 40, 20 45")]
        public void TrimForOuterParenthesisThrows(string s)
        {
            Assert.Throws<ArgumentException>(() => Geometry.TrimForOuterParenthesis(s));
        }

        [Theory]
        [InlineData("LineString (30.3 10.2, 10.3 14.2, 40 40.6)")]
        [InlineData("point (30.3 10.2)")]
        public void GeometryFromWktThrows(string wkt)
        {
            Assert.Throws<NotSupportedException>(() => Geometry.FromWKT(wkt));
        }

        [Fact]
        public void PositionFromWktIsOk()
        {
            var position2D = Position.FromWKT("103.24 207.8");
            Assert.Equal(103.24, position2D.X);
            Assert.Equal(207.8, position2D.Y);
            Assert.Null(position2D.Z);

            var position3D = Position.FromWKT("103.24 207.8 99.87");
            Assert.Equal(103.24, position3D.X);
            Assert.Equal(207.8, position3D.Y);
            Assert.Equal(99.87, position3D.Z);
        }

        [Theory]
        [InlineData("103.24")]
        [InlineData("103.24  207.8")]
        [InlineData("103.24, 207.8")]
        public void PositionFromWktThrows(string wkt)
        {
            Assert.Throws<ArgumentException>(() => Position.FromWKT(wkt));
        }

        [Fact]
        public void PointFromWktIsOk()
        {
            var geometry = Geometry.FromWKT("POINT (30.3 10.2)");
            Assert.IsType<Point>(geometry);
            Assert.Equal(30.3, geometry.Coordinates.X);
            Assert.Equal(10.2, geometry.Coordinates.Y);
        }

        [Fact]
        public void LineStringFromWktIsOk()
        {
            var geometry = Geometry.FromWKT("LINESTRING (30.3 10.2, 10.3 14.2, 40 40.6)");
            Assert.IsType<LineString>(geometry);
            Assert.Equal(3, geometry.Coordinates.Count);
            Assert.Equal(30.3, geometry.Coordinates[0].X);
        }

        [Fact]
        public void PolygonFromWktIsOk()
        {
            var geometry = Geometry.FromWKT("POLYGON ((130.3 110.2, 110.3 114.2, 140 140.6), (30.3 10.2, 10.3 14.2, 40 40.6))");
            Assert.IsType<Polygon>(geometry);
            Assert.Equal(2, geometry.Coordinates.Count);
            Assert.Equal(3, geometry.Coordinates[0].Count);
            Assert.Equal(130.3, geometry.Coordinates[0][0].X);
        }

        [Fact]
        public void MultiPointFromWktIsOk()
        {
            var geometry = Geometry.FromWKT("MULTIPOINT (30.3 10.2, 10.3 14.2, 40 40.6)");
            Assert.IsType<MultiPoint>(geometry);
            Assert.Equal(3, geometry.Coordinates.Count);
            Assert.Equal(30.3, geometry.Coordinates[0].X);
        }

        [Fact]
        public void MultiLineStringFromWktIsOk()
        {
            var geometry = Geometry.FromWKT("MULTILINESTRING ((130.3 110.2, 110.3 114.2, 140 140.6), (30.3 10.2, 10.3 14.2, 40 40.6))");
            Assert.IsType<MultiLineString>(geometry);
            Assert.Equal(2, geometry.Coordinates.Count);
            Assert.Equal(3, geometry.Coordinates[0].Count);
            Assert.Equal(130.3, geometry.Coordinates[0][0].X);
        }

        [Fact]
        public void MultiPolygonFromWktIsOk()
        {
            var geometry = Geometry.FromWKT("MULTIPOLYGON (((130.3 110.2, 110.3 114.2, 140 140.6), (30.3 10.2, 10.3 14.2, 40 40.6)), ((230.3 210.2, 210.3 214.2, 240 240.6)))");
            Assert.IsType<MultiPolygon>(geometry);
            Assert.Equal(2, geometry.Coordinates.Count);
            Assert.Equal(2, geometry.Coordinates[0].Count);
            Assert.Equal(3, geometry.Coordinates[0][0].Count);
            Assert.Equal(110.3, geometry.Coordinates[0][0][1].X);
            Assert.Equal(40.6, geometry.Coordinates[0][1][2].Y);
        }
    }
}