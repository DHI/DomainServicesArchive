namespace IntegrationTestHost.Tests
{
    using DHI.Services.Provider.DS;
    using DHI.Services.TimeSeries;
    using DHI.Services.TimeSeries.WebApi;
    using DocumentFormat.OpenXml.Spreadsheet;
    using System;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class TimeSeriesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string ConnectionId = "myTsConnection";
        private const string ConnectionIdDiscrete = "grouped-csv";
        private const string SeriesId = "timeseries.csv;item1";
        private DateTime _dateTime;

        public TimeSeriesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options;
            _output = output;
        }

        [Fact(DisplayName = "Run full TimeSeriesController integration suite")]
        public async Task Run_TimeSeriesController_IntegrationFlow()
        {
            await Step("EnsureTimeSeriesDoesNotExist", EnsureTimeSeriesDoesNotExist);
            await Step("Add", Add);
            await Step("Update", Update);
            await Step("SetValues", SetValues);
            //await Step("DeleteByGroup", DeleteByGroup);
            await Step("Get", Get);
            await Step("GetCount", GetCount);
            await Step("GetDateTimes", GetDateTimes);
            await Step("GetFirstDateTime", GetFirstDateTime);
            await Step("GetFirstValue", GetFirstValue);
            await Step("GetFirstValueList", GetFirstValueList);
            await Step("GetFirstValueAfter", GetFirstValueAfter);
            //await Step("GetFullNames", GetFullNames);
            await Step("GetIds", GetIds);
            await Step("GetLastDateTime", GetLastDateTime);
            await Step("GetLastValue", GetLastValue);
            await Step("GetLastValueList", GetLastValueList);
            await Step("GetLastValueBefore", GetLastValueBefore);
            await Step("GetList", GetList);
            await Step("GetListByIds", GetListByIds);
            await Step("GetValue", GetValue);
            await Step("GetValues", GetValues);
            await Step("GetValuesList", GetValuesList);
            await Step("GetVectors", GetVectors);
            await Step("GetVectorsList", GetVectorsList);
            await Step("DeleteValues", DeleteValues);
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

        private async Task EnsureTimeSeriesDoesNotExist()
        {
            var id = FullNameString.ToUrl(SeriesId);
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var delete = await _client.DeleteAsync($"api/timeseries/{ConnectionId}/{id}");
                _output.WriteLine(await delete.Content.ReadAsStringAsync());
                Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            }
        }


        private async Task Add()
        {
            var dto = new TimeSeriesDTO
            {
                FullName = SeriesId,
                Id = SeriesId,
                DataType = "Instantaneous",
                Quantity = "WaterLevel",
                Unit = "m",
                Dimension = "Length",
                Data = new TimeSeriesData<double>()
            };

            var response = await _client.PostAsync(
                $"api/timeseries/{ConnectionId}",
                TimeSeriesContentHelper.GetStringContent(dto)
            );

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task Update()
        {
            var response = await _client.PutAsJsonAsync($"api/timeseries/{ConnectionId}", new TimeSeriesDTO { FullName = SeriesId });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task SetValues()
        {
            _dateTime = DateTime.Now;
            var response = await _client.PutAsJsonAsync($"api/timeseries/{ConnectionId}/{SeriesId}/values", new TimeSeriesDataDTO { DateTimes = new[] { _dateTime }, Values = new List<double?> { 42.0 } });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Delete()
        {
            var response = await _client.DeleteAsync($"api/timeseries/{ConnectionId}/{SeriesId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task DeleteByGroup()
        {
            var response = await _client.DeleteAsync($"api/timeseries/{ConnectionId}/group/mysubfolder");
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task DeleteValues()
        {
            var response = await _client.DeleteAsync($"api/timeseries/{ConnectionId}/{SeriesId}/values?from=2015-01-01");
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task Get()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetDateTimes()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/datetimes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFirstDateTime()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/datetime/first");
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

        private async Task GetFirstValueAfter()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/value/firstafter/2015-01-01T00:00:00");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFullNames()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/fullnames");
            _output.WriteLine(await response.Content.ReadAsStringAsync());
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

        private async Task GetLastValue()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionId}/{SeriesId}/value/last");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLastValueList()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionId}/list/value/last", new[] { SeriesId });
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

        private async Task GetValue()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdDiscrete}/timeseries.csv;item1/value/2015-11-14T10:52:31");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetValues()
        {
            var response = await _client.GetAsync($"api/timeseries/{ConnectionIdDiscrete}/{SeriesId}/values");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetValuesList()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionIdDiscrete}/list/values", new[] { SeriesId });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetVectors()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionIdDiscrete}/vectors", new ComponentsDTO { X = SeriesId, Y = "timeseries.csv;item2" });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetVectorsList()
        {
            var response = await _client.PostAsJsonAsync($"api/timeseries/{ConnectionIdDiscrete}/list/vectors", new[] { new ComponentsDTO { X = SeriesId, Y = "timeseries.csv;item2" } });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
