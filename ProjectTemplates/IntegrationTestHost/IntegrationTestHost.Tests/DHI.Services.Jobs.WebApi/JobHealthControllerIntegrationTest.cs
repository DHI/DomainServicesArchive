namespace IntegrationTestHost.Tests
{
    using DHI.Services;
    using DHI.Services.Jobs;
    using DHI.Services.Jobs.Automations;
    using DHI.Services.Jobs.Workflows;
    using NuGet.Versioning;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class JobHealthControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;
        private const string ConnectionId = "wf-jobs";

        public JobHealthControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full JobHealthController integration suite")]
        public async Task Run_JobHealthController_IntegrationFlow()
        {
            await Step("GapJobsInPastHoursByFieldForTaskId", GapJobsInPastHoursByFieldForTaskId);
            await Step("IncompleteJobsInPastHoursForTaskId", IncompleteJobsInPastHoursForTaskId);
            await Step("ErrorRatioInPastHours", ErrorRatioInPastHours);
            await Step("DelayedJobsInPastHours", DelayedJobsInPastHours);
            await Step("MinimumStartedJobsInPastHours", MinimumStartedJobsInPastHours);
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

        private async Task GapJobsInPastHoursByFieldForTaskId()
        {
            var hours = Math.Ceiling((DateTime.UtcNow - new DateTime(2015, 01, 04, 0, 0, 0, DateTimeKind.Utc)).TotalHours);
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}/health/gap/requested/{hours}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task IncompleteJobsInPastHoursForTaskId()
        {
            var hours = Math.Ceiling((DateTime.UtcNow - new DateTime(2015, 01, 04, 0, 0, 0, DateTimeKind.Utc)).TotalHours);
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}/health/incomplete/{hours}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task ErrorRatioInPastHours()
        {
            var since = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            var hours = Math.Ceiling((DateTime.UtcNow - since).TotalHours);
            var maxErrorRatio = 0;

            var response = await _client.GetAsync($"/api/jobs/{ConnectionId}/health/errorratio/{hours}/{maxErrorRatio}");
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task DelayedJobsInPastHours()
        {
            var since = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            var hours = Math.Ceiling((DateTime.UtcNow - since).TotalHours);
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}/health/delay/{hours}/10");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task MinimumStartedJobsInPastHours()
        {
            var since = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            var hours = Math.Ceiling((DateTime.UtcNow - since).TotalHours);
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}/health/minimumstarted/{hours}/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
