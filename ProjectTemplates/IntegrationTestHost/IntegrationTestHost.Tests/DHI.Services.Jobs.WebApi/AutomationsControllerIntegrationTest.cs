namespace IntegrationTestHost.Tests
{
    using DHI.Services.Jobs.Automations;
    using DHI.Services.WebApiCore;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class AutomationsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;
        private Automation _automationResponse;

        public AutomationsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full AutomationsController integration suite")]
        public async Task Run_AutomationsController_IntegrationFlow()
        {
            await Step("Get", Get);
            await Step("GetAll", GetAll);
            await Step("GetByGroup", GetByGroup);
            await Step("GetIds", GetIds);
            await Step("GetFullNames", GetFullNames);
            await Step("GetCount", GetCount);
            await Step("Add", Add);
            await Step("Update", Update);
            await Step("Delete", Delete);
            await Step("Disable", Disable);
            await Step("Enable", Enable);
            await Step("GetVersion", GetVersion);
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
            var response = await _client.GetAsync("api/automations/my-group-1|my-automation");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAll()
        {
            var response = await _client.GetAsync("api/automations");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByGroup()
        {
            var response = await _client.GetAsync("api/automations?group=my-group-1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetIds()
        {
            var response = await _client.GetAsync("api/automations/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFullNames()
        {
            var response = await _client.GetAsync("api/automations/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync("api/automations/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Add()
        {
            var body = new Dictionary<string, object>
            {
                 { "$type", "DHI.Services.Jobs.Automations.Automation, DHI.Services.Jobs" },
                 { "taskId", "my-task" },
                 { "group", "my-group-3" },
                 { "name", "my-automation" },
                 { "priority", 10 }
            };

            var response = await _client.PostAsync("api/automations", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task Update()
        {
            var body = new Dictionary<string, object>
            {
                 { "$type", "DHI.Services.Jobs.Automations.Automation, DHI.Services.Jobs" },
                 { "taskId", "my-task" },
                 { "group", "my-group-3" },
                 { "name", "my-automation" },
                 { "priority", -5 }
            };

            var response = await _client.PutAsync("api/automations", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            _automationResponse = JsonSerializer.Deserialize<Automation>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
        }

        private async Task Delete()
        {
            var response = await _client.DeleteAsync($"api/automations/{FullNameString.ToUrl(_automationResponse.FullName)}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task Disable()
        {
            var body = new Dictionary<string, object>
            {
                { "flag", false }
            };

            var response = await _client.PutAsync("api/automations/my-group-1%2Fmy-automation/enable", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Enable()
        {
            var body = new Dictionary<string, object>
            {
                { "flag", true }
            };

            var response = await _client.PutAsync("api/automations/my-group-1%2Fmy-automation/enable", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetVersion()
        {
            var response = await _client.GetAsync("api/automations/version");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
