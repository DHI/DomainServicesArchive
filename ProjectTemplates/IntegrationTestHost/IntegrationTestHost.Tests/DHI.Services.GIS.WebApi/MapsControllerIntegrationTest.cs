namespace IntegrationTestHost.Tests
{
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class MapsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string ConnectionId = "dfsu-map";
        private const string GroupedConnectionId = "mc-groupedmap";
        private const string FileId = "KBHEC3dF012.dfsu";

        public MapsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full MapsController integration suite")]
        public async Task Run_MapsController_IntegrationFlow()
        {
            await Step("GetDateTimes", GetDateTimes);
            await Step("GetDateTimesWithinRange", GetDateTimesWithinRange);
            await Step("GetMap", GetMap);
            await Step("GetMultipleMaps", GetMultipleMaps);
            await Step("GetLayerFullnameIds", GetLayerFullnameIds);
            await Step("GetLayerFullnameIdsByGroup", GetLayerFullnameIdsByGroup);
            await Step("GetLayerIdsNonRecursive", GetLayerIdsNonRecursive);
            await Step("GetAllLayers", GetAllLayers);
            await Step("GetLayersByGroup", GetLayersByGroup);
            await Step("GetLayerById", GetLayerById);
            await Step("GetStreamForNonExisting", GetStreamForNonExisting);
        }

        private async Task Step(string name, Func<Task> func)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                await func();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task GetDateTimes()
        {
            var response = await _client.GetAsync($"api/maps/{ConnectionId}/datetimes/{FileId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetDateTimesWithinRange()
        {
            var response = await _client.GetAsync($"api/maps/{ConnectionId}/datetimes/{FileId}?from=2015-11-14T10:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMap()
        {
            var url = "api/maps?request=GetMap&service=wms&version=1.3.0&layers=dfs2-map&item=1&styles=MyStyle" +
                      "&width=500&height=500&crs=EPSG:3857&bbox=11584184.510675031,78271.51696402066," +
                      "11623320.26915704,117407.27544603013";
            var response = await _client.GetAsync(url);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMultipleMaps()
        {
            var url = $"/api/maps/{ConnectionId}?style=Ecoli&item=3&width=1068&height=967" +
                      "&bbox=1395013.9847186094,7487006.920533015,1415422.6712707514,7505485.572120957";
            var body = new Dictionary<DateTime, string>
            {
                { DateTime.Parse("2018-09-12T12:00:00"), FileId },
                { DateTime.Parse("2018-09-12T12:30:00"), FileId },
                { DateTime.Parse("2018-09-12T13:00:00"), FileId }
            };
            var response = await _client.PostAsync(url, GISContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLayerFullnameIds()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLayerFullnameIdsByGroup()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers/fullnames?group=Riverbank");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLayerIdsNonRecursive()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers/fullnames?group=;nonrecursive");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAllLayers()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLayersByGroup()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers?group=Riverbank");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLayerById()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers/DEM250");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetStreamForNonExisting()
        {
            var response = await _client.GetAsync($"api/maps/{GroupedConnectionId}/layers/NonExistingId/stream/ascii");
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        }
    }
}
