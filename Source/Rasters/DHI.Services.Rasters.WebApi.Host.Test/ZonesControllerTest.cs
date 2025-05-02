namespace DHI.Services.Rasters.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using System.Text.Json;
    using Xunit;
    using Zones;

    [Collection("Controllers collection")]
    public class ZonesControllerTest
    {
        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ZonesControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        public async Task AddExistingNameReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/zones",
                Body = new ZoneDTO
                {
                    Id = "AarhusAaCatchment",
                    Name = "Aarhus Aa Catchment",
                    PixelWeights = new HashSet<PixelWeight>
                    {
                        new PixelWeight(new Pixel(3, 4), new Weight(0.5)),
                        new PixelWeight(new Pixel(4, 4), new Weight(0.5))
                    },
                    Type = ZoneType.LineString.ToString(),
                    ImageSize = new Size(256, 256)
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already a zone with the name", json);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/zones",
                Body = new ZoneDTO
                {
                    Id = "AarhusAa",
                    Name = "Aarhus Aa",
                    PixelWeights = new HashSet<PixelWeight>
                    {
                        new PixelWeight(new Pixel(3, 4), new Weight(0.5)),
                        new PixelWeight(new Pixel(4, 4), new Weight(0.5))
                    },
                    Type = ZoneType.LineString.ToString(),
                    ImageSize = new Size(256, 256)
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = "/api/zones",
                Body = new ZoneDTO
                {
                    Id = "AarhusAaCatchment",
                    Name = "Aarhus Aa",
                    PixelWeights = new HashSet<PixelWeight>
                    {
                        new PixelWeight(new Pixel(3, 4), new Weight(0.5)),
                        new PixelWeight(new Pixel(4, 4), new Weight(0.5))
                    },
                    Type = ZoneType.LineString.ToString(),
                    ImageSize = new Size(256, 256)
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var zone = JsonSerializer.Deserialize<Zone>(json, SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/api/zones/AarhusAaCatchment", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, zone.Id);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{zone.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync($"{request.Url}/{zone.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddWithMissingPixelWeightsReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/zones",
                Body = new ZoneDTO
                {
                    Id = "AarhusAaCatchment",
                    Name = "Aarhus Aa Catchment",
                    Type = ZoneType.LineString.ToString(),
                    ImageSize = new Size(256, 256)
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The PixelWeights field is required", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/zones/NonExistingZone");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/zones");
            var json = await response.Content.ReadAsStringAsync();
            var zones = JsonSerializer.Deserialize<IEnumerable<Zone>>(json, SerializerOptionsDefault.Options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, zones.Length);
            Assert.Contains(zones, z => z.Id == "AarhusAa");
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/zones/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync("api/zones/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, ids.Length);
            Assert.Contains("AarhusAa", ids);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/zones/AarhusAa");
            var json = await response.Content.ReadAsStringAsync();
            var zone = JsonSerializer.Deserialize<Zone>(json, SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("AarhusAa", zone.Id);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/zones/NonExistingZone");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync("api/zones/AarhusAa");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }
    }
}