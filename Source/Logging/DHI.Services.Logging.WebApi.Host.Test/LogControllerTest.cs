namespace DHI.Services.Logging.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Logging;
    using Xunit;

    [Collection("Controllers collection")]
    public class LogControllerTest
    {
        private readonly JsonSerializerOptions _options = new();

        public LogControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _options.Converters.Add(new JsonStringEnumConverter());
            _options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        private readonly HttpClient _client;

        [Theory]
        [InlineData("Equals")]
        [InlineData("=")]
        [InlineData(">")]
        public async Task IllegalQueryOperatorReturns400BadRequest(string queryOperator)
        {
            var request = new
            {
                Url = "api/logs/json-logger/query",
                Body = new List<object>
                {
                    new { Item = "LogLevel", QueryOperator = queryOperator, Value = "Error" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Could not parse query operator", json);
        }

        [Fact]
        public async Task PostQueryIsOk()
        {
            var request = new
            {
                Url = "api/logs/json-logger/query",
                Body = new List<object>
                {
                    new { Item = "Tag", QueryOperator = "Equal", Value = "log" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<LogEntryDTO>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(13, entries.Count());
        }

        [Fact]
        public async Task PostQueryWithNoResultReturnsEmptyList()
        {
            var request = new
            {
                Url = "api/logs/json-logger/query",
                Body = new List<object>
                {
                    new { Item = "Tag", QueryOperator = "Equal", Value = "NonExistingSource" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<LogEntryDTO>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(entries);
        }

        [Fact]
        public async Task GetByQueryStringIsOk()
        {
            var response = await _client.GetAsync("api/logs/json-logger?Tag=log");
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<LogEntryDTO>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(13, entries.Count());
        }

        [Fact]
        public async Task GetByQueryStringWithNoResultReturnsEmptyList()
        {
            var response = await _client.GetAsync("api/logs/json-logger?Tag=NonExistingSource");
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<LogEntryDTO>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(entries);
        }

        [Fact]
        public async Task LastIsOk()
        {
            var request = new
            {
                Url = "api/logs/json-logger/last",
                Body = new List<object>
                {
                    new { Item = "Tag", QueryOperator = "Equal", Value = "log" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var entry = JsonSerializer.Deserialize<LogEntryDTO>(json, _options);
            Assert.Equal("An error has occurred in MIKE 11", entry.Text);
        }

        [Fact]
        public async Task LastWithNoQueryResultReturns404NotFound()
        {
            var request = new
            {
                Url = "api/logs/json-logger/last",
                Body = new List<object>
                {
                    new { Item = "Tag", QueryOperator = "Equal", Value = "NotFound" }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("No log entry found", json);
        }

        [Fact]
        public async Task AddLogEntryIsOk()
        {
            var request = new
            {
                Url = "api/logs/json-logger",
                Body = new LogEntryDTO
                {
                    LogLevel = "Critical",
                    Text = "An error has occurred in MIKE 11",
                    MachineName = "Server-Alpha",
                    Source = "Beta",
                    Tag = "Followup",
                    Metadata = new Dictionary<string, object>
                    {
                        { "handler", "temporary" },
                        { "format", "main" }
                    }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var logEntry = JsonSerializer.Deserialize<LogEntryDTO>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/logs/json-logger", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Text, logEntry.Text);
        }
    }
}