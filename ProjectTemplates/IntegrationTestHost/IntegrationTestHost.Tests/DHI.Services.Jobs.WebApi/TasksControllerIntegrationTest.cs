namespace IntegrationTestHost.Tests
{
    using DHI.Services.Jobs.Scenarios;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;
    using static System.Data.Entity.Infrastructure.Design.Executor;

    public class TasksControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;
        private const string ConnectionId = "wf-tasks";

        public TasksControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full TasksController integration suite")]
        public async Task Run_TasksController_IntegrationFlow()
        {
            await Step("Get", Get);
            await Step("GetAll", GetAll);
            await Step("GetIds", GetIds);
            await Step("GetCount", GetCount);
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
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/WriteToFile");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAll()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetIds()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
