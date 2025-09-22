namespace IntegrationTestHost.Tests
{
    using DHI.Services.Models.WebApi;
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class ModelsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "json-models";
        private const string ReaderId = "fakeReader";
        
        private ModelDataReaderDtoRequest? _reader = new ModelDataReaderDtoRequest();

        public ModelsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full ModelsController integration suite")]
        public async Task Run_ModelsController_IntegrationFlow()
        {
            await Step("Get", Get);
            await Step("GetIds", GetIds);
            await Step("GetCount", GetCount);
            await Step("Add", Add);
            await Step("Update", Update);
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

        private async Task Get()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/{ReaderId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetIds()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"api/models/readers/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Add()
        {
            var (url, body) = CreateReaderRequest();

            var response = await _client.PostAsync(url, ModelsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _reader = JsonSerializer.Deserialize<ModelDataReaderDtoRequest>(json, SerializerOptionsDefault.Options);
        }

        private async Task Update()
        {
            var (url, body) = CreateReaderRequest("Updated name");
            var response = await _client.PutAsync(url, ModelsContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _reader = JsonSerializer.Deserialize<ModelDataReaderDtoRequest>(json, SerializerOptionsDefault.Options);
        }

        private async Task Delete()
        {
            var (url, body) = CreateReaderRequest();

            var response = await _client.DeleteAsync($"{url}/{_reader?.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private (string Url, ModelDataReaderDtoRequest Body) CreateReaderRequest(string? nameOverride = null)
        {
            var body = new ModelDataReaderDtoRequest
            {
                Id = "testReader",
                Name = nameOverride ?? "Test reader",
                ModelDataReaderTypeName = "BaseWebApi.FakeModelDataReader, BaseWebApi"
            };

            return ($"/api/models/readers/{ConnectionId}", body);
        }

    }
}
