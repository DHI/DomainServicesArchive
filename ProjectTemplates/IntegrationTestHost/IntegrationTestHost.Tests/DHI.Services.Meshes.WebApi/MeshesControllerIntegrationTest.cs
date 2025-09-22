namespace IntegrationTestHost.Tests
{
    using DHI.Spatial;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class MeshesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "dfsu";
        private const string MeshId = "PY_F012.dfsu";
        private const string Item = "1";
        private const string ItemNamed = "Sign. Wave Height";
        private const string Aggregation = "Average";
        private static readonly DateTime From = new(2014, 1, 1);
        private static readonly DateTime To = new(2014, 1, 2);
        private static readonly DateTime DateTime = new(2014, 1, 1, 12, 0, 0);

        public MeshesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full MeshesController integration suite")]
        public async Task Run_MeshesController_IntegrationFlow()
        {
            await Step("GetAll", async () => await AssertSuccess($"api/meshes/{ConnectionId}"));
            await Step("GetCount", async () => await AssertSuccess($"api/meshes/{ConnectionId}/count"));
            await Step("GetById", async () => await AssertSuccess($"api/meshes/{ConnectionId}/{MeshId}"));
            await Step("GetIds", async () => await AssertSuccess($"api/meshes/{ConnectionId}/ids"));
            await Step("GetFullNames", async () => await AssertSuccess($"api/meshes/{ConnectionId}/fullnames"));
            await Step("GetFullNamesByGroup", async () => await AssertSuccess($"api/meshes/{ConnectionId}/fullnames?group=copies"));
            await Step("GetByGroup", async () => await AssertSuccess($"api/meshes/{ConnectionId}?group=copies"));
            await Step("GetDateTimes", async () => await AssertSuccess($"api/meshes/{ConnectionId}/{MeshId}/datetimes"));
            await Step("GetValues", async () => await PostAndAssert($"api/meshes/{ConnectionId}/{MeshId}/{Item}/values", new Point(new Position(110, 6))));
            await Step("GetValuesForAllItems", async () => await PostAndAssert($"api/meshes/{ConnectionId}/{MeshId}/values", new Point(new Position(110, 6))));
            await Step("GetAggregatedValues", async () => await AssertSuccess($"api/meshes/{ConnectionId}/{MeshId}/{ItemNamed}/Maximum"));
            await Step("GetAggregatedValuesWithinPolygon", async () => await PostAndAssert($"api/meshes/{ConnectionId}/{MeshId}/{Item}/{Aggregation}", CreatePolygon()));
            await Step("GetAggregatedValuesByPeriod", async () => await AssertSuccess($"api/meshes/{ConnectionId}/{MeshId}/{ItemNamed}/Maximum/period/Daily"));
            await Step("GetAggregatedValuesWithinPolygonByPeriod", async () => await PostAndAssert($"api/meshes/{ConnectionId}/{MeshId}/{Item}/{Aggregation}/period/Daily", CreatePolygon()));
            await Step("GetAggregatedValue", async () => await AssertSuccess($"api/meshes/{ConnectionId}/{MeshId}/{ItemNamed}/Minimum/{DateTime:yyyy-MM-ddTHH:mm:ss}"));
            await Step("GetAggregatedValueWithinPolygon", async () => await PostAndAssert($"api/meshes/{ConnectionId}/{MeshId}/{Item}/{Aggregation}/{DateTime:yyyy-MM-ddTHH:mm:ss}", CreatePolygon()));
            await Step("GetContours", async () => await PostAndAssert($"api/meshes/{ConnectionId}/contours/{MeshId}/{Item}/2014-01-01", new[] { 0.1, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0 }));
            await Step("GetContoursInRange", async () => await PostAndAssert($"api/meshes/{ConnectionId}/contours/{MeshId}/{Item}?from=2014-01-01&to=2014-01-01", new[] { 0.1, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0 }));
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

        private async Task AssertSuccess(string url)
        {
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Expected success but got {(int)response.StatusCode}: {content}");
        }

        private async Task PostAndAssert<T>(string url, T body)
        {
            var response = await _client.PostAsync(url, MeshesContentHelper.GetStringContent(body));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _output.WriteLine(content);
                throw new Exception($"Expected success but got {(int)response.StatusCode}: {content}");
            }
        }

        private Polygon CreatePolygon()
        {
            return new Polygon
            {
                Coordinates =
                {
                    new List<Position>
                    {
                        new Position(100, 10),
                        new Position(110, 10),
                        new Position(100, 5),
                        new Position(100, 10)
                    }
                }
            };
        }
    }
}
