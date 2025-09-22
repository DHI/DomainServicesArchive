namespace IntegrationTestHost.Tests
{
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class RadarImagesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "ascii";
        private const string ZoneId = "TestPoint";
        private static readonly string Date = "2018-03-17T13:00:00";

        public RadarImagesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full RadarImagesController integration suite")]
        public async Task Run_RadarImagesController_IntegrationFlow()
        {
            await Step("Get", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/{Date}"));
            await Step("GetLast", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/last"));
            await Step("GetLastBefore", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/lastbefore/2018-03-17T15:00:00"));
            await Step("GetFirstAfter", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/firstafter/{Date:O}"));
            await Step("PostListLastBefore", async () => await PostAndAssert($"api/radarimages/{ConnectionId}/list/lastbefore", new[] { "2018-03-17T15:00:00", "2018-03-17T14:00:00" }));
            await Step("PostListFirstAfter", async () => await PostAndAssert($"api/radarimages/{ConnectionId}/list/firstafter", new[] { Date }));
            await Step("GetDateTimes", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/datetimes?from={Date:O}"));
            await Step("GetFirstDateTime", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/datetime/first"));
            await Step("GetLastDateTime", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/datetime/last"));
            await Step("PostDateTimesFirstAfter", async () => await PostAndAssert($"api/radarimages/{ConnectionId}/datetimes/firstafter", new[] { Date }));
            await Step("PostDateTimesLastBefore", async () => await PostAndAssert($"api/radarimages/{ConnectionId}/datetimes/lastbefore", new[] { Date }));
            await Step("GetDepthByTimeInterval", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/depth/{ZoneId}?from={Date:O}"));
            await Step("GetDepthByHours", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/depth/{ZoneId}/hours/2"));
            await Step("GetIntensities", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/intensities/{ZoneId}?from={Date:O}"));
            await Step("GetMaxIntensity", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/intensity/max/{ZoneId}?from={Date:O}"));
            await Step("GetAverageIntensity", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/intensity/average/{ZoneId}?from={Date:O}"));
            await Step("GetAverageIntensityByHours", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/intensity/average/{ZoneId}/hours/2"));
            await Step("GetAsBitmap", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/{Date:O}/bitmap"));
            await Step("GetLastAsBitmap", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/last/bitmap"));
            await Step("GetStyleAsBitmap", async () => await AssertSuccess($"api/radarimages/{ConnectionId}/style/IntensityDefault/bitmap"));
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

        private async Task AssertSuccess(string url)
        {
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _output.WriteLine(content);
                throw new Exception($"Expected success but got {(int)response.StatusCode}: {content}");
            }
        }

        private async Task PostAndAssert<T>(string url, T body)
        {
            var response = await _client.PostAsync(url, RastersContentHelper.GetStringContent(body));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _output.WriteLine(content);
                throw new Exception($"Expected success but got {(int)response.StatusCode}: {content}");
            }
        }
    }
}
