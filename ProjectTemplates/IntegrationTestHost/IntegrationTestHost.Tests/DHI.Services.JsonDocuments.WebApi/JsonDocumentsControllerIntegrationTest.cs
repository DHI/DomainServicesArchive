namespace IntegrationTestHost.Tests
{
    using DHI.Services.JsonDocuments.WebApi;
    using DHI.Services.WebApiCore;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class JsonDocumentsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string ConnectionId = "json-documents";
        private string _fullName;

        public JsonDocumentsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full JsonDocumentsController integration suite")]
        public async Task Run_JsonDocumentsController_IntegrationFlow()
        {
            await Step("AddDocument", AddDocument);
            await Step("GetById", GetById);
            await Step("GetAll", GetAll);
            await Step("GetWithSelectors", GetWithSelectors);
            await Step("GetByGroup", GetByGroup);
            await Step("GetByGroupWithSelectors", GetByGroupWithSelectors);
            await Step("GetFullNames", GetFullNames);
            await Step("GetCount", GetCount);
            await Step("GetWithinTimeInterval", GetWithinTimeInterval);
            await Step("GetByQuery", GetByQuery);
            await Step("GetByQueryWithSelectors", GetByQueryWithSelectors);
            await Step("UpdateDocument", UpdateDocument);
            await Step("SoftDeleteDocument", SoftDeleteDocument);
            await Step("HardDeleteDocument", HardDeleteDocument);
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

        private async Task AddDocument()
        {
            _fullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}";
            var request = new JsonDocumentDTO
            {
                FullName = _fullName,
                Data = JsonSerializer.Serialize(new { foo = 1, bar = 2 }),
                Permissions = new List<PermissionDTO>
                {
                    new PermissionDTO
                    {
                        Operation = "read",
                        Principals = new List<string> { "Administrators", "Editors"}
                    },
                    new PermissionDTO
                    {
                        Operation = "update",
                        Principals = new List<string> { "Administrators", "Editors"}
                    },
                    new PermissionDTO
                    {
                        Operation = "delete",
                        Principals = new List<string> { "Administrators", "Editors"}
                    }
                }
            };

            var response = await _client.PostAsync($"api/jsondocuments/{ConnectionId}", JsonDocumentsContentHelper.GetStringContent(request));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetById()
        {
            var url = $"api/jsondocuments/{ConnectionId}/{FullNameString.ToUrl(_fullName)}";
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAll()
        {
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetWithSelectors()
        {
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}?dataSelectors=[foo]");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByGroup()
        {
            var group = _fullName.Substring(0, _fullName.LastIndexOf("/"));
            var url = $"api/jsondocuments/{ConnectionId}/group/{FullNameString.ToUrl(group)}";
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByGroupWithSelectors()
        {
            var group = _fullName.Substring(0, _fullName.LastIndexOf("/"));
            var url = $"api/jsondocuments/{ConnectionId}/group/{FullNameString.ToUrl(group)}?dataSelectors=[foo]";
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFullNames()
        {
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetWithinTimeInterval()
        {
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}?from=2019-01-01&to=2030-01-01");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQuery()
        {
            var query = new[]
            {
                new { Item = "Id", QueryOperator = "Equal", Value = _fullName }
            };
            var response = await _client.PostAsJsonAsync($"api/jsondocuments/{ConnectionId}/query", query);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQueryWithSelectors()
        {
            var query = new[]
            {
                new { Item = "Id", QueryOperator = "Equal", Value = _fullName }
            };
            var response = await _client.PostAsJsonAsync($"api/jsondocuments/{ConnectionId}/query?dataSelectors=[foo]", query);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UpdateDocument()
        {
            var request = new JsonDocumentDTO
            {
                FullName = _fullName,
                Data = JsonSerializer.Serialize(new { foo = 999 }),
                Permissions = new List<PermissionDTO>
                {
                    new PermissionDTO
                    {
                        Operation = "read",
                        Principals = new List<string> { "Administrators", "Editors"}
                    },
                    new PermissionDTO
                    {
                        Operation = "update",
                        Principals = new List<string> { "Administrators", "Editors"}
                    },
                    new PermissionDTO
                    {
                        Operation = "delete",
                        Principals = new List<string> { "Administrators", "Editors"}
                    }
                }
            };

            var response = await _client.PutAsync($"api/jsondocuments/{ConnectionId}", JsonDocumentsContentHelper.GetStringContent(request));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task SoftDeleteDocument()
        {
            var url = $"api/jsondocuments/{ConnectionId}/{FullNameString.ToUrl(_fullName)}?softDelete=true";
            var response = await _client.DeleteAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task HardDeleteDocument()
        {
            var url = $"api/jsondocuments/{ConnectionId}/{FullNameString.ToUrl(_fullName)}";
            var response = await _client.DeleteAsync(url);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
