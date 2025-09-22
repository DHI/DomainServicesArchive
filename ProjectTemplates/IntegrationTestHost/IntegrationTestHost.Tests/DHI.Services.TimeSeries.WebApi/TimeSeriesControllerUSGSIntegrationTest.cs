namespace IntegrationTestHost.Tests
{
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class TimeSeriesControllerUSGSIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string ConnectionId = "timeseries-usgs";
        private const string SeriesId = @"USGS:11458000:00060:00003";
        private DateTime _dateTime;

        public TimeSeriesControllerUSGSIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options;
            _output = output;
        }

        [Fact(DisplayName = "Run full TimeSeriesControllerUSGS integration suite")]
        public async Task Run_TimeSeriesControllerUSGS_IntegrationFlow()
        {
            await Step("Get", Get);
            await Step("GetFirstValue", GetFirstValue);
            await Step("GetFirstValueList", GetFirstValueList);
            await Step("GetLastDateTime", GetLastDateTime);
            await Step("GetLastValueBefore", GetLastValueBefore);
            await Step("GetListByIds", GetListByIds);
            await Step("GetValues", GetValues);
            await Step("GetValuesList", GetValuesList);
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
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFirstValue()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/value/first");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFirstValueList()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionId}/list/value/first", new[] { SeriesId });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLastDateTime()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/datetime/last");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLastValueBefore()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/value/lastbefore/2026-01-01T00:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetListByIds()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionId}/list?fullnames=true", new[] { SeriesId });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetValues()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/values");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetValuesList()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionId}/list/values", new[] { SeriesId });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
