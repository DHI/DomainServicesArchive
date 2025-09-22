namespace IntegrationTestHost.Tests
{
    using DHI.Services.Models;
    using DHI.Services.Models.WebApi;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class ScenarioModelsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "json-models-scenarios";
        private const string ScenarioId = "scenario1";
        private const string ReaderId = "fakeReader";
        private const string SimulationId = "185ba082-94ec-4b6a-bc7a-ceed263a33ed";
        private const string TimeSeriesId = "ts1-out";

        private Scenario? _scenario;

        public ScenarioModelsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full ScenarioControllers inside model integration suite")]
        public async Task Run_ScenarioControllers_IntegrationFlow()
        {
            await Step("Get", Get);
            await Step("GetAll", GetAll);
            await Step("GetSimulation", GetSimulation);
            await Step("GetSimulationData", GetSimulationData);
            await Step("Add", Add);
            await Step("Update", Update);
            await Step("Delete", Delete);
            await Step("CreateDerived", CreateDerived);
            await Step("DeleteDerived", DeleteDerived);
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

        private async Task Get()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{ConnectionId}/{ScenarioId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAll()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSimulation()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{ConnectionId}/{ScenarioId}/simulations");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSimulationData()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{ConnectionId}/{ScenarioId}/simulations/{SimulationId}/data/{TimeSeriesId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Add()
        {
            var (url, body) = CreateScenarioRequest();

            var response = await _client.PostAsync(url, ModelsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);
        }

        private async Task Update()
        {
            var (url, body) = CreateScenarioRequest("Updated name");
            var response = await _client.PutAsync(url, ModelsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);
        }

        private async Task Delete()
        {
            var (url, body) = CreateScenarioRequest();

            var response = await _client.DeleteAsync($"{url}/{_scenario?.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task CreateDerived()
        {
            var response = await _client.PostAsync($"api/models/scenarios/{ConnectionId}/{ScenarioId}/derived?derivedName=derivedScenario&simulationId={SimulationId}", null);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);
        }

        private async Task DeleteDerived()
        {
            var response = await _client.DeleteAsync($"api/models/scenarios/{ConnectionId}/{_scenario?.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private (string Url, ScenarioDto Body) CreateScenarioRequest(string? nameOverride = null)
        {
            var body = new ScenarioDto
            {
                Id = "MyScenario",
                Name = nameOverride ?? "My Scenario",
                ModelDataReaderId = $"{ReaderId}",
                ParameterValues = new Dictionary<string, object> { { "foo", 12345 } },
                InputTimeSeriesValues = new Dictionary<string, string>
                    {
                        { "ts1-in", "MyTimeSeriesId" }
                    }
            };

            return ($"/api/models/scenarios/{ConnectionId}", body);
        }
    }
}
