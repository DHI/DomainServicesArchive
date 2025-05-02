namespace DHI.Services.TimeSteps.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Provider.MIKECore;
    using Xunit;

    [Collection("Controllers collection")]
    public class TimeStepsControllerTest
    {
        public TimeStepsControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private const string ConnectionId = "dfs2";

        [Fact]
        public async Task GetDataWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/timesteps/{ConnectionId}/H Water Depth m/data/2014-10-01T12:00:00");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetDataForNonExistingItemReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/NonExistingItem/data/2014-10-01T12:00:00");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetDataForNonExistingDateReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/H Water Depth m/data/2099-10-01T12:00:00");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetDataIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/H Water Depth m/data/2014-10-01T12:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var raster = JsonSerializer.Deserialize<Dfs2TimeStepServer.Raster>(json, new JsonSerializerOptions
            {
                Converters = { new ObjectInferredTypeConverter() }
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var bitmap = raster.ToBitmap();
            Assert.IsType<string>("Bitmap looks very nice :-)");
            Assert.IsType<Dfs2TimeStepServer.Raster>(raster);
            Assert.Equal("Instantaneous", raster.Metadata["valueType"]);
            Assert.Equal("meter", raster.Metadata["unit"]);
        }

        [Fact]
        public async Task GetFirstDataAfterIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/H Water Depth m/data/firstafter/2014-10-01T12:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var raster = JsonSerializer.Deserialize<Dfs2TimeStepServer.Raster>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new DateTime(2014, 10, 1, 12, 30, 0), raster.DateTime);
        }

        [Fact]
        public async Task GetLastDataBeforeIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/H Water Depth m/data/lastbefore/2014-10-01T12:00:00");
            var json = await response.Content.ReadAsStringAsync();
            var raster = JsonSerializer.Deserialize<Dfs2TimeStepServer.Raster>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new DateTime(2014, 10, 1, 11, 30, 0), raster.DateTime);
        }

        [Fact]
        public async Task GetLastDataIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/H Water Depth m/data/last");
            var json = await response.Content.ReadAsStringAsync();
            var raster = JsonSerializer.Deserialize<Dfs2TimeStepServer.Raster>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new DateTime(2014, 10, 2, 0, 0, 0), raster.DateTime);
        }

        [Fact]
        public async Task GetDataFormManyItemsAndDateTimesIsOk()
        {
            var request = new
            {
                Url = $"api/timesteps/{ConnectionId}/list",
                Body = new Dictionary<string, List<DateTime>>
                {
                    { "H Water Depth m", new List<DateTime>
                    {
                        new DateTime(2014,10,1, 12, 0, 0),
                        new DateTime(2014,10,1, 12, 30, 0),
                        new DateTime(2014,10,1, 16, 30, 0)
                    }}
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IDictionary<string, IDictionary<DateTime, object>>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(data);
            Assert.Equal(3, data["H Water Depth m"].Count);
        }

        [Fact]
        public async Task GetDateTimesIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/datetimes");
            var json = await response.Content.ReadAsStringAsync();
            var dateTimes = JsonSerializer.Deserialize<DateTime[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(49, dateTimes.Length);
        }

        [Fact]
        public async Task GeFirstDateTimeIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/datetime/first");
            var json = await response.Content.ReadAsStringAsync();
            var dateTime = JsonSerializer.Deserialize<DateTime>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new DateTime(2014, 10, 1, 0, 0, 0), dateTime);
        }

        [Fact]
        public async Task GeLastDateTimeIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/datetime/last");
            var json = await response.Content.ReadAsStringAsync();
            var dateTime = JsonSerializer.Deserialize<DateTime>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new DateTime(2014, 10, 2, 0, 0, 0), dateTime);
        }

        [Fact]
        public async Task GetItemsIsOk()
        {
            var response = await _client.GetAsync($"api/timesteps/{ConnectionId}/items");
            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<Item<string>[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(items, item => item.Id == "H Water Depth m");
            Assert.Equal(3, items.Length);
        }
    }
}