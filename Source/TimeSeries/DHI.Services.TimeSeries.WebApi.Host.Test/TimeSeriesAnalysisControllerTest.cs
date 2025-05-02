namespace DHI.Services.TimeSeries.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    [Collection("Controllers collection")]
    public class TimeSeriesAnalysisControllerTest
    {
        public TimeSeriesAnalysisControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string _connectionId = "csv";

        [Fact]
        public async Task GetMinWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/min");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetMinForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/min");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetMinForNonExistingDateTimesReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/min?from=2099-12-12");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetMinIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/min");
            var json = await response.Content.ReadAsStringAsync();
            var min = JsonSerializer.Deserialize<double>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1.3, min);
        }

        [Fact]
        public async Task GetMinByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/min/period/monthly");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(dataPoints);
            Assert.Equal(new DateTime(2015, 11, 1), DateTime.Parse(dataPoints[0][0].ToString()));
            Assert.Equal(1.3, double.Parse(dataPoints[0][1].ToString(), CultureInfo.InvariantCulture), 3);
        }

        [Fact]
        public async Task GetMinForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/min",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var minValues = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1.3, minValues["timeseries.csv;item1"]);
            Assert.Equal(13.0, minValues["timeseries.csv;item2"]);
        }

        [Fact]
        public async Task GetMaxWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/max");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetMaxForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/max");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetMaxForNonExistingDateTimesReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/max?from=2099-12-12");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetMaxIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/max");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(12, max);
        }

        [Fact]
        public async Task GetMaxByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/max/period/monthly");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(dataPoints);
            Assert.Equal(new DateTime(2015, 11, 1), DateTime.Parse(dataPoints[0][0].ToString()));
            Assert.Equal(12.0, double.Parse(dataPoints[0][1].ToString()), 3);
        }

        [Fact]
        public async Task GetMaxForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/max",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var maxValues = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(12, maxValues["timeseries.csv;item1"]);
            Assert.Equal(120, maxValues["timeseries.csv;item2"]);
        }

        [Fact]
        public async Task GetSumWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/sum");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetSumForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/sum");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetSumForNonExistingDateTimesReturnsZero()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/sum?from=2099-12-12");
            var json = await response.Content.ReadAsStringAsync();
            var sum = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, sum);
        }

        [Fact]
        public async Task GetSumIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/sum");
            var json = await response.Content.ReadAsStringAsync();
            var sum = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(78.3, sum);
        }

        [Fact]
        public async Task GetSumByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/sum/period/monthly");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(dataPoints);
            Assert.Equal(new DateTime(2015, 11, 1), DateTime.Parse(dataPoints[0][0].ToString(), CultureInfo.InvariantCulture));
            Assert.Equal(78.3, double.Parse(dataPoints[0][1].ToString(), CultureInfo.InvariantCulture), 3);
        }

        [Fact]
        public async Task GetSumForManyReturnsNothingIfNonExisting()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/sum",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;NonExistingItem"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var sumValues = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(78.3, sumValues["timeseries.csv;item1"]);
            Assert.DoesNotContain("timeseries.csv;NonExistingItem", sumValues.Keys);
        }

        [Fact]
        public async Task GetSumForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/sum",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var sumValues = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(78.3, sumValues["timeseries.csv;item1"]);
            Assert.Equal(783, sumValues["timeseries.csv;item2"]);
        }

        [Fact]
        public async Task GetAverageWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/average");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetAverageForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/average");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetAverageForNonExistingDateTimesReturnsZero()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/average?from=2099-12-12");
            var json = await response.Content.ReadAsStringAsync();
            var average = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, average);
        }

        [Fact]
        public async Task GetAverageIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/average");
            var json = await response.Content.ReadAsStringAsync();
            var average = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6.525, average, 3);
        }

        [Fact]
        public async Task GetAverageByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/average/period/monthly");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(dataPoints);
            Assert.Equal(new DateTime(2015, 11, 1), DateTime.Parse(dataPoints[0][0].ToString(), CultureInfo.InvariantCulture));
            Assert.Equal(6.525, double.Parse(dataPoints[0][1].ToString(), CultureInfo.InvariantCulture), 3);
        }


        [Fact]
        public async Task GetAverageForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/average",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var averageValues = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(averageValues["timeseries.csv;item1"]);
            Assert.Equal(6.525, double.Parse(averageValues["timeseries.csv;item1"].ToString()), 3);
            Assert.NotNull(averageValues["timeseries.csv;item2"]);
            Assert.Equal(65.25, double.Parse(averageValues["timeseries.csv;item2"].ToString()), 2);
        }

        [Fact]
        public async Task GetMovingAverageWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/movingaverage");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetMovingAverageForNonExistingReturnsEmptyArray()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/movingaverage?window=4");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(dataPoints);
        }

        [Fact]
        public async Task GetMovingAverageIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/movingaverage?window=4");
            var json = await response.Content.ReadAsStringAsync();
            var movingAverage = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(movingAverage[4][0].ToString()));
            Assert.IsType<double>(double.Parse(movingAverage[4][1].ToString()));
        }

        [Fact]
        public async Task GetReducedIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/reduced");
            var json = await response.Content.ReadAsStringAsync();
            var reduced = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(reduced[4][0].ToString()));
            Assert.IsType<double>(double.Parse(reduced[4][1].ToString()));
        }

        [Fact]
        public async Task GetReducedForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/reduced",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var reducedList = JsonSerializer.Deserialize<Dictionary<string, object[][]>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(reducedList["timeseries.csv;item2"][6][0].ToString()));
            Assert.IsType<double>(double.Parse(reducedList["timeseries.csv;item2"][6][1].ToString()));
        }

        [Fact]
        public async Task GetSmoothedIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/smoothed?order=3");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var smoothed = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.IsType<DateTime>(DateTime.Parse(smoothed[4][0].ToString()));
            Assert.IsType<double>(double.Parse(smoothed[4][1].ToString()));
        }

        [Fact]
        public async Task GetSmoothedForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/smoothed?window=5",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var smoothedList = JsonSerializer.Deserialize<Dictionary<string, object[][]>>(json, _options);

            Assert.IsType<DateTime>(DateTime.Parse(smoothedList["timeseries.csv;item2"][6][0].ToString()));
            Assert.IsType<double>(double.Parse(smoothedList["timeseries.csv;item2"][6][1].ToString()));
        }

        [Fact]
        public async Task GetPercentileForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/percentile/90");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetPercentileForNonExistingDateTimesReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/percentile/90?from=2099-12-12");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetPercentileIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/percentile/90");
            var json = await response.Content.ReadAsStringAsync();
            var percentile = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(11.0, percentile);
        }

        [Fact]
        public async Task GetPercentileForManyReturnsNullValueIfNonExisting()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/percentile/90",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;NonExistingItem"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var values = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(11.0, double.Parse(values["timeseries.csv;item1"].ToString()));
            Assert.Null(values["timeseries.csv;NonExistingItem"]);
        }

        [Fact]
        public async Task GetPercentileForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/percentile/90",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var values = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(11.0, double.Parse(values["timeseries.csv;item1"].ToString()));
            Assert.Equal(110.0, double.Parse(values["timeseries.csv;item2"].ToString()));
        }

        [Fact]
        public async Task GetResampledAverageForNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;NonExistingItem/resampled?timespan=00:30:00");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetResampledIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled?timespan=00:30:00");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints[4][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints[4][1].ToString()));
        }

        [Fact]
        public async Task GetMaxForResampledIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/max?timespan=00:30:00");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(12, max);
        }

        [Fact]
        public async Task GetMinForResampledIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/min?timespan=00:30:00");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1.3, max);
        }

        [Fact]
        public async Task GetAverageForResampledIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/average?timespan=00:30:00");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6.51389413988658, max, 3);
        }

        [Fact]
        public async Task GetStandardDeviationForResampledIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/standarddeviation?timespan=00:30:00");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3.16216365491417, max, 3);
        }

        [Fact]
        public async Task GetTrendlineForResampledIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/lineartrendline?timespan=00:30:00");
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var output = doc.RootElement;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            if (output.TryGetProperty("slope", out var slope))
            {
                Assert.Equal(0.9928980211214391, slope.GetDouble(), 3);
            }

            if (output.TryGetProperty("offset", out var offset))
            {
                Assert.Equal(1.0529550237186633, offset.GetDouble(), 3);
            }

            if (output.TryGetProperty("trendline", out var trendline))
            {
                var firstPoint = trendline.EnumerateArray().First();
                var lastPoint = trendline.EnumerateArray().Last();
                Assert.Equal(1.0529550237186633, firstPoint[1].GetDouble());
                Assert.Equal(11.974833256054493, lastPoint[1].GetDouble());
            }
        }

        [Fact]
        public async Task GetResampledByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/period/Hourly");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints[4][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints[4][1].ToString()));
        }

        [Fact]
        public async Task GetMaxForResampledByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/period/Hourly/max");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(11.964, max, 3);
        }

        [Fact]
        public async Task GetMinForResampledByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/period/Hourly/min");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1.304, max, 3);
        }

        [Fact]
        public async Task GetAverageForResampledByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/period/Hourly/average");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6.498, max, 3);
        }

        [Fact]
        public async Task GetStandardDeviationForResampledByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/period/Hourly/standarddeviation");
            var json = await response.Content.ReadAsStringAsync();
            var max = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3.159, max, 3);
        }

        [Fact]
        public async Task GetTrendlineForResampledByPeriodIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/resampled/period/Hourly/lineartrendline");
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var output = doc.RootElement;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            if (output.TryGetProperty("slope", out var slope))
            {
                Assert.Equal(0.992801596566861, slope.GetDouble(), 3);

            }

            if (output.TryGetProperty("offset", out var offset))
            {
                Assert.Equal(1.059, offset.GetDouble(), 3);
            }

            if (output.TryGetProperty("trendline", out var trendline))
            {
                var firstPoint = trendline.EnumerateArray().First();
                var lastPoint = trendline.EnumerateArray().Last();
                Assert.Equal(1.0587008270599025, firstPoint[1].GetDouble());
                Assert.Equal(11.938151656105088, lastPoint[1].GetDouble());
            }
        }

        [Fact]
        public async Task GetDurationCurveIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries3.csv;item1/durationcurve?durationInHours=24&numberOfIntervals=50");
            var json = await response.Content.ReadAsStringAsync();
            var durationCurve = JsonSerializer.Deserialize<Dictionary<double, double?>>(json);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(49, durationCurve.Count);

        }

        [Fact]
        public async Task GetExtremeDurationCurveIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries3.csv;item1/durationcurve/extreme");
            var json = await response.Content.ReadAsStringAsync();
            var durationCurve = JsonSerializer.Deserialize<Dictionary<double, double?>>(json);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(33, durationCurve.Count);
            Assert.Equal(12, durationCurve[0]);
            Assert.Equal(0, durationCurve[0.9999]);

        }
        [Fact]
        public async Task GetStandardDeviationIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/standarddeviation");
            var json = await response.Content.ReadAsStringAsync();
            var stdev = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3.565, stdev, 3);
        }

        [Fact]
        public async Task GetStandardDeviationByPeriodIsOk()
        {
            var responseHourly = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/standarddeviation/period/hourly");
            var jsonHourly = await responseHourly.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(jsonHourly, _options);
            Assert.Equal(HttpStatusCode.OK, responseHourly.StatusCode);
            Assert.Equal(new DateTime(2015, 11, 9, 10, 0, 0), DateTime.Parse(dataPoints[0][0].ToString(), CultureInfo.InvariantCulture));
            Assert.Equal("NaN", dataPoints[0][1].ToString());

            var responseMonthly = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/standarddeviation/period/monthly");
            var jsonMonthly = await responseMonthly.Content.ReadAsStringAsync();
            dataPoints = JsonSerializer.Deserialize<object[][]>(jsonMonthly, _options);
            Assert.Equal(HttpStatusCode.OK, responseMonthly.StatusCode);
            Assert.Single(dataPoints);
            Assert.Equal(new DateTime(2015, 11, 1), DateTime.Parse(dataPoints[0][0].ToString(), CultureInfo.InvariantCulture));
            Assert.Equal(3.565, double.Parse(dataPoints[0][1].ToString(), CultureInfo.InvariantCulture), 3);
        }

        [Fact]
        public async Task GetStandardDeviationForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{_connectionId}/list/standarddeviation",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var averageValues = JsonSerializer.Deserialize<Dictionary<string, double?>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(averageValues["timeseries.csv;item1"]);
            Assert.Equal(3.565, (double)averageValues["timeseries.csv;item1"], 3);
            Assert.NotNull(averageValues["timeseries.csv;item2"]);
            Assert.Equal(35.65, (double)averageValues["timeseries.csv;item2"], 2);
        }

        [Fact]
        public async Task GetTrendlineIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{_connectionId}/timeseries.csv;item1/lineartrendline");
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var output = doc.RootElement;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            if (output.TryGetProperty("slope", out var slope))
            {
                Assert.Equal(0.98846153846153839, slope.GetDouble());
            }

            if (output.TryGetProperty("offset", out var offset))
            {
                Assert.Equal(1.0884615384615384, offset.GetDouble());
            }

            if (output.TryGetProperty("trendline", out var trendline))
            {
                var firstPoint = trendline.EnumerateArray().First();
                var lastPoint = trendline.EnumerateArray().Last();
                Assert.Equal(1.0884615384615384, firstPoint[1].GetDouble());
                Assert.Equal(11.961538461538462, lastPoint[1].GetDouble());
            }
        }
    }
}
