namespace IntegrationTestHost.Tests
{
    using global::DHI.Services.Rasters;
    using global::DHI.Services.Rasters.WebApi;
    using global::DHI.Services.Rasters.Zones;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class ZonesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        public ZonesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full ZonesController integration suite")]
        public async Task Run_ZonesController_IntegrationFlow()
        {
            const string testZoneId = "ZoneIntegrationTest";

            await Step("EnsureDoesntExist", async () =>
            {
                var response = await _client.GetAsync($"api/zones/{testZoneId}");
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response = await _client.DeleteAsync($"api/zones/{testZoneId}");
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }
            });

            await Step("Add", async () => await PostAndAssert("api/zones", new ZoneDTO
            {
                Id = testZoneId,
                Name = "Zone Test",
                PixelWeights = new HashSet<PixelWeight>
                {
                    new(new Pixel(1, 2), new Weight(0.6)),
                    new(new Pixel(2, 2), new Weight(0.4))
                },
                Type = ZoneType.LineString.ToString(),
                ImageSize = new System.Drawing.Size(256, 256)
            }));

            await Step("GetById", async () => await AssertSuccess($"api/zones/{testZoneId}"));
            await Step("GetAll", async () => await AssertSuccess("api/zones"));
            await Step("GetCount", async () => await AssertSuccess("api/zones/count"));
            await Step("GetIds", async () => await AssertSuccess("api/zones/ids"));

            await Step("Delete", async () =>
            {
                var response = await _client.DeleteAsync($"api/zones/{testZoneId}");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            });
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
