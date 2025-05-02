namespace DHI.Services.Meshes.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using TimeSeries;
    using Spatial;
    using Microsoft.AspNetCore.Http;
    using Xunit;
    using System.Text.Json;
    using JsonConvert = System.Text.Json.JsonSerializer;

    [Collection("Controllers collection")]
    public class MeshesControllerTest
    {
        public MeshesControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = new JsonSerializerOptions(_fixture.SerializerOptions);
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private const string _connectionId = "dfsu";
        private readonly JsonSerializerOptions _options;

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/meshes/{_connectionId}");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Theory, AutoData]
        public async Task GetWithNonExistingConnectionIdReturns404NotFound(string connectionId)
        {
            var response = await _client.GetAsync($"api/meshes/{connectionId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory, AutoData]
        public async Task GetNonExistingReturns404NotFound(string id)
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory, AutoData]
        public async Task GetByNonExistingGroupReturns404NotFound(string group)
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}?group={group}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory, AutoData]
        public async Task GetFullNamesByNonExistingGroupReturns404NotFound(string group)
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/fullnames?group={group}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory, AutoData]
        public async Task GetDateTimesForNonExistingReturns404NotFound(string id)
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/{id}/datetimes");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/PY_F012.dfsu");
            var json = await response.Content.ReadAsStringAsync();
            var meshInfo = JsonConvert.Deserialize<MeshInfo>(json, _options);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("PY_F012.dfsu", meshInfo.FullName);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var meshes = JsonConvert.Deserialize<IEnumerable<MeshInfo>>(json, _options).ToArray();

            Assert.NotEmpty(meshes);
            Assert.Contains("PY_F012.dfsu", meshes.Select(m => m.Id));
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonConvert.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("copies\\PY_F012 - Copy.dfsu", fullnames);
            Assert.Equal(2, fullnames.Length);
        }

        [Fact]
        public async Task GetFullNamesByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/fullnames?group=copies");
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonConvert.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("copies\\PY_F012 - Copy.dfsu", fullnames);
            Assert.Single(fullnames);
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var ids = JsonConvert.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.NotEmpty(ids);
            Assert.Contains("PY_F012.dfsu", ids);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var count = JsonConvert.Deserialize<int>(json);
            Assert.True(count > 0);
        }

        [Fact]
        public async Task GetByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}?group=copies");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var meshes = JsonConvert.Deserialize<IEnumerable<MeshInfo>>(json, _options).ToArray();

            Assert.NotEmpty(meshes);
            Assert.Contains("copies\\PY_F012 - Copy.dfsu", meshes.Select(m => m.Id));
        }

        [Fact]
        public async Task GetValuesIsOk()
        {
            var point = new Point(new Position(110, 6));
            var geoJson = JsonConvert.Serialize(point, _options);
            var request = new
            {
                Url = $"/api/meshes/{_connectionId}/PY_F012.dfsu/1/values",
                Body = geoJson
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesData = JsonConvert.Deserialize<ITimeSeriesData<double>>(json, _options);
            Assert.NotEmpty(timeSeriesData.Values);
        }

        [Fact]
        public async Task GetValuesForAllItemsIsOk()
        {
            var point = new Point(new Position(110, 6));
            var geoJson = JsonConvert.Serialize(point, _options);
            var request = new
            {
                Url = $"/api/meshes/{_connectionId}/PY_F012.dfsu/values",
                Body = geoJson
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var dictionary = JsonConvert.Deserialize<Dictionary<string, ITimeSeriesData<double>>>(json, _options);
            const string item = "Sign. Wave Height";
            Assert.Contains(item, dictionary.Keys);
            Assert.NotEmpty(dictionary[item].Values);
        }

        [Fact]
        public async Task GetAggregatedValuesIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/PY_F012.dfsu/Sign. Wave Height/Maximum");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesData = JsonConvert.Deserialize<ITimeSeriesData<double>>(json, _options);
            Assert.NotEmpty(timeSeriesData.Values);
        }

        [Fact]
        public async Task GetAggregatedValuesWithinPolygonIsOk()
        {
            var polygon = new Polygon();
            polygon.Coordinates.Add(new List<Position> { new Position(100, 10), new Position(110, 10), new Position(100, 5), new Position(100, 10) });
            var geoJson = JsonConvert.Serialize(polygon, _options);
            var request = new
            {
                Url = $"/api/meshes/{_connectionId}/PY_F012.dfsu/1/Average",
                Body = geoJson
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesData = JsonConvert.Deserialize<ITimeSeriesData<double>>(json, _options);
            Assert.NotEmpty(timeSeriesData.Values);
        }

        [Fact]
        public async Task GetAggregatedValuesByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/PY_F012.dfsu/Sign. Wave Height/Maximum/period/Daily");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesData = JsonConvert.Deserialize<ITimeSeriesData<double>>(json, _options);
            Assert.Single(timeSeriesData.DateTimes);
            Assert.Equal(new DateTime(2014, 1, 1), timeSeriesData.DateTimes.Single());
        }

        [Fact]
        public async Task GetAggregatedValuesByPeriodWithinPolygonIsOk()
        {
            var polygon = new Polygon();
            polygon.Coordinates.Add(new List<Position> { new Position(100, 10), new Position(110, 10), new Position(100, 5), new Position(100, 10) });
            var geoJson = JsonConvert.Serialize(polygon, _options);
            var request = new
            {
                Url = $"/api/meshes/{_connectionId}/PY_F012.dfsu/1/Average/period/Daily",
                Body = geoJson
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesData = JsonConvert.Deserialize<ITimeSeriesData<double>>(json, _options);
            Assert.Single(timeSeriesData.DateTimes);
            Assert.Equal(new DateTime(2014, 1, 1), timeSeriesData.DateTimes.Single());
        }

        [Fact]
        public async Task GetAggregatedValueIsOk()
        {
            var response = await _client.GetAsync($"api/meshes/{_connectionId}/PY_F012.dfsu/Sign. Wave Height/Minimum/2014-01-01T12:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var value = JsonConvert.Deserialize<double>(json, _options);
            Assert.Equal(0.0004005056, value, 10);
        }

        [Fact]
        public async Task GetAggregatedValueWithinPolygonIsOk()
        {
            var polygon = new Polygon();
            polygon.Coordinates.Add(new List<Position> { new Position(100, 10), new Position(110, 10), new Position(100, 5), new Position(100, 10) });
            var geoJson = JsonConvert.Serialize(polygon, _options);
            var request = new
            {
                Url = $"/api/meshes/{_connectionId}/PY_F012.dfsu/1/Average/2014-01-01T12:00:00",
                Body = geoJson
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var value = JsonConvert.Deserialize<double>(json, _options);
            Assert.Equal(1.7166166134, value, 10);
        }

        [Fact]
        public async Task GetContoursIsOk()
        {
            var request = new
            {
                Url = $"/api/meshes/{_connectionId}/contours/PY_F012.dfsu/1/2014-01-01",
                Body = new[] { 0.1, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0 }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"type\": \"FeatureCollection\"", json);
        }
    }
}