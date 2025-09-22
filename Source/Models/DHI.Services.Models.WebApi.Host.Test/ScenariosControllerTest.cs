namespace DHI.Services.Models.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using TimeSeries;
    using TimeSeries.Converters;
    using Xunit;

    [Collection("Controllers collection")]
    public class ScenariosControllerTest
    {
        public ScenariosControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private const string _connectionId = "json-scenarios";
        private const string _scenarioId = "scenario1";
        private const string _readerId = "fakeReader";
        private const string _simulationId = "185ba082-94ec-4b6a-bc7a-ceed263a33ed";
        private const string _timeSeriesId = "ts1-out";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetWithNonExistingConnectionIdReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/models/scenarios/NonExistingConnection/{_scenarioId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/NonExistingScenario");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetSimulationsForNonExistingScenarioReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/NonExistingScenario/simulations");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            using var doc = JsonDocument.Parse(json);
            var error = doc.RootElement.GetProperty("error").GetString();

            Assert.Contains("NonExistingScenario", error);
            Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingScenarioReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/NonExistingScenario/simulations/{_simulationId}/data/{_timeSeriesId}");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            using var doc = JsonDocument.Parse(json);
            var error = doc.RootElement.GetProperty("error").GetString();

            Assert.Contains("NonExistingScenario", error);
            Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingSimulationReturns404NotFound()
        {
            var simulationId = Guid.NewGuid();
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}/simulations/{simulationId}/data/{_timeSeriesId}");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            using var doc = JsonDocument.Parse(json);
            var error = doc.RootElement.GetProperty("error").GetString();

            Assert.Contains("Simulation", error);
            Assert.Contains(simulationId.ToString(), error);
            Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingDataReturns400BadRequest()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}/simulations/{_simulationId}/data/NonExistingData");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var doc = JsonDocument.Parse(json);
            var error = doc.RootElement.GetProperty("error").GetString();

            Assert.Contains("NonExistingData", error);
            Assert.Contains("not a valid output time series", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/models/scenarios/{_connectionId}/NonExistingScenario");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            using var doc = JsonDocument.Parse(json);
            var error = doc.RootElement.GetProperty("error").GetString();

            Assert.Contains("NonExistingScenario", error);
            Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateDerivedForNonExistingScenarioReturns404NotFound()
        {
            var response = await _client.PostAsync($"api/models/scenarios/{_connectionId}/NonExistingScenario/derived?derivedName=derivedScenario&simulationId={_simulationId}", null);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("NonExistingScenario", json);
            Assert.Contains("not found", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateDerivedForNonExistingSimulationReturns404NotFound()
        {
            var simulationId = Guid.NewGuid();
            var response = await _client.PostAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}/derived?derivedName=derivedScenario&simulationId={simulationId}", null);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains($"{simulationId}", json);
            Assert.Contains("Simulation", json);
            Assert.Contains("not found", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);

            Assert.Contains("foo", scenario.ParameterValues);
            Assert.Contains("ts1-in", scenario.InputTimeSeriesValues);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var fooValue = doc.RootElement
            .GetProperty("ParameterValues")
            .GetProperty("foo")
            .GetInt64();

            Assert.Equal(123L, fooValue);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var scenarios = JsonSerializer.Deserialize<IEnumerable<Scenario>>(json, SerializerOptionsDefault.Options).ToArray();
            
            Assert.NotEmpty(scenarios);
            Assert.Contains("fakeReader", scenarios.Select(s => s.ModelDataReaderId));
        }

        [Fact]
        public async Task GetSimulationsIsOk()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}/simulations");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var simulations = JsonSerializer.Deserialize<IEnumerable<Simulation>>(json, SerializerOptionsDefault.Options).ToArray();

            Assert.NotEmpty(simulations);
        }

        [Fact]
        public async Task GetSimulationDataIsOk()
        {
            var response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}/simulations/{_simulationId}/data/{_timeSeriesId}");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            Assert.True(root.GetArrayLength() > 0);

            var firstEntry = root[0];

            Assert.Equal(JsonValueKind.Array, firstEntry.ValueKind);
            Assert.True(firstEntry.GetArrayLength() >= 1);

            var dateTimeString = firstEntry[0].GetString();
            Assert.IsType<DateTime>(DateTime.Parse(dateTimeString));
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/models/scenarios/{_connectionId}",
                Body = new ScenarioDto
                {
                    Id = "MyScenario", 
                    Name = "My Scenario",
                    ModelDataReaderId = $"{_readerId}",
                    ParameterValues = new Dictionary<string, object> { { "foo", 12345 } },
                    InputTimeSeriesValues = new Dictionary<string, string>
                    {
                        { "ts1-in", "MyTimeSeriesId" }
                    }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);
            var id = scenario.Id;
            Assert.Equal($"http://localhost/api/models/scenarios/{_connectionId}/{id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Name, scenario.Name);

            // Update
            request.Body.Name = "Updated name";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);
            Assert.Equal(request.Body.Name, scenario.Name);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateDerivedAndDeleteIsOk()
        {
            // Create derived scenario
            var response = await _client.PostAsync($"api/models/scenarios/{_connectionId}/{_scenarioId}/derived?derivedName=derivedScenario&simulationId={_simulationId}", null);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var scenario = JsonSerializer.Deserialize<Scenario>(json, SerializerOptionsDefault.Options);
            var id = scenario.Id;
            Assert.Equal($"http://localhost/api/models/scenarios/{_connectionId}/{id}", response.Headers.Location.ToString());
            Assert.Equal("derivedScenario", scenario.Name);

            // Delete
            response = await _client.DeleteAsync($"api/models/scenarios/{_connectionId}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"api/models/scenarios/{_connectionId}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}