namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class TriggerControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        public TriggerControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full TriggersController integration suite")]
        public async Task Run_TriggersController_IntegrationFlow()
        {
            await Step("GetAllTriggers", GetAllTriggers);
            await Step("GetTriggerById", GetTriggerById);
            await Step("GetTriggerCount", GetTriggerCount);
            await Step("GetTriggerIds", GetTriggerIds);
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

        private async Task GetAllTriggers()
        {
            var response = await _client.GetAsync("api/automations/triggers");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTriggerById()
        {
            const string triggerId = "SqlTrigger";
            var response = await _client.GetAsync($"api/automations/triggers/{triggerId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTriggerCount()
        {
            var response = await _client.GetAsync("api/automations/triggers/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTriggerIds()
        {
            var response = await _client.GetAsync("api/automations/triggers/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
