namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class TimeStepsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private const string ConnectionId = "dfs2";
        private const string ItemId = "H Water Depth m";
        private static readonly DateTime SampleDate = new(2014, 10, 1, 12, 0, 0);

        public TimeStepsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full TimeStepsController integration suite")]
        public async Task Run_TimeStepsController_IntegrationFlow()
        {
            await Step("GetData", GetData);
            await Step("GetListByIds", GetListByIds);
            await Step("GetDateTimes", GetDateTimes);
            await Step("GetFirstDateTime", GetFirstDateTime);
            await Step("GetLastDateTime", GetLastDateTime);
            await Step("GetFirstAfter", GetFirstAfter);
            await Step("GetLastBefore", GetLastBefore);
            await Step("GetLast", GetLast);
            await Step("GetItems", GetItems);
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

        private async Task GetData()
        {
            var url = $"/api/timesteps/{ConnectionId}/{Uri.EscapeDataString(ItemId)}/data/{SampleDate:yyyy-MM-ddTHH:mm:ss}";
            var response = await _client.GetAsync(url);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetListByIds()
        {
            var url = $"/api/timesteps/{ConnectionId}/list";
            var body = new Dictionary<string, IEnumerable<DateTime>>
            {
                { ItemId, new[] { SampleDate, SampleDate.AddMinutes(30) } }
            };
            var response = await _client.PostAsJsonAsync(url, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetDateTimes()
        {
            var response = await _client.GetAsync($"/api/timesteps/{ConnectionId}/datetimes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFirstDateTime()
        {
            var response = await _client.GetAsync($"/api/timesteps/{ConnectionId}/datetime/first");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLastDateTime()
        {
            var response = await _client.GetAsync($"/api/timesteps/{ConnectionId}/datetime/last");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFirstAfter()
        {
            var url = $"/api/timesteps/{ConnectionId}/{Uri.EscapeDataString(ItemId)}/data/firstafter/{SampleDate:yyyy-MM-ddTHH:mm:ss}";
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLastBefore()
        {
            var url = $"/api/timesteps/{ConnectionId}/{Uri.EscapeDataString(ItemId)}/data/lastbefore/{SampleDate:yyyy-MM-ddTHH:mm:ss}";
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLast()
        {
            var url = $"/api/timesteps/{ConnectionId}/{Uri.EscapeDataString(ItemId)}/data/last";
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetItems()
        {
            var response = await _client.GetAsync($"/api/timesteps/{ConnectionId}/items");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
