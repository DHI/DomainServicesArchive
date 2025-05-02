namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Maps;
    using NetCDF;
    using SkiaSharp;
    using Xunit;

    public class MapServiceTest : IClassFixture<MapSourceFixture>
    {
        private readonly MapSourceFixture _fixture;

        public MapServiceTest(MapSourceFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoMoqData]
        public void CreateWithNullArgumentsThrows(MapStyleService mapStyleService)
        {
            Assert.Throws<ArgumentNullException>(() => new MapService(null));
            Assert.Throws<ArgumentNullException>(() => new MapService(null, mapStyleService));
        }

        [Theory, AutoMapStyleData]
        public void GetMapUsingStyleIdThrowsIfNonExistingStyle(MapStyleService mapStyleService, Parameters parameters)
        {
            var mapService = new MapService(_fixture.MapSource, mapStyleService);
            var layerInfo = _fixture.MapSource.GetLayerInfo();
            Assert.Throws<KeyNotFoundException>(() => mapService.GetMap("MyStyle", "EPSG:4326", layerInfo.BoundingBox, 1000, 1000, string.Empty, null, string.Empty, parameters));
        }

        [Theory, AutoMapStyleData]
        public void GetMapUsingStyleIdIsOk(MapStyleService mapStyleService)
        {
            mapStyleService.Add(new MapStyle("MyStyle", "My Style") { StyleCode = _fixture.StyleCode });
            var mapService = new MapService(_fixture.MapSource, mapStyleService);
            var layerInfo = _fixture.MapSource.GetLayerInfo();
            var parameters = new Parameters { { "variable", "TRMM_daily_rainfall" } };
            var map = mapService.GetMap("MyStyle", "EPSG:4326", layerInfo.BoundingBox, 1000, 1000, null, null, string.Empty, parameters);
            var sample = SKBitmap.Decode(new MemoryStream(File.ReadAllBytes(_fixture.ImageFilePath)));

            Assert.True(map.IsSimilar(sample));
        }

        [Fact]
        public void GetMapUsingStyleCodeIsOk()
        {
            var mapService = new MapService(_fixture.MapSource);
            var layerInfo = _fixture.MapSource.GetLayerInfo();
            var parameters = new Parameters { { "variable", "TRMM_daily_rainfall" } };
            var map = mapService.GetMap(_fixture.StyleCode, "EPSG:4326", layerInfo.BoundingBox, 1000, 1000, string.Empty, null, string.Empty, parameters);
            var sample = SKBitmap.Decode(new MemoryStream(File.ReadAllBytes(_fixture.ImageFilePath)));
            Assert.True(map.IsSimilar(sample));
        }

        [Fact]
        public void GetDateTimesIsOk()
        {
            var mapService = new MapService(_fixture.MapSource);
            var dateTimes = mapService.GetDateTimes(_fixture.FileName);
            Assert.Equal(306, dateTimes.Count);
            Assert.Contains(new DateTime(2000, 3, 1), dateTimes);
        }

        [Fact]
        public void GetDateTimesWithinRangeIsOk()
        {
            var mapService = new MapService(_fixture.MapSource);
            var dateRange = new DateRange(new DateTime(2000, 3, 7), TimeSpan.FromDays(10));
            var dateTimes = mapService.GetDateTimes(_fixture.FileName, dateRange);
            Assert.Equal(11, dateTimes.Count);
            Assert.DoesNotContain(new DateTime(2000, 3, 1), dateTimes);
            Assert.Contains(new DateTime(2000, 3, 7), dateTimes);
        }
    }
}