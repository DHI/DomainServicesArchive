namespace DHI.Services.GIS.Test.NetCDF
{
    using System;
    using System.IO;
    using Maps;
    using Xunit;
    using System.Runtime.Versioning;
    using GIS.NetCDF;
    using SkiaSharp;

    public class MapSourceTest : IClassFixture<MapSourceFixture>
    {
        private readonly MapSourceFixture _fixture;

        public MapSourceTest(MapSourceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new MapSource(null, null));
        }

        [Fact]
        public void GetLayerInfoIsOk()
        {
            var layerInfo = _fixture.MapSource.GetLayerInfo();
            Assert.Equal(98.125, layerInfo.BoundingBox.Xmin);
        }

        [Fact]
        public void GetMapIsOk()
        {
            var style = new MapStyle("MyStyle", "My Style") {StyleCode = _fixture.StyleCode};
            var layerInfo = _fixture.MapSource.GetLayerInfo();
            var parameters = new Parameters {{"variable", "TRMM_daily_rainfall"}};
            var map = _fixture.MapSource.GetMap(style, string.Empty, layerInfo.BoundingBox, 1000, 1000, string.Empty, null, string.Empty, parameters);
            using var sample = SKBitmap.Decode(new MemoryStream(File.ReadAllBytes(_fixture.ImageFilePath)));

            Assert.True(map.IsSimilar(sample));
        }
    }
}
