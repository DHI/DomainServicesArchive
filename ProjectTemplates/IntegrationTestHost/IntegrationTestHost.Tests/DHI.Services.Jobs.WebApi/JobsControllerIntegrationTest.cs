namespace IntegrationTestHost.Tests
{
    using Azure.Core;
    using DHI.Services.Jobs;
    using DHI.Services.Jobs.Automations;
    using DHI.Services.Jobs.WebApi;
    using DHI.Services.Provider.DS;
    using NuGet.Versioning;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class JobsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;
        private const string ConnectionId = "wf-jobs";
        private Job<Guid, string> _job;

        public JobsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full JobsController integration suite")]
        public async Task Run_JobsController_IntegrationFlow()
        {
            await Step("GetCount", GetCount);
            await Step("Get", Get);
            await Step("GetByQuery", GetByQuery);
            await Step("GetLastIsOk", GetLastIsOk);
            await Step("Add", Add);
            await Step("Update", Update);
            await Step("Delete", Delete);
            await Step("AddAndDeleteByQuery", AddAndDeleteByQuery);
            await Step("CancelInProgress", CancelInProgress);
            await Step("AddAndCancel", AddAndCancel);
            await Step("UpdateStatus", UpdateStatus);
            await Step("UpdateHeartbeat", UpdateHeartbeat);
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

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Get()
        {
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}?status=Completed");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQuery()
        {
            var body = new object[]
            {
                new { Item = "Status", QueryOperator = "Equal", Value = "Completed" },
            };

            var response = await _client.PostAsync($"api/jobs/{ConnectionId}/query", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLastIsOk()
        {
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}/last?status=Completed");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Add()
        {
            var body = new JobDTO
            {
                TaskId = "WriteToFile",
                Priority = 1,
            };

            var response = await _client.PostAsync($"api/jobs/{ConnectionId}", JobsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            _job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task Update()
        {
            var body = new JobUpdateDTO(_job.Id, _job.TaskId, _job.Requested, _job.Status)
            {
                Priority = 99,
                Tag = "MyTag"
            };

            var response = await _client.PutAsync($"api/jobs/{ConnectionId}", JobsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            _job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Delete()
        {
            var response = await _client.DeleteAsync($"api/jobs/{ConnectionId}/{_job.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task AddAndDeleteByQuery()
        {
            var body = new JobDTO
            {
                TaskId = "WriteToFile",
                Priority = 1
            };

            var response = await _client.PostAsync($"api/jobs/{ConnectionId}", JobsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await _client.DeleteAsync($"api/jobs/{ConnectionId}?account=john.doe");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task CancelInProgress()
        {
            var response = await _client.GetAsync($"api/jobs/{ConnectionId}?status=InProgress");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonSerializer.Deserialize<IEnumerable<Job<Guid, string>>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options).First();

            response = await _client.PutAsync($"api/jobs/{ConnectionId}/{job.Id}/cancel", null);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{ConnectionId}/{job.Id}");
            json = await response.Content.ReadAsStringAsync();
            job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(JobStatus.Cancel, job.Status);
        }

        private async Task AddAndCancel()
        {
            var body = new JobDTO
            {
                TaskId = "WriteToFile",
                Priority = 1
            };

            var response = await _client.PostAsync($"api/jobs/{ConnectionId}", ContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var cancelRequest = new
            {
                Url = $"api/jobs/{ConnectionId}/cancel",
                Body = new[]
                {
                    job.Id
                }
            };

            response = await _client.PutAsync(cancelRequest.Url, ContentHelper.GetStringContent(cancelRequest.Body));
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{ConnectionId}/{job.Id}");
            json = await response.Content.ReadAsStringAsync();
            job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(JobStatus.Cancel, job.Status);

            response = await _client.DeleteAsync($"api/jobs/{ConnectionId}/{job.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task UpdateStatus()
        {
            var body = new
            {
                StatusMessage = "Completed",
                JobStatus = JobStatus.InProgress,
                Progress = 100
            };

            var response = await _client.PutAsync($"api/jobs/{ConnectionId}/status/111131e3-39f6-40be-b2c3-4aff8e196abc", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{ConnectionId}/111131e3-39f6-40be-b2c3-4aff8e196abc");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(JobStatus.InProgress, job.Status);
        }

        private async Task UpdateHeartbeat()
        {
            var response = await _client.PutAsync($"api/jobs/{ConnectionId}/heartbeat/111131e3-39f6-40be-b2c3-4aff8e196abc", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{ConnectionId}/111131e3-39f6-40be-b2c3-4aff8e196abc");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonSerializer.Deserialize<Job<Guid, string>>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.NotNull(job.Heartbeat);
        }
    }
}
