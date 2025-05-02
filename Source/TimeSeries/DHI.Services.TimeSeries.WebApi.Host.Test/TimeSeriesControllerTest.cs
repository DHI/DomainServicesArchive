namespace DHI.Services.TimeSeries.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    [Collection("Controllers collection")]
    public class TimeSeriesControllerTest
    {
        public TimeSeriesControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string ConnectionId = "csv";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByNonExistingGroupReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}?group=NonExistingGroup");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetListByNonExistingGroupReturns404NotFound()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list",
                Body = new[]
                {
                    "mysubfolder",
                    "NonExistingGroup"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1");
            var json = await response.Content.ReadAsStringAsync();
            var timeSeries = JsonSerializer.Deserialize<TimeSeries<string, double>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("timeseries.csv;item1", timeSeries.Id);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(10, count);
        }

        [Fact]
        public async Task GetDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/datetimes");
            var json = await response.Content.ReadAsStringAsync();
            var dateTimes = JsonSerializer.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(12, dateTimes.Length);
            Assert.IsType<DateTime>(DateTime.Parse(dateTimes[0]));
        }

        [Fact]
        public async Task GetDateTimesForNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem/datetimes");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetFirstDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/datetime/first");
            var json = await response.Content.ReadAsStringAsync();
            var dateTime = JsonSerializer.Deserialize<string>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dateTime));
        }

        [Fact]
        public async Task GetFirstDateTimesForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem/datetime/first");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetFirstValueIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/value/first");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoint = JsonSerializer.Deserialize<object[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoint[0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoint[1].ToString()));
        }

        [Fact]
        public async Task GetFirstValueForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem/value/first");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetFirstValueForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/value/first",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<Dictionary<string, object[]>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints["timeseries.csv;item1"][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints["timeseries.csv;item1"][1].ToString()));
        }

        [Fact]
        public async Task GetFirstValueForManyReturnsEmptyDictionaryIfNonExisting()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/value/first",
                Body = new[]
                {
                    "NonExistingTimeSeries.csv;item1",
                    "NonExistingTimeSeries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<Dictionary<string, object[]>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(dataPoints);
        }

        [Fact]
        public async Task GetFirstValueAfterIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/value/firstafter/2015-11-14T10:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoint = JsonSerializer.Deserialize<object[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoint[0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoint[1].ToString()));
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonSerializer.Deserialize<string[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("timeseries.csv;item1", fullnames);
            Assert.Equal(10, fullnames.Length);
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<string[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("timeseries.csv;item1", ids);
            Assert.Contains("mysubfolder/timeseries4.csv;item1", ids);
            Assert.Equal(10, ids.Length);
        }

        [Fact]
        public async Task GetLastDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/datetime/last");
            var json = await response.Content.ReadAsStringAsync();
            var dateTime = JsonSerializer.Deserialize<string>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dateTime));
        }

        [Fact]
        public async Task GetLastDateTimesForNonExistingReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem/datetime/last");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetLastValueIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/value/last");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoint = JsonSerializer.Deserialize<object[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoint[0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoint[1].ToString()));
        }

        [Fact]
        public async Task GetLastValueForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/value/last",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<Dictionary<string, object[]>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints["timeseries.csv;item1"][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints["timeseries.csv;item1"][1].ToString()));
        }

        [Fact]
        public async Task GetLastValueBeforeIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/value/lastbefore/2015-11-14T10:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoint = JsonSerializer.Deserialize<object[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoint[0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoint[1].ToString()));
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesList = JsonSerializer.Deserialize<TimeSeries<string, double>[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(10, timeSeriesList.Count());
        }

        [Fact]
        public async Task GetByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}?group=mysubfolder");
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesList = JsonSerializer.Deserialize<TimeSeries<string, double>[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, timeSeriesList.Length);
            Assert.Contains("mysubfolder/timeseries4.csv;item1", timeSeriesList.Select(ts => ts.Id));
        }

        [Fact]
        public async Task GetListByGroupsIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list",
                Body = new[]
                {
                    "mysubfolder"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesList = JsonSerializer.Deserialize<List<TimeSeries<string, double>>>(json, _options);

            Assert.Equal(4, timeSeriesList.Count);
            Assert.Contains("mysubfolder/timeseries4.csv;item1", timeSeriesList.Select(ts => ts.Id));
            Assert.Contains("mysubfolder/timeseries4.csv;item2", timeSeriesList.Select(ts => ts.Id));
        }

        [Fact]
        public async Task GetListByFullNamesIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list?fullnames=true",
                Body = new[]
                {
                    "timeseries2.csv;item1",
                    "timeseries3.csv;item2",
                    "mysubfolder/timeseries4.csv;item1",
                    "timeseries3.csv;non-existing-item"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesList = JsonSerializer.Deserialize<List<TimeSeries<string, double>>>(json, _options);

            Assert.Equal(3, timeSeriesList.Count);
            Assert.Contains("mysubfolder/timeseries4.csv;item1", timeSeriesList.Select(ts => ts.Id));
            Assert.Contains("timeseries3.csv;item2", timeSeriesList.Select(ts => ts.Id));
            Assert.DoesNotContain("NonExistingId", timeSeriesList.Select(ts => ts.Id));
        }

        [Fact]
        public async Task GetValueIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/value/2015-11-14T10:52:31");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoint = JsonSerializer.Deserialize<object[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6.0, double.Parse(dataPoint[1].ToString()));
        }

        [Fact]
        public async Task GetValueForNonExistingIdReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem/value/2015-11-14T10:52:31");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetValueForNonExistingDateTimeReturns204NoContent()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/value/2099-12-12");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        [Fact]
        public async Task GetAllValuesIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/values");
            var json = await response.Content.ReadAsStringAsync();
            var serializer = new JsonSerializerOptions();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(12, dataPoints.Length);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints[11][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints[11][1].ToString()));
        }

        [Fact]
        public async Task GetAllValuesForNonExistingIdReturnsEmptyArray()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;NonExistingItem/values");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(dataPoints);
        }

        [Fact]
        public async Task GetValuesFromIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/values?from=2015-11-14T10:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(7, dataPoints.Length);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints[6][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints[6][1].ToString()));
        }

        [Fact]
        public async Task GetValuesFromForNonExistingDateTimesReturnsEmptyArray()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/values?from=2099-11-14");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(dataPoints);
        }

        [Fact]
        public async Task GetValuesToIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/values?to=2015-11-14T10:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(5, dataPoints.Length);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints[4][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints[4][1].ToString()));
        }

        [Fact]
        public async Task GetValuesFromToIsOk()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/timeseries.csv;item1/values?from=2015-11-12&to=2015-11-14T10:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, dataPoints.Length);
            Assert.IsType<DateTime>(DateTime.Parse(dataPoints[1][0].ToString()));
            Assert.IsType<double>(double.Parse(dataPoints[1][1].ToString()));
        }

        [Fact]
        public async Task GetValuesForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/values?from=2015-11-14T10:00:00",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var values = JsonSerializer.Deserialize<Dictionary<string, object[][]>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, values.Count);
            Assert.Equal(7, values["timeseries.csv;item2"].Length);
            Assert.IsType<DateTime>(DateTime.Parse(values["timeseries.csv;item2"][6][0].ToString()));
            Assert.IsType<double>(double.Parse(values["timeseries.csv;item2"][6][1].ToString()));
        }

        [Fact]
        public async Task GetValuesForManyWithDistinctTimeStepsIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/values?from=2015-11-14T10:00:00&distinctdatetime=true",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var values = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(7, values.Length);
            Assert.True(values.All(x => x.Length == 3));
            Assert.Equal(DateTime.Parse("2015-11-19T10:52:31"), DateTime.Parse(values[5][0].ToString()));
            Assert.Equal(11d, double.Parse(values[5][1].ToString()));
            Assert.Equal(110d, double.Parse(values[5][2].ToString()));
        }

        [Fact]
        public async Task GetValuesForManyWithDifferentDistinctTimeStepsIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/values?from=2015-11-14T10:00:00&distinctdatetime=true",
                Body = new[]
                {
                    "timeseries.csv;item1",
                    "timeseries2.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var values = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(14, values.Length);
            Assert.True(values.All(x => x.Length == 3));
            Assert.Equal(DateTime.Parse("2015-11-19T10:52:31"), DateTime.Parse(values[11][0].ToString()));
            Assert.Equal(11d, double.Parse(values[11][1].ToString()));
            Assert.Null(values[11][2]);
        }

        [Fact]
        public async Task GetVectorsIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/vectors?from=2015-11-14T10:00:00",
                Body = new ComponentsDTO
                {
                    X = "timeseries.csv;item1",
                    Y = "timeseries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var vectors = JsonSerializer.Deserialize<object[][]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(7, vectors.Length);
            Assert.IsType<DateTime>(DateTime.Parse(vectors[6][0].ToString()));
            var vector = JsonSerializer.Deserialize<Vector<double>>(vectors[6][1].ToString());
            Assert.IsType<Vector<double>>(vector);
        }

        [Fact]
        public async Task GetVectorsForNonExistingIdsReturnsEmptyArray()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/vectors?from=2015-11-14T10:00:00",
                Body = new ComponentsDTO
                {
                    X = "NonExistingTimeSeries.csv;item1",
                    Y = "NonExistingTimeSeries.csv;item2"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var vectors = JsonSerializer.Deserialize<object[][]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(vectors);
        }

        [Fact]
        public async Task GetVectorsForManyIsOk()
        {
            var request = new
            {
                Url = $"api/timeseries/{ConnectionId}/list/vectors?from=2015-11-14T10:00:00",
                Body = new[]
                {
                    new ComponentsDTO
                    {
                        X = "timeseries.csv;item1",
                        Y = "timeseries.csv;item2"
                    },
                    new ComponentsDTO
                    {
                        X = "timeseries.csv;item1",
                        Y = "timeseries.csv;item2"
                    }
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var timeSeriesList = JsonSerializer.Deserialize<Dictionary<string, object[][]>>(json, _options);

            Assert.Single(timeSeriesList); // Duplicate vectors...
            var vectorKey = request.Body[0].ToString();
            Assert.Equal(7, timeSeriesList[vectorKey].Length);
            Assert.IsType<DateTime>(DateTime.Parse(timeSeriesList[vectorKey][6][0].ToString()));
            var vector = JsonSerializer.Deserialize<Vector<double>>(timeSeriesList[vectorKey][6][1].ToString());
            Assert.IsType<Vector<double>>(vector);
        }
    }
}