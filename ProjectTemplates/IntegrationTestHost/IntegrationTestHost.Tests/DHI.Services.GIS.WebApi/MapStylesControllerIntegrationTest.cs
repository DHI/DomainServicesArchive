namespace IntegrationTestHost.Tests
{
    using DHI.Services.GIS.WebApi;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class MapStylesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private string _styleId;

        public MapStylesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full MapStylesController integration suite")]
        public async Task Run_MapStylesController_IntegrationFlow()
        {
            await Step("AddMapStyle", AddMapStyle);
            await Step("GetMapStyle", GetMapStyle);
            await Step("GetPalette", GetPalette);
            await Step("GetAllMapStyles", GetAllMapStyles);
            await Step("GetMapStylesCount", GetMapStylesCount);
            await Step("DeleteMapStyle", DeleteMapStyle);
            await Step("VerifyDeletedReturnsNotFound", VerifyDeletedReturnsNotFound);
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

        private async Task AddMapStyle()
        {
            _styleId = $"integration-style-{Guid.NewGuid()}";
            var dto = new MapStyleDTO
            {
                Id = _styleId,
                Name = "Integration Test Style",
                StyleCode = "0^10:green,yellow,red"
            };

            var response = await _client.PostAsync("api/mapstyles", GISContentHelper.GetStringContent(dto));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetMapStyle()
        {
            var response = await _client.GetAsync($"api/mapstyles/{_styleId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetPalette()
        {
            var response = await _client.GetAsync($"api/mapstyles/{_styleId}/palette");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAllMapStyles()
        {
            var response = await _client.GetAsync("api/mapstyles");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMapStylesCount()
        {
            var response = await _client.GetAsync("api/mapstyles/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteMapStyle()
        {
            var response = await _client.DeleteAsync($"api/mapstyles/{_styleId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task VerifyDeletedReturnsNotFound()
        {
            var response = await _client.GetAsync($"api/mapstyles/{_styleId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
