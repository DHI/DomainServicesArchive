namespace IntegrationTestHost.Tests
{
    using DHI.Services.Jobs.Scenarios;
    using DHI.Services.Jobs.WebApi;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class ScenariosControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;
        private const string ConnectionId = "json-scenarios";
        private ScenarioInfo _scenarioInfo;

        public ScenariosControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [SkippableFact(DisplayName = "Run full ScenariosController integration suite")]
        public async Task Run_ScenariosController_IntegrationFlow()
        {
            Skip.If(Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true", "Skipped in GitHub Actions environment.");

            await Step("GetInTimeInterval", GetInTimeInterval);
            await Step("GetWithDataSelectors", GetWithDataSelectors);
            await Step("GetByQuery", GetByQuery);
            await Step("GetByQueryMetReturnsEmpty", GetByQueryMetReturnsEmpty);
            await Step("GetByQueryWithDataSelectors", GetByQueryWithDataSelectors);
            await Step("Add", Add);
            await Step("Update", Update);
            await Step("Delete", Delete);
            await Step("AddAndSoftDelete", AddAndSoftDelete);
        }

        private async Task Step(string name, Func<Task> func)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                ResetScenarioJsonFile();
                await func();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task GetInTimeInterval()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario1",
                    Data = "SomeData1",
                    DateTime = new DateTime(2019, 07, 20)
                }
            };
            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario2",
                    Data = "SomeData2",
                    DateTime = new DateTime(2019, 07, 21)
                }
            };
            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario3",
                    Data = "SomeData3",
                    DateTime = new DateTime(2019, 07, 22)
                }
            };
            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            var response = await _client.GetAsync($"api/scenarios/{ConnectionId}?from=2019-07-20T13:00:00&to=2019-07-23");
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfoList = JsonSerializer.Deserialize<ScenarioInfo[]>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetWithDataSelectors()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario1",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 }),
                    DateTime = new DateTime(2019, 07, 20)
                }
            };

            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            var response = await _client.GetAsync($"api/scenarios/{ConnectionId}/{request.Body.Id}?dataSelectors=[foo]");
            var json = await response.Content.ReadAsStringAsync();
            _output.WriteLine(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQuery()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario11",
                    Data = "SomeData11",
                    DateTime = new DateTime(2019, 08, 20)
                }
            };
            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario12",
                    Data = "SomeSpecialData1",
                    DateTime = new DateTime(2019, 08, 21)
                }
            };
            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario13",
                    Data = "SomeSpecialData1",
                    DateTime = new DateTime(2019, 08, 22)
                }
            };
            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));

            var queryRequest = new
            {
                Url = $"/api/scenarios/{ConnectionId}/query",
                Body = new object[]
                {
                    new {Item = "Data", QueryOperator = "Equal", Value = "SomeSpecialData1"},
                    new {Item = "Id", QueryOperator = "NotEqual", Value = "Scenario13"}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, JobsContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfoList = JsonSerializer.Deserialize<ScenarioInfo[]>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQueryMetReturnsEmpty()
        {
            var queryRequest = new
            {
                Url = $"/api/scenarios/{ConnectionId}/query",
                Body = new object[]
                {
                    new {Item = "Data", QueryOperator = "Equal", Value = "NoSuchDataExists"},
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, JobsContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfoList = JsonSerializer.Deserialize<ScenarioInfo[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(scenarioInfoList);
        }

        private async Task GetByQueryWithDataSelectors()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "Scenario11",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 }),
                    DateTime = new DateTime(2019, 08, 20)
                }
            };

            await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));
            var queryRequest = new
            {
                Url = $"/api/scenarios/{ConnectionId}/query?dataSelectors=[foo,bar]",
                Body = new object[]
                {
                    new {Item = "Id", QueryOperator = "Equal", Value = "Scenario11"}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, JobsContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Add()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "MyScenario",
                    Data = "SomeData"
                }
            };

            var response = await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            _scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task Update()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "MyScenario",
                    Data = "SomeModifiedData"
                }
            };

            var response = await _client.PutAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            _scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Delete()
        {
            var response = await _client.DeleteAsync($"api/scenarios/{ConnectionId}/{_scenarioInfo.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task AddAndSoftDelete()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "MyScenario",
                    Data = "SomeData"
                }
            };

            var response = await _client.PostAsync(request.Url, JobsContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await _client.DeleteAsync($"{request.Url}/{scenarioInfo.Id}?softDelete=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"{request.Url}/{scenarioInfo.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private void ResetScenarioJsonFile()
        {
            var webApiAppDataPath = Path.Combine("..", "..", "..", "..", "..", "..", "Build", "BaseWebApi", "App_Data");
            var fullAppDataPath = Path.GetFullPath(webApiAppDataPath);

            var sourceFile = Path.Combine(fullAppDataPath, "scenarios-baseline.json");
            var destFile = Path.Combine(fullAppDataPath, "scenarios.json");

            _output.WriteLine($"webApiAppDataPath: {webApiAppDataPath}");
            _output.WriteLine($"Source file path: {sourceFile}");

            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("The source file 'scenarios-baseline.json' was not found.", sourceFile);
            }

            File.Copy(sourceFile, destFile, overwrite: true);
        }
    }
}
