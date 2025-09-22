namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Scenarios;
    using Xunit;

    [Collection("Controllers collection")]
    public class ScenariosControllerTest : IDisposable
    {
        private readonly ControllersFixture _factory;
        private readonly HttpClient _client;
        private const string ConnectionId = "json-scenarios";
        private readonly JsonSerializerOptions _options;

        public ScenariosControllerTest(ControllersFixture factory) 
        {
            _factory = factory;
            factory.DeleteFromTempAppDataPath("scenarios.json");
            _client = factory.Client;
            factory.CopyToTempAppDataPath("scenarios.json");
            _options = SerializerOptionsDefault.Options;
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/scenarios/{ConnectionId}/NonExistingScenarioId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/scenarios/{ConnectionId}/NonExistingScenarioId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetInTimeIntervalIsOk()
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
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

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
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

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
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var response = await _client.GetAsync($"api/scenarios/{ConnectionId}?from=2019-07-20T13:00:00&to=2019-07-23");
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfoList = JsonSerializer.Deserialize<ScenarioInfo[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, scenarioInfoList.Length);
            Assert.DoesNotContain(scenarioInfoList, info => info.Id == "scenario1");
        }

        [Fact]
        public async Task GetWithDataSelectorsIsOk()
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

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var response = await _client.GetAsync($"api/scenarios/{ConnectionId}/{request.Body.Id}?dataSelectors=[foo]");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = JsonSerializer.Deserialize<ScenarioInfo>(json, _options);
            Assert.Equal(JsonSerializer.Serialize(new { foo = 1 }), actual.Data);
        }

        [Fact]
        public async Task GetByQueryIsOk()
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
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

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
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

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
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var queryRequest = new
            {
                Url = $"/api/scenarios/{ConnectionId}/query",
                Body = new object[]
                {
                    new {Item = "Data", QueryOperator = "Equal", Value = "SomeSpecialData1"},
                    new {Item = "Id", QueryOperator = "NotEqual", Value = "Scenario13"}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfoList = JsonSerializer.Deserialize<ScenarioInfo[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(scenarioInfoList);
            Assert.Equal("Scenario12", scenarioInfoList[0].Id);
            Assert.DoesNotContain(scenarioInfoList, info => info.Data.Contains("someData"));
        }

        [Fact]
        public async Task GetByQueryNotMetReturnsEmpty()
        {
            var queryRequest = new
            {
                Url = $"/api/scenarios/{ConnectionId}/query",
                Body = new object[]
                {
                    new {Item = "Data", QueryOperator = "Equal", Value = "NoSuchDataExists"},
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfoList = JsonSerializer.Deserialize<ScenarioInfo[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(scenarioInfoList);
        }

        [Fact]
        public async Task GetByQueryWithDataSelectorsIsOk()
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

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var queryRequest = new
            {
                Url = $"/api/scenarios/{ConnectionId}/query?dataSelectors=[foo,bar]",
                Body = new object[]
                {
                    new {Item = "Id", QueryOperator = "Equal", Value = "Scenario11"}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = Assert.Single(JsonSerializer.Deserialize<ScenarioInfo[]>(json, _options));
            Assert.Equal(JsonSerializer.Serialize(new { foo = 1, bar = 2 }), actual.Data);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "NonExistingScenario",
                    Data = "SomeData"
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
                Url = $"/api/scenarios/{ConnectionId}",
                Body = new ScenarioDTO
                {
                    Id = "MyScenario",
                    Data = "SomeData"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }


        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
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

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/scenarios/{ConnectionId}/{request.Body.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, scenarioInfo.Id);

            // Update
            request.Body.Data = "SomeModifiedData";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal($"{request.Body.Data}", scenarioInfo.Data);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{scenarioInfo.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{scenarioInfo.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddAndSoftDeleteIsOk()
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

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/scenarios/{ConnectionId}/{request.Body.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, scenarioInfo.Id);

            // Soft delete
            response = await _client.DeleteAsync($"{request.Url}/{scenarioInfo.Id}?softDelete=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"{request.Url}/{scenarioInfo.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            scenarioInfo = JsonSerializer.Deserialize<ScenarioInfo>(json, _options);
            Assert.NotNull(scenarioInfo.Deleted);
        }

        public void Dispose()
        {
            File.Delete(Path.Combine(_factory.TempAppDataPath, "scenarios.json"));
        }
    }
}