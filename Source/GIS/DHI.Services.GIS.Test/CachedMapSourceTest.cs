namespace DHI.Services.GIS.Test
{
    using System;
    using Maps;
    using Spatial;
    using Xunit;

    public class CachedMapSourceTest
    {
        [Fact]
        public void CreateWithNullMapSourceThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new CachedMapSource(null, new Parameters()));
        }


        [Theory, AutoMoqData]
        public void CreateWithNullParametersThrows(IMapSource mapSource)
        {
            Assert.Throws<ArgumentNullException>(() => new CachedMapSource(mapSource, null));
        }

        [Theory, AutoMoqData]
        public void GetMapWithWrongCoordinateSystemThrows(IMapSource mapSource, MapStyle mapStyle, string crs, BoundingBox bbox, int width, int height, string filePath, DateTime? dateTime, string item, Parameters parameters)
        {
            var cachedMapSource = new CachedMapSource(mapSource, parameters);
            var e = Assert.Throws<Exception>(() => cachedMapSource.GetMap(mapStyle, crs, bbox, width, height, filePath, dateTime, item, parameters));
            Assert.Contains("Only the Google Maps coordinate system (EPSG:3857) is supported.", e.Message);
        }
    }
}