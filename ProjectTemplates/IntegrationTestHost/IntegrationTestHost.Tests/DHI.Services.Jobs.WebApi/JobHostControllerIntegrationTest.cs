namespace IntegrationTestHost.Tests
{
    using DHI.Services.Jobs;
    using DHI.Services.Jobs.Automations;
    using DHI.Services.Jobs.WebApi;
    using DHI.Services.WebApiCore;
    using DocumentFormat.OpenXml.VariantTypes;
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class JobHostControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;
        private Host _host;

        public JobHostControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full JobHostController integration suite")]
        public async Task Run_JobHostController_IntegrationFlow()
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
            var response = await _client.GetAsync("api/jobhosts/MyGroup2|MyHost3");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAll()
        {
            var response = await _client.GetAsync("api/jobhosts");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByGroup()
        {
            var response = await _client.GetAsync("api/jobhosts?group=MyGroup1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetIds()
        {
            var response = await _client.GetAsync("api/jobhosts/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFullNames()
        {
            var response = await _client.GetAsync("api/jobhosts/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync("api/jobhosts/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Add()
        {
            var body = new HostDTO
            {
                Id = "194.456.456.456",
                Group = "MyGroup3",
                Name = "MyHost",
                Priority = 5,
                RunningJobsLimit = 10
            };

            var response = await _client.PostAsync("api/jobhosts", JobsContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task Update()
        {
            var body = new HostDTO
            {
                Id = "194.456.456.456",
                Group = "MyGroup3",
                Name = "MyHost",
                Priority = -5,
                RunningJobsLimit = 10
            };

            var response = await _client.PutAsync("api/jobhosts", JobsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            _host = JsonSerializer.Deserialize<Host>(json, DHI.Services.Jobs.WebApi.SerializerOptionsDefault.Options);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Delete()
        {
            var response = await _client.DeleteAsync($"api/jobhosts/{FullNameString.ToUrl(_host.FullName)}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
