namespace DHI.Services.Tables.WebApi.Host.Test
{
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    //ref: https://xunit.net/docs/shared-context#class-fixture
    [Collection("Controllers collection")]
    public class TablesControllerTest : IClassFixture<ControllersFixture>
    {
        public TablesControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string _connectionId = "accessdb";

        [SkippableFact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/tables/{_connectionId}/Stations");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [SkippableFact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}/NonExistingTable");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [SkippableFact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}/Stations");
            var json = await response.Content.ReadAsStringAsync();
            var table = JsonSerializer.Deserialize<Table>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Stations", table.Id);
        }

        [SkippableFact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, count);
        }

        [SkippableFact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Stations", ids);
            Assert.Equal(2, ids.Length);
        }

        [SkippableFact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var tables = JsonSerializer.Deserialize<Table[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(tables, table => table.Id == "Stations");
            Assert.Equal(2, tables.Length);
        }

        [SkippableFact]
        public async Task GetDataIsOk()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}/Stations/data");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, data.GetLongLength(0));
            Assert.Equal(5, data.GetLongLength(1));
            Assert.Equal("Station 1", data[0, 1]);
        }

        [SkippableFact]
        public async Task GetQueriedDataIsOk()
        {
            var response = await _client.GetAsync($"api/tables/{_connectionId}/Stations/data?Name=Station 1");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, data.GetLongLength(0));
            Assert.Equal(5, data.GetLongLength(1));
            Assert.Equal("Station 1", data[0, 1]);
        }
    }
}
