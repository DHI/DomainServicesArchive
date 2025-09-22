namespace IntegrationTestHost.Tests
{
    using DHI.Services.Logging.WebApi;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class LogsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private const string ConnectionId = "json-logger";

        public LogsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _options.Converters.Add(new JsonStringEnumConverter());
            _options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            _output = output;
        }

        [Fact(DisplayName = "Run full LogsController integration suite")]
        public async Task Run_LogsController_IntegrationFlow()
        {
            await Step("AddLogEntry", AddLogEntry);
            await Step("PostQuery", PostQuery);
            await Step("PostQueryWithNoResult", PostQueryWithNoResult);
            await Step("GetByQueryString", GetByQueryString);
            await Step("GetByQueryStringWithNoResult", GetByQueryStringWithNoResult);
            await Step("LastEntry", LastEntry);
            await Step("LastWithNoResult", LastWithNoResult);
        }

        private async Task Step(string name, Func<Task> action)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                await action();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task AddLogEntry()
        {
            var entry = new LogEntryDTO
            {
                LogLevel = "Critical",
                Text = "Integration log entry",
                MachineName = "IntegrationHost",
                Source = "IntegrationTest",
                Tag = "Integration",
                Metadata = new Dictionary<string, object> { { "key", "value" } }
            };

            var response = await _client.PostAsJsonAsync($"/api/logs/{ConnectionId}", entry);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine(content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = JsonSerializer.Deserialize<LogEntryDTO>(content, _options);
            Assert.Equal(entry.Text, result.Text);
        }

        private async Task PostQuery()
        {
            var query = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "Integration" }
            };

            var response = await _client.PostAsJsonAsync($"/api/logs/{ConnectionId}/query", query);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task PostQueryWithNoResult()
        {
            var query = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "NonExistentTag" }
            };

            var response = await _client.PostAsJsonAsync($"/api/logs/{ConnectionId}/query", query);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<LogEntryDTO>>(json, _options);
            Assert.Empty(entries);
        }

        private async Task GetByQueryString()
        {
            var response = await _client.GetAsync($"/api/logs/{ConnectionId}?Tag=Integration");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQueryStringWithNoResult()
        {
            var response = await _client.GetAsync($"/api/logs/{ConnectionId}?Tag=NoMatch");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<LogEntryDTO>>(json, _options);
            Assert.Empty(entries);
        }

        private async Task LastEntry()
        {
            var query = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "Integration" }
            };

            var response = await _client.PostAsJsonAsync($"/api/logs/{ConnectionId}/last", query);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task LastWithNoResult()
        {
            var query = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "DefinitelyMissing" }
            };

            var response = await _client.PostAsJsonAsync($"/api/logs/{ConnectionId}/last", query);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
