namespace IntegrationTestHost.Tests
{
    using DHI.Services.Scalars.WebApi;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;
    using DHI.Services.WebApiCore;

    public class ScalarsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private const string ConnectionId = "ds-scalars";
        private const string ScalarFullName = "MyGroup/MySubGroup/IntegrationScalar";

        public ScalarsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full ScalarsController integration suite")]
        public async Task Run_ScalarsController_IntegrationFlow()
        {
            await Step("EnsureScalarDoesNotExist", EnsureScalarDoesNotExist);
            await Step("AddScalar", AddScalar);
            await Step("GetScalar", GetScalar);
            await Step("UpdateScalar", UpdateScalar);
            await Step("SetScalarData", SetScalarData);
            await Step("SetScalarLocked", SetScalarLocked);
            await Step("GetScalarList", GetScalarList);
            await Step("GetScalarCount", GetScalarCount);
            await Step("GetScalarIds", GetScalarIds);
            await Step("GetScalarFullNames", GetScalarFullNames);
            await Step("DeleteScalar", DeleteScalar);
        }

        private async Task Step(string name, Func<Task> action)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                await action();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureScalarDoesNotExist()
        {
            var id = FullNameString.ToUrl(ScalarFullName);
            var response = await _client.GetAsync($"/api/scalars/{ConnectionId}/{id}");
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var delete = await _client.DeleteAsync($"/api/scalars/{ConnectionId}/{id}");
                Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            }
        }

        private async Task AddScalar()
        {
            var dto = new ScalarDTO
            {
                FullName = ScalarFullName,
                ValueTypeName = "System.String",
                Value = "Test"
            };

            var response = await _client.PostAsJsonAsync($"/api/scalars/{ConnectionId}", dto);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetScalar()
        {
            var id = FullNameString.ToUrl(ScalarFullName);
            var response = await _client.GetAsync($"/api/scalars/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UpdateScalar()
        {
            var dto = new ScalarDTO
            {
                FullName = ScalarFullName,
                ValueTypeName = "System.String",
                Description = "Updated by integration test"
            };

            var response = await _client.PutAsJsonAsync($"/api/scalars/{ConnectionId}", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task SetScalarData()
        {
            var id = FullNameString.ToUrl(ScalarFullName);
            var dto = new ScalarDataDTO
            {
                Value = "This is string!",
                DateTime = DateTime.UtcNow
            };

            var response = await _client.PutAsJsonAsync($"/api/scalars/{ConnectionId}/{id}/data", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task SetScalarLocked()
        {
            var id = FullNameString.ToUrl(ScalarFullName);
            var dto = new LockedDTO { Locked = true };

            var response = await _client.PutAsJsonAsync($"/api/scalars/{ConnectionId}/{id}/locked", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetScalarList()
        {
            var response = await _client.GetAsync($"/api/scalars/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetScalarCount()
        {
            var response = await _client.GetAsync($"/api/scalars/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetScalarIds()
        {
            var response = await _client.GetAsync($"/api/scalars/{ConnectionId}/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetScalarFullNames()
        {
            var response = await _client.GetAsync($"/api/scalars/{ConnectionId}/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteScalar()
        {
            var id = FullNameString.ToUrl(ScalarFullName);
            var response = await _client.DeleteAsync($"/api/scalars/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var confirm = await _client.GetAsync($"/api/scalars/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, confirm.StatusCode);
        }
    }
}
