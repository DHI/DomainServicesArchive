namespace DHI.Services.Connections.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Controllers collection")]
    public class ConnectionControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ControllersFixture _fixture;

        public ConnectionControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/connections/NonExistingConnection");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTypeNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/connections/types/NonExistingConnectionType");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task VerifyNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/connections/NonExistingConnection/verification");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/connections",
                Body = new Dictionary<string, string>
                {
                    { "$type", "DHI.Services.TimeSeries.WebApi.TimeSeriesServiceConnection, DHI.Services.TimeSeries.WebApi" },
                    { "ConnectionString", "[AppData]"},
                    { "RepositoryType", "DHI.Services.TimeSeries.CSV.TimeSeriesRepository, DHI.Services.TimeSeries" },
                    { "Name", "CSV time series service connection"},
                    { "Id", "NonExistingConnectionId" }
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/connections",
                Body = new Dictionary<string, string>
                {
                    { "$type", "DHI.Services.TimeSeries.WebApi.TimeSeriesServiceConnection, DHI.Services.TimeSeries.WebApi" },
                    { "ConnectionString", "[AppData]"},
                    { "RepositoryType", "DHI.Services.TimeSeries.CSV.TimeSeriesRepository, DHI.Services.TimeSeries" },
                    { "Name", "CSV time series service connection"},
                    { "Id", "csv" }
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
            var response = await _client.DeleteAsync("api/connections/not-existing-id");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/connections/csv");
            var json = await response.Content.ReadAsStringAsync();
            var connection = JsonSerializer.Deserialize<IConnection>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("csv", connection.Id);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/connections");
            var json = await response.Content.ReadAsStringAsync();
            var connections = JsonSerializer.Deserialize<IEnumerable<IConnection>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(connections.Any());
            Assert.Contains(connections, connection => connection.Id == "csv");
        }

        [Fact]
        public async Task GetTypeIsOk()
        {
            var response = await _client.GetAsync("api/connections/types/TimeSeriesServiceConnection");
            var json = await response.Content.ReadAsStringAsync();
            var type = JsonSerializer.Deserialize<ConnectionType>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("TimeSeriesServiceConnection", type.Id);
        }

        [Fact]
        public async Task GetTypeIdsIsOk()
        {
            var response = await _client.GetAsync("api/connections/types/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(ids.Any());
            Assert.Contains(ids, id => id == "GroupedUpdatableTimeSeriesServiceConnection");
        }

        [Fact]
        public async Task GetAllTypesIsOk()
        {
            var response = await _client.GetAsync("api/connections/types");
            var json = await response.Content.ReadAsStringAsync();
            var types = JsonSerializer.Deserialize<ConnectionType[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(types.Any());
            Assert.Contains(types, type => type.Id == "DiscreteTimeSeriesServiceConnection");
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/connections/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = "api/connections",
                Body = new Dictionary<string, string>
                {
                    { "$type", "DHI.Services.TimeSeries.WebApi.DiscreteTimeSeriesServiceConnection, DHI.Services.TimeSeries.WebApi" },
                    { "ConnectionString", "[AppData]" },
                    { "RepositoryType", "DHI.Services.TimeSeries.CSV.TimeSeriesRepository, DHI.Services.TimeSeries" },
                    { "Name", "CSV time series service connection"},
                    { "Id", "csv-discrete" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var connection = JsonSerializer.Deserialize<IConnection>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/api/connections/csv-discrete", response.Headers.Location.ToString());
            Assert.Equal("csv-discrete", connection.Id);

            // Update
            request.Body["Name"] = "CSV time series service connection (UPDATED)";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            connection = JsonSerializer.Deserialize<IConnection>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(request.Body["Name"], connection.Name);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{connection.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{connection.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task VerifyExistingIsOk()
        {
            var response = await _client.GetAsync("api/connections/csv/verification");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task VerifyNewIsOk()
        {
            var request = new
            {
                Url = "api/connections/verification",
                Body = new Dictionary<string, string>
                {
                    { "$type", "DHI.Services.TimeSeries.WebApi.DiscreteTimeSeriesServiceConnection, DHI.Services.TimeSeries.WebApi" },
                    { "ConnectionString", "[AppData]"},
                    { "RepositoryType", "DHI.Services.TimeSeries.CSV.TimeSeriesRepository, DHI.Services.TimeSeries" },
                    { "Name", "CSV time series service connection"},
                    { "Id", "csv-discrete" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}