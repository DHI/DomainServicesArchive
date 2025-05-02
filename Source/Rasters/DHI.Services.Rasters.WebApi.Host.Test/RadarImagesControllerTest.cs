namespace DHI.Services.Rasters.WebApi.Host.Test
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Text.Json;
    using Microsoft.AspNetCore.Http;
    using Radar;
    using Radar.DELIMITEDASCII;
    using Xunit;
    using DHI.Services.Rasters.WebApi;

    [Collection("Controllers collection")]
    public class RadarImagesControllerTest
    {
        private readonly HttpClient _client;
        private readonly ControllersFixture _fixture;
        private const string _connectionId = "ascii";
        private readonly JsonSerializerOptions _options;

        public RadarImagesControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;

            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _options.Converters.Add(new PixelValueTypeConverter());
        }

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/radarimages/{_connectionId}/2018-03-17T13:00:00");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingDateTimeReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/2099-03-17T13:00:00");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/2018-03-17T13:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var image = JsonSerializer.Deserialize<AsciiImage>(json, _options);

            Assert.Equal(PixelValueType.Intensity, image.PixelValueType);
        }

        [Fact]
        public async Task GetLastIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/last");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var image = JsonSerializer.Deserialize<AsciiImage>(json, _options);

            Assert.Equal(PixelValueType.Intensity, image.PixelValueType);
        }

        [Fact]
        public async Task GetLastBeforeIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/lastbefore/2018-03-17T15:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var image = JsonSerializer.Deserialize<AsciiImage>(json, _options);

            Assert.Equal(PixelValueType.Intensity, image.PixelValueType);
        }

        [Fact]
        public async Task GetLastBeforeForEarlyDateReturns400BadRequest()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/lastbefore/1900-03-17T15:00:00");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("No rasters before", json);
        }

        [Fact]
        public async Task GetLastBeforeForManyIsOk()
        {
            var request = new
            {
                Url = $"api/radarimages/{_connectionId}/list/lastbefore",
                Body = new[]
                {
                    "2018-03-17T15:00:00",
                    "2018-03-17T14:00:00"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var images = JsonSerializer.Deserialize<AsciiImage[]>(json, _options);

            Assert.Equal(PixelValueType.Intensity, images[0].PixelValueType);
        }

        [Fact]
        public async Task GetFirstAfterIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/firstafter/2018-03-17T14:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var image = JsonSerializer.Deserialize<AsciiImage>(json, _options);

            Assert.Equal(PixelValueType.Intensity, image.PixelValueType);
        }

        [Fact]
        public async Task GetFirstAfterForLateDateReturns400BadRequest()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/firstafter/2099-03-17T15:00:00");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("No rasters after", json);
        }

        [Fact]
        public async Task GetFirstAfterForManyIsOk()
        {
            var request = new
            {
                Url = $"api/radarimages/{_connectionId}/list/firstafter",
                Body = new[]
                {
                    "2018-03-17T13:00:00",
                    "2018-03-17T14:00:00"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var images = JsonSerializer.Deserialize<AsciiImage[]>(json, _options);

            Assert.Equal(PixelValueType.Intensity, images[0].PixelValueType);
        }

        [Fact]
        public async Task GetDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/datetimes?from=2018-03-17T13:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var dateTimes = JsonSerializer.Deserialize<string[]>(json, _options);

            Assert.Equal(2, dateTimes.Length);
            Assert.IsType<DateTime>(DateTime.Parse(dateTimes[0]));
        }

        [Fact]
        public async Task GetFirstDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/datetime/first");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var dateTime = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(new DateTime(2018, 03, 17, 13, 0, 0), dateTime);
        }

        [Fact]
        public async Task GetLastDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/datetime/last");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var dateTime = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(new DateTime(2018, 03, 17, 15, 0, 0), dateTime);
        }

        [Fact]
        public async Task GetDateTimesFirstAfterIsOk()
        {
            var request = new
            {
                Url = $"api/radarimages/{_connectionId}/datetimes/firstafter",
                Body = new[]
                {
                    "2018-03-17T13:00:00",
                    "2018-03-17T14:00:00"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var dateTimes = JsonSerializer.Deserialize<DateTime[]>(json, _options);

            Assert.Equal(2, dateTimes.Length);
        }

        [Fact]
        public async Task GetDateTimesLastBeforeIsOk()
        {
            var request = new
            {
                Url = $"api/radarimages/{_connectionId}/datetimes/lastbefore",
                Body = new[]
                {
                    "1900-03-17T12:00:00",
                    "2018-03-17T14:00:00"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var dateTimes = JsonSerializer.Deserialize<DateTime[]>(json, _options);

            Assert.Single(dateTimes);
        }

        [Fact]
        public async Task GetDepthIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/depth/TestPoint?from=2018-03-17T12:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var depth = JsonSerializer.Deserialize<double>(json, _options);

            Assert.Equal(0, depth);
        }

        [Fact]
        public async Task GetAsBitmapIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/2018-03-17T14:00:00/bitmap");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            using var ms = new MemoryStream(bytes);
            var image = Image.FromStream(ms);

            Assert.Equal(new MediaTypeHeaderValue("image/png"), response.Content.Headers.ContentType);
            Assert.IsType<Bitmap>(image);
        }

        [Fact]
        public async Task GetLastAsBitmapIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/last/bitmap");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            using var ms = new MemoryStream(bytes);
            var image = Image.FromStream(ms);

            Assert.Equal(new MediaTypeHeaderValue("image/png"), response.Content.Headers.ContentType);
            Assert.IsType<Bitmap>(image);
        }

        [Fact]
        public async Task GetStyleAsBitmapIsOk()
        {
            var response = await _client.GetAsync($"api/radarimages/{_connectionId}/style/IntensityDefault/bitmap");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            using var ms = new MemoryStream(bytes);
            var image = Image.FromStream(ms);

            Assert.Equal(new MediaTypeHeaderValue("image/png"), response.Content.Headers.ContentType);
            Assert.IsType<Bitmap>(image);
        }
    }
}