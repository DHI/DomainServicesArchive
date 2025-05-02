namespace DHI.Services.Places.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using WebApiCore;
    using Xunit;

    [Collection("Controllers collection")]
    public class PlaceControllerTest
    {
        public PlaceControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private JsonSerializerOptions _options;
        private const string ConnectionId = "json";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/places/{ConnectionId}/MyStation");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetWithNonExistingConnectionIdReturns404NotFound()
        {
            var response = await _client.GetAsync("api/places/NonExistingConnection/MyStation");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/NonExistingStation");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = $"/api/places/{ConnectionId}",
                Body = new PlaceDTO
                {
                    FullName = "Stations/MyStation",
                    FeatureId = new FeatureId("Stationer.shp", "StatId", "ID92_M16")
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/places/{ConnectionId}/MyStations|NonExistingStation");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/Stations|MyStation");
            var json = await response.Content.ReadAsStringAsync();
            var place = JsonSerializer.Deserialize<PlaceDTO>(json, _options);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Stations/MyStation", place.FullName);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JsonSerializer.Deserialize<List<PlaceDTO>>(json, _options);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetIndicatorIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/Stations|MyStation/indicators/WaterLevel");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var indicator = JsonSerializer.Deserialize<IndicatorDTO>(json, _options);
            Assert.Equal("TimeSeries", indicator.DataSource.Type.ToString());
        }

        [Fact]
        public async Task GetIndicatorsByPlaceIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/Stations|MyStation/indicators");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JsonSerializer.Deserialize<Dictionary<string, IndicatorDTO>>(json, _options);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetIndicatorsByTypeIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/indicators/WaterLevel");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JsonSerializer.Deserialize<Dictionary<string, IndicatorDTO>>(json, _options);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetIndicatorStatusByTypeIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/indicators/WaterLevel/status");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetIndicatorStatusIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/Stations|MyStation/indicators/WaterLevel/status?datetime=2015-12-01");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetThresholdValuesIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/Stations|MyStation/thresholds/WaterLevel");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JsonSerializer.Deserialize<List<double>>(json);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetThresholdValuesByPlaceIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/Stations|MyStation/thresholds");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JsonSerializer.Deserialize<Dictionary<string, IList<double>>>(json);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetFeaturesIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/features?datetime=2015-11-19");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFeaturesWithStatusDateTimeIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/features?datetime=2015-11-19&includeIndicatorStatus=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFeaturesWithStatusFromToIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/features?from=2015-11-10&to=2015-11-19&includeIndicatorStatus=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var response = await _client.GetAsync($"api/places/{ConnectionId}/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<string>>(json);
            Assert.Single(list);
        }

        [Fact]
        public async Task AddAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/places/{ConnectionId}",
                Body = new PlaceDTO
                {
                    FullName = "Stations/MyTestStation",
                    FeatureId = new FeatureId("Stationer.shp", "StatId", "ID92_M16"),
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var placeDTO = JsonSerializer.Deserialize<PlaceDTO>(json, _options);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/places/{ConnectionId}/{FullNameString.ToUrl(placeDTO.FullName)}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, placeDTO.FullName);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(placeDTO.FullName)}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(placeDTO.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
