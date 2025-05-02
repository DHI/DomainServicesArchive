namespace DHI.Services.Models.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Xunit;

    [Collection("Controllers collection")]
    public class ModelsControllerTest
    {
        public ModelsControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private const string ConnectionId = "json-models";
        private const string ReaderId = "fakeReader";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/models/readers/{ConnectionId}/{ReaderId}");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingWithConnectionIdReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/models/readers/NonExistingConnection/{ReaderId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/NonExisting");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/models/readers/{ConnectionId}",
                Body = new ModelDataReaderDtoRequest
                {
                    Id = "NonExistingReader",
                    Name = "Non existing model reader",
                    ModelDataReaderTypeName = "DHI.Services.Models.WebApi.Host.FakeModelDataReader, DHI.Services.Models.WebApi.Host"
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = $"/api/models/readers/{ConnectionId}",
                Body = new ModelDataReaderDtoRequest
                {
                    Id = $"{ReaderId}",
                    Name = "Fake reader",
                    ModelDataReaderTypeName = "DHI.Services.Models.WebApi.Host.FakeModelDataReader, DHI.Services.Models.WebApi.Host"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/models/readers/{ConnectionId}/NonExistingReader");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/{ReaderId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var reader = JsonConvert.DeserializeObject<ModelDataReaderDtoResponse>(json);

            Assert.Contains("foo", reader.Parameters);
            Assert.Equal(typeof(long), reader.Parameters["foo"]);
            Assert.Contains("ts1-in", reader.InputTimeSeriesList);
            Assert.Contains("ts1-out", reader.OutputTimeSeriesList);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var readers = JsonConvert.DeserializeObject<IEnumerable<ModelDataReaderDtoRequest>>(json).ToArray();
            
            Assert.NotEmpty(readers);
            Assert.Contains($"{ReaderId}", readers.Select(m => m.Id));
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var readerIds = JsonConvert.DeserializeObject<IEnumerable<string>>(json).ToArray();

            Assert.NotEmpty(readerIds);
            Assert.Contains($"{ReaderId}", readerIds);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var count = JsonConvert.DeserializeObject<int>(json);
            Assert.True(count > 0);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/models/readers/{ConnectionId}",
                Body = new ModelDataReaderDtoRequest
                {
                    Id = "testReader",
                    Name = "Test reader",
                    ModelDataReaderTypeName = "DHI.Services.Models.WebApi.Host.FakeModelDataReader, DHI.Services.Models.WebApi.Host"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var reader = JsonConvert.DeserializeObject<ModelDataReaderDtoRequest>(json);
            Assert.Equal($"http://localhost/api/models/readers/{ConnectionId}/{reader.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, reader.Id);

            // Update
            request.Body.Name = "Updated name";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            reader = JsonConvert.DeserializeObject<ModelDataReaderDtoRequest>(json);
            Assert.Equal(request.Body.Name, reader.Name);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{reader.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{reader.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}