namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Workflows;
    using Xunit;


    [Collection("Controllers collection")]
    public class TasksControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string ConnectionId = "wf-tasks";

        public TasksControllerTest(ControllersFixture factory)
        {
            _client = factory.Client;
            _options = SerializerOptionsDefault.Options;
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/NonExistingTaskId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/WriteToFile");
            var json = await response.Content.ReadAsStringAsync();
            var workflow = JsonSerializer.Deserialize<Workflow>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("WriteToFile", workflow.Id);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var workflows = JsonSerializer.Deserialize<IEnumerable<Workflow>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(workflows, host => host.Id == "WriteToFile");
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("WriteToFile", ids);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/tasks/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, count);
        }
    }
}