namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;
    using System.Linq;

    [Collection("Controllers collection")]
    public class TriggersControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;

        public TriggersControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _serializerOptions = SerializerOptionsDefault.Options;
        }

        [Fact]
        public async Task GetAllTriggers_IsOk()
        {
            var response = await _client.GetAsync("api/automations/triggers");
            var json = await response.Content.ReadAsStringAsync();
            var triggers = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json, _serializerOptions).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(triggers);
        }

        [Fact]
        public async Task GetTriggerById_IsOk()
        {
            const string triggerId = "SqlTrigger";
            var response = await _client.GetAsync($"api/automations/triggers/{triggerId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var trigger = JsonSerializer.Deserialize<JsonElement>(json, _serializerOptions);
            Assert.Equal(triggerId, trigger.GetProperty("id").GetString());
        }

        [Fact]
        public async Task GetNonExistingTrigger_Returns404()
        {
            var response = await _client.GetAsync("api/automations/triggers/NonExistingTriggerId");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTriggerCount_IsOk()
        {
            var response = await _client.GetAsync("api/automations/triggers/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(count > 0);
        }

        [Fact]
        public async Task GetTriggerIds_IsOk()
        {
            var response = await _client.GetAsync("api/automations/triggers/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json, _serializerOptions).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(ids);
        }
    }
}
