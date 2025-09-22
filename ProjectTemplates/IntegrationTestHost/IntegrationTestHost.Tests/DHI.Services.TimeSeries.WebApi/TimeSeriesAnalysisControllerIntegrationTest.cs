namespace IntegrationTestHost.Tests
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class TimeSeriesAnalysisControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string ConnectionIdCsv = "grouped-csv";
        private const string SeriesId = "timeseries.csv;item1";
        private const string SeriesId3 = "timeseries3.csv;item1";

        public TimeSeriesAnalysisControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options;
            _output = output;
        }

        [Fact(DisplayName = "Run full TimeSeriesAnalysisController integration suite")]
        public async Task Run_TimeSeriesController_IntegrationFlow()
        {
            await Step("GetMin", GetMin);
            await Step("GetMinByPeriod", GetMinByPeriod);
            await Step("GetMinList", GetMinList);
            await Step("GetMax", GetMax);
            await Step("GetMaxByPeriod", GetMaxByPeriod);
            await Step("GetMaxList", GetMaxList);
            await Step("GetSum", GetSum);
            await Step("GetSumByPeriod", GetSumByPeriod);
            await Step("GetSumList", GetSumList);
            await Step("GetAverage", GetAverage);
            await Step("GetAverageByPeriod", GetAverageByPeriod);
            await Step("GetAverageList", GetAverageList);
            await Step("GetMovingAverage", GetMovingAverage);
            await Step("GetReduced", GetReduced);
            await Step("GetReducedList", GetReducedList);
            await Step("GetSmoothed", GetSmoothed);
            await Step("GetSmoothedList", GetSmoothedList);
            await Step("GetPercentile", GetPercentile);
            await Step("GetPercentileList", GetPercentileList);
            await Step("GetResampled", GetResampled);
            await Step("GetMaxForResampled", GetMaxForResampled);
            await Step("GetMinForResampled", GetMinForResampled);
            await Step("GetAverageForResampled", GetAverageForResampled);
            await Step("GetStandardDeviationForResampled", GetStandardDeviationForResampled);
            await Step("GetTrendLineForResampled", GetTrendLineForResampled);
            await Step("GetResampledByPeriod", GetResampledByPeriod);
            await Step("GetMaxForResampledByPeriod", GetMaxForResampledByPeriod);
            await Step("GetMinForResampledByPeriod", GetMinForResampledByPeriod);
            await Step("GetAverageForResampledByPeriod", GetAverageForResampledByPeriod);
            await Step("GetStandardDeviationForResampledByPeriod", GetStandardDeviationForResampledByPeriod);
            await Step("GetTrendLineForResampledByPeriod", GetTrendLineForResampledByPeriod);
            await Step("GetDurationCurve", GetDurationCurve);
            await Step("GetExtremeDurationCurve", GetExtremeDurationCurve);
            await Step("GetStandardDeviation", GetStandardDeviation);
            await Step("GetStandardDeviationByPeriod", GetStandardDeviationByPeriod);
            await Step("GetStandardDeviationForMany", GetStandardDeviationForMany);
            await Step("GetTrendline", GetTrendline);
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

        private async Task GetMin()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/min");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMinByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/min/period/monthly");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMinList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/min", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMax()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/max");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMaxByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/max/period/monthly");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMaxList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/max", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSum()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/sum");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSumByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/sum/period/monthly");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSumList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/sum", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAverage()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/average");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAverageByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/average/period/monthly");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAverageList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/average", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMovingAverage()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/movingaverage?window=4");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetReduced()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/reduced");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetReducedList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/reduced", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSmoothed()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/smoothed?order=3");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSmoothedList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/smoothed?window=5", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetPercentile()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/percentile/90");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetPercentileList()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/percentile/90", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetResampled()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMaxForResampled()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/max?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMinForResampled()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/min?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAverageForResampled()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/average?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetStandardDeviationForResampled()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/standarddeviation?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTrendLineForResampled()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/lineartrendline?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetResampledByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/period/Hourly");
            Assert.Equal(HttpStatusCode.OK, response?.StatusCode);
        }

        private async Task GetMaxForResampledByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/period/Hourly/max");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMinForResampledByPeriod()
        {
            var response = await _client.GetAsync($"/api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/period/Hourly/min");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAverageForResampledByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/period/Hourly/average");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetStandardDeviationForResampledByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/period/Hourly/standarddeviation");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTrendLineForResampledByPeriod()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/resampled/period/Hourly/lineartrendline");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetDurationCurve()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId3}/durationcurve?durationInHours=24&numberOfIntervals=50");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetExtremeDurationCurve()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId3}/durationcurve/extreme");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetStandardDeviation()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/standarddeviation");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetStandardDeviationByPeriod()
        {
            var responseHourly = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/standarddeviation/period/hourly");
            Assert.Equal(HttpStatusCode.OK, responseHourly.StatusCode);

            var responseMonthly = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/standarddeviation/period/monthly");
            Assert.Equal(HttpStatusCode.OK, responseMonthly.StatusCode);
        }

        private async Task GetStandardDeviationForMany()
        {
            var body = new[]
            {
                "timeseries.csv;item1",
                "timeseries.csv;item2"
            };

            var response = await _client.PostAsync($"api/timeseries/{ConnectionIdCsv}/list/standarddeviation", TimeSeriesContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTrendline()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdCsv}/{SeriesId}/lineartrendline");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
