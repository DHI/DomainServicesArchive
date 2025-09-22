namespace IntegrationTestHost.Tests
{
    using DHI.Services.TimeSeries.WebApi;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class TimeSeriesControllerMIKE1DIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string ConnectionId = "exam-6-base";
        private const string SeriesId = "Exam6Base.res1d|Node;WaterLevel;1";
        private DateTime _dateTime;

        public TimeSeriesControllerMIKE1DIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options;
            _output = output;
        }

        [Fact(DisplayName = "Run full TimeSeriesControllerMIKE1D integration suite")]
        public async Task Run_TimeSeriesControllerMIKE1D_IntegrationFlow()
        {
            await Step("Get", Get);
            await Step("GetAll", GetAll);
            await Step("GetCount", GetCount);
            await Step("GetFirstValue", GetFirstValue);
            await Step("GetFirstValueList", GetFirstValueList);
            await Step("GetIds", GetIds);
            await Step("GetLastDateTime", GetLastDateTime);
            await Step("GetLastValueBefore", GetLastValueBefore);
            await Step("GetList", GetList);
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

        private async Task GetAll()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/count");
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
        private async Task GetIds()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/ids");
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

        private async Task GetList()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}");
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
