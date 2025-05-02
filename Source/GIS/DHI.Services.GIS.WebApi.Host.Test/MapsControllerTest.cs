namespace DHI.Services.GIS.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;
    using Maps;
    using WebApiCore;
    using Xunit;
    using JsonConvert = System.Text.Json.JsonSerializer;

    [Collection("Controllers collection")]
    [SupportedOSPlatform("windows")]
    public class MapsControllerTest
    {
        public MapsControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
            fixture.CopyFileToTempAppDataPath("styles.json");
        }

        private readonly HttpClient _client;
        private readonly System.Text.Json.JsonSerializerOptions _options;
        private const string _connectionId = "dfsu-map";
        private const string _groupedConnectionId = "mc-groupedmap";
        private const string _fileId = "KBHEC3dF012.dfsu";

        [Theory]
        [InlineData("GetCapabilities")]
        [InlineData("GetLayerInfo")]
        [InlineData("GetFeatureInfo")]
        public async Task IllegalRequestTypeReturns501NotImplemented(string request)
        {
            var response = await _client.GetAsync($"api/maps?request={request}&width=500&height=500&styles=MyStyle");
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        }

        [Theory]
        [InlineData("api/maps?service=wms&version=1.3.0")]
        [InlineData("api/maps?request=GetMap")]
        [InlineData("api/maps?request=GetMap&width=500")]
        [InlineData("api/maps?request=GetMap&width=500&height=500")]
        [InlineData("api/maps?request=GetMap&width=500&height=500&styles=MyStyle")]
        [InlineData("api/maps?request=GetMap&width=500&height=500&styles=MyStyle&item=1")]
        [InlineData("api/maps?request=GetMap&width=500&height=500&styles=MyStyle&item=1&layers=dfs2-map")]
        [InlineData("api/maps?request=GetMap&width=500&height=500&styles=MyStyle&item=1&layers=dfs2-map&crs=EPSG:3857")]
        public async Task GetMapWithMissingParametersReturns400BadRequest(string url)
        {
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.True(json.Contains("One or more validation errors occurred.") || json.Contains("Value cannot be null."));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(50, 0)]
        [InlineData(0, 500)]
        [InlineData(-50, -500)]
        [InlineData(50, -500)]
        [InlineData(-50, 500)]
        public async Task GetMapWithNegativeOrZeroDimensionsReturns400BadRequest(int width, int height)
        {
            var response = await _client.GetAsync($"api/maps?request=GetMap&width={width}&height={height}&styles=MyStyle");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("cannot be zero or negative", json);
        }

        [Theory]
        [InlineData("api/maps?request=GetLegendGraphic", "width")]
        [InlineData("api/maps?request=GetLegendGraphic&width=500", "height")]
        [InlineData("api/maps?request=GetLegendGraphic&width=500&height=500", "styles")]
        public async Task GetLegendGraphicWithMissingParametersReturns400BadRequest(string url, string missingParameter)
        {
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.True(json.Contains($"The {missingParameter} field is required.") || json.Contains($"Parameter name: {missingParameter}"));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(50, 0)]
        [InlineData(0, 500)]
        [InlineData(-50, -500)]
        [InlineData(50, -500)]
        [InlineData(-50, 500)]
        public async Task GetLegendGraphicWithNegativeOrZeroDimensionsReturns400BadRequest(int width, int height)
        {
            var response = await _client.GetAsync($"api/maps?request=GetLegendGraphic&width={width}&height={height}&styles=MyStyle");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("cannot be zero or negative", json);
        }

        [Theory]
        [InlineData("/api/maps/dfsu-map")]
        [InlineData("/api/maps/dfsu-map&width=1068")]
        [InlineData("/api/maps/dfsu-map&width=1068&height=967")]
        [InlineData("/api/maps/dfsu-map&width=1068&height=967&bbox=1395013.9847186094,7487006.920533015,1415422.6712707514,7505485.572120957")]
        [InlineData("/api/maps/dfsu-map&width=1068&height=967&styles=Ecoli&bbox=1395013.9847186094,7487006.920533015,1415422.6712707514,7505485.572120957")]
        public async Task GetMultipleMapsWithMissingParametersReturns400BadRequest(string url)
        {
            var request = new
            {
                Url = url,
                Body = new Dictionary<DateTime, string>
                {
                    { DateTime.Parse("2018-09-12T12:00:00") , "KBHEC3dF012.dfsu" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("One or more validation errors occurred.", json);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(50, 0)]
        [InlineData(0, 500)]
        [InlineData(-50, -500)]
        [InlineData(50, -500)]
        [InlineData(-50, 500)]
        public async Task GetMultipleMapsWithNegativeOrZeroDimensionsReturns400BadRequest(int width, int height)
        {
            var request = new
            {
                Url = $"/api/maps/dfsu-map?&style=Ecoli&item=3&width={width}&height={height}&bbox=1395013.9847186094,7487006.920533015,1415422.6712707514,7505485.572120957",
                Body = new Dictionary<DateTime, string>
                {
                    { DateTime.Parse("2018-09-12T12:00:00") , "KBHEC3dF012.dfsu" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("cannot be zero or negative", json);
        }

        [Fact]
        public async Task GetDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_connectionId}/datetimes/{_fileId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var datetimes = JsonConvert.Deserialize<SortedSet<DateTime>>(json);

            Assert.Equal(25, datetimes.Count);
        }

        [Fact]
        public async Task GetDateTimesWithinRangeIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_connectionId}/datetimes/{_fileId}?from=2015-11-14T10:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var datetimes = JsonConvert.Deserialize<SortedSet<DateTime>>(json);

            Assert.Equal(25, datetimes.Count);
        }

        [Fact]
        public async Task GetMapIsOk()
        {
            var response = await _client.GetAsync("api/maps?request=GetMap&service=wms&version=1.3.0&layers=dfs2-map&item=1&styles=MyStyle&width=500&height=500&crs=EPSG:3857&bbox=11584184.510675031,78271.51696402066,11623320.26915704,117407.27544603013");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var ms = new MemoryStream(bytes);
            var image = Image.FromStream(ms);

            Assert.Equal("image/png", response.Content.Headers.ContentType.MediaType);
            Assert.IsType<Bitmap>(image);
        }

        [Fact]
        public async Task GetMultipleMapsIsOk()
        {
            var request = new
            {
                Url = $"/api/maps/{_connectionId}?&style=Ecoli&item=3&width=1068&height=967&bbox=1395013.9847186094,7487006.920533015,1415422.6712707514,7505485.572120957&shadingtype=ShadedContour&scale=1",
                Body = new Dictionary<DateTime, string>
                {
                    { DateTime.Parse("2018-09-12T12:00:00") , "KBHEC3dF012.dfsu" },
                    { DateTime.Parse("2018-09-12T12:30:00") , "KBHEC3dF012.dfsu" },
                    { DateTime.Parse("2018-09-12T13:00:00") , "KBHEC3dF012.dfsu" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var images = JsonConvert.Deserialize<Dictionary<DateTime, byte[]>>(json);
            var bytes = images[DateTime.Parse("2018-09-12T12:00:00")];
            var ms = new MemoryStream(bytes);
            var image = Image.FromStream(ms);

            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(3, images.Count);
            Assert.IsType<Bitmap>(image);
        }

        [Fact]
        public async Task GetLayerFullnameIdsIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonConvert.Deserialize<string[]>(json);
            Assert.Equal(5, fullnames.Length);
        }

        [Fact]
        public async Task GetLayerFullnameIdsByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers/fullnames?group=Riverbank");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonConvert.Deserialize<string[]>(json);
            Assert.Equal(4, fullnames.Length);
        }

        /// <remarks>
        ///     The ";nonrecursive"-convention is provider-specific (MCLite)
        /// </remarks>>
        [Fact]
        public async Task GetLayerIdsNonRecursiveIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers/fullnames?group=;nonrecursive");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonConvert.Deserialize<string[]>(json);
            Assert.Equal(2, fullnames.Length);
        }

        [Fact]
        public async Task GetAllLayersIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers");
            var json = await response.Content.ReadAsStringAsync();
            var layers = JsonConvert.Deserialize<Layer[]>(json, _options);
            Assert.Equal(5, layers.Length);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLayersByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers?group=Riverbank");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var layers = JsonConvert.Deserialize<Layer[]>(json, _options);
            Assert.Equal(4, layers.Length);
            Assert.Contains(layers, layer => layer.FullName == "Riverbank/Mean annual soil loss");
        }

        [Theory]
        [InlineData("DEM250")]
        [InlineData("Riverbank|Mean annual soil loss")]
        public async Task GetLayerIsOk(string layerId)
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers/{layerId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var layer = JsonConvert.Deserialize<Layer>(json, _options);
            Assert.True(layer.Metadata.Any());
            Assert.Equal(FullNameString.FromUrl(layerId), layer.FullName);
        }

        [Fact]
        public async Task GetStreamIsOk()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers/Riverbank|Mean annual soil loss/stream/ascii");
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("GetStream is not supported for SQLite.", json);
        }

        [Fact]
        public async Task GetStreamForNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/maps/{_groupedConnectionId}/layers/NonExistingId/stream/ascii");
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        }
    }
}