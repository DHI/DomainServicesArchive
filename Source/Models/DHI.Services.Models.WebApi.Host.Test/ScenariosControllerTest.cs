namespace DHI.Services.Models.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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
            var response = await client.GetAsync($"api/scenarios/{_connectionId}/{_scenarioId}");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetWithNonExistingConnectionIdReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/scenarios/NonExistingConnection/{_scenarioId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/NonExistingScenario");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetSimulationsForNonExistingScenarioReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/NonExistingScenario/simulations");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("'NonExistingScenario' was not found.", json);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingScenarioReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/NonExistingScenario/simulations/{_simulationId}/data/{_timeSeriesId}");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("'NonExistingScenario' was not found.", json);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingSimulationReturns404NotFound()
        {
            var simulationId = Guid.NewGuid();
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/{_scenarioId}/simulations/{simulationId}/data/{_timeSeriesId}");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains($"'{simulationId}' was not found.", json);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingDataReturns400BadRequest()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/{_scenarioId}/simulations/{_simulationId}/data/NonExistingData");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("'NonExistingData' is not a valid output time series", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/scenarios/{_connectionId}/NonExistingScenario");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("'NonExistingScenario' was not found.", json);
        }

        [Fact]
        public async Task CreateDerivedForNonExistingScenarioReturns404NotFound()
        {
            var response = await _client.PostAsync($"api/scenarios/{_connectionId}/NonExistingScenario/derived?derivedName=derivedScenario&simulationId={_simulationId}", null);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("'NonExistingScenario' was not found.", json);
        }

        [Fact]
        public async Task CreateDerivedForNonExistingSimulationReturns404NotFound()
        {
            var simulationId = Guid.NewGuid();
            var response = await _client.PostAsync($"api/scenarios/{_connectionId}/{_scenarioId}/derived?derivedName=derivedScenario&simulationId={simulationId}", null);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains($"'{simulationId}' was not found.", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/{_scenarioId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var scenario = JsonConvert.DeserializeObject<Scenario>(json);

            Assert.Contains("foo", scenario.ParameterValues);
            Assert.IsType<long>(scenario.ParameterValues["foo"]);
            Assert.Contains("ts1-in", scenario.InputTimeSeriesValues);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var scenarios = JsonConvert.DeserializeObject<IEnumerable<Scenario>>(json).ToArray();
            
            Assert.NotEmpty(scenarios);
            Assert.Contains("fakeReader", scenarios.Select(s => s.ModelDataReaderId));
        }

        [Fact]
        public async Task GetSimulationsIsOk()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/{_scenarioId}/simulations");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var simulations = JsonConvert.DeserializeObject<IEnumerable<Simulation>>(json).ToArray();

            Assert.NotEmpty(simulations);
        }

        [Fact]
        public async Task GetSimulationDataIsOk()
        {
            var response = await _client.GetAsync($"api/scenarios/{_connectionId}/{_scenarioId}/simulations/{_simulationId}/data/{_timeSeriesId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var o = JsonConvert.DeserializeObject(json);
            Assert.IsType<JArray>(o);
            Assert.True(((JArray)o).HasValues);
            var dateTimeString = ((JArray)o)[0].First.ToString();
            Assert.IsType<DateTime>(DateTime.Parse(dateTimeString));
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/scenarios/{_connectionId}",
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
            var scenario = JsonConvert.DeserializeObject<Scenario>(json);
            var id = scenario.Id;
            Assert.Equal($"http://localhost/api/scenarios/{_connectionId}/{id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Name, scenario.Name);

            // Update
            request.Body.Name = "Updated name";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            scenario = JsonConvert.DeserializeObject<Scenario>(json);
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
            var response = await _client.PostAsync($"api/scenarios/{_connectionId}/{_scenarioId}/derived?derivedName=derivedScenario&simulationId={_simulationId}", null);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var scenario = JsonConvert.DeserializeObject<Scenario>(json);
            var id = scenario.Id;
            Assert.Equal($"http://localhost/api/scenarios/{_connectionId}/{id}", response.Headers.Location.ToString());
            Assert.Equal("derivedScenario", scenario.Name);

            // Delete
            response = await _client.DeleteAsync($"api/scenarios/{_connectionId}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"api/scenarios/{_connectionId}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}