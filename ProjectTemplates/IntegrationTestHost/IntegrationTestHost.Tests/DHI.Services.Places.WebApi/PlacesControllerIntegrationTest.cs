namespace IntegrationTestHost.Tests
{
    using DHI.Services.WebApiCore;
    using DocumentFormat.OpenXml.Wordprocessing;
    using global::DHI.Services.GIS.Maps;
    using global::DHI.Services.Places;
    using global::DHI.Services.Places.WebApi;
    using global::DHI.Services.WebApiCore;
    using System;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class PlacesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "json";
        private const string FullName = "Stations/MyTestStation";
        private const string EncodedId = "Stations|MyTestStation";
        private const string IndicatorType = "WaterLevel";

        public PlacesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full PlacesController integration suite")]
        public async Task Run_PlacesController_IntegrationFlow()
        {
            await Step("EnsureDoesntExist", async () =>
            {
                var response = await _client.GetAsync($"api/places/{ConnectionId}/{EncodedId}");
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response = await _client.DeleteAsync($"api/places/{ConnectionId}/{EncodedId}");
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }
            });
            await Step("Add", async () => await PostAndAssert($"api/places/{ConnectionId}", new PlaceDTO
            {
                FullName = FullName,
                FeatureId = new FeatureId("Stationer.shp", "StatId", "ID92_M16")
            }));

            await Step("GetById", async () => await AssertSuccess($"api/places/{ConnectionId}/{EncodedId}"));
            await Step("GetAll", async () => await AssertSuccess($"api/places/{ConnectionId}"));
            await Step("GetFeatures", async () => await AssertSuccess($"api/places/{ConnectionId}/features?datetime=2015-11-19"));
            await Step("GetFeaturesWithStatusDateTime", async () => await AssertSuccess($"api/places/{ConnectionId}/features?datetime=2015-11-19&includeIndicatorStatus=true"));
            await Step("GetFeaturesWithStatusFromTo", async () => await AssertSuccess($"api/places/{ConnectionId}/features?from=2015-11-10&to=2015-11-19&includeIndicatorStatus=true"));
            await Step("GetFullNames", async () => await AssertSuccess($"api/places/{ConnectionId}/fullnames"));
            await Step("GetIndicatorsByPlace", async () => await AssertSuccess($"api/places/{ConnectionId}/{EncodedId}/indicators"));
            await Step("GetIndicator", async () => await AssertSuccess($"api/places/{ConnectionId}/Stations|MyStation/indicators/{IndicatorType}"));
            await Step("GetIndicatorsByType", async () => await AssertSuccess($"api/places/{ConnectionId}/indicators/{IndicatorType}"));
            await Step("GetIndicatorStatus", async () => await AssertSuccess($"api/places/{ConnectionId}/Stations|MyStation/indicators/{IndicatorType}/status?datetime=2015-12-01"));
            await Step("GetIndicatorStatusByType", async () => await AssertSuccess($"api/places/{ConnectionId}/indicators/{IndicatorType}/status"));
            await Step("GetThresholdValues", async () => await AssertSuccess($"api/places/{ConnectionId}/Stations|MyStation/thresholds/{IndicatorType}"));
            await Step("GetThresholdValuesByPlace", async () => await AssertSuccess($"api/places/{ConnectionId}/Stations|MyStation/thresholds"));
            await Step("GetPalette", async () => await AssertSuccess($"api/places/{ConnectionId}/Stations|MyStation/indicators/{IndicatorType}/palette?width=100&height=25"));

            await Step("Delete", async () =>
            {
                var response = await _client.DeleteAsync($"api/places/{ConnectionId}/{EncodedId}");
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
            var response = await _client.PostAsync(url, PlacesContentHelper.GetStringContent(body));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _output.WriteLine(content);
                throw new Exception($"Expected success but got {(int)response.StatusCode}: {content}");
            }
        }
    }
}
