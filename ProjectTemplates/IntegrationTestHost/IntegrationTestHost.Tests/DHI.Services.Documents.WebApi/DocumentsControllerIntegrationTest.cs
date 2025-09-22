namespace IntegrationTestHost.Tests
{
    using System.Net;
    using System.Text.Json;
    using System.Text;
    using Xunit.Abstractions;
    using DHI.Services.WebApiCore;

    public class DocumentsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private const string ConnectionId = "mclite";
        private const string ConnectionIdMikeCloud = "document-mc";
        private const string TestId = "IntegrationTestDoc.txt";
        private const string FullId = "IntegrationTest/TestId.txt";
        private const string FullIdDsTest = "DS Test/TestId.txt";

        public DocumentsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full DocumentsController integration suite")]
        public async Task Run_DocumentsController_IntegrationFlow()
        {
            await Step("EnsureTestDocDoesNotExist", EnsureTestDocDoesNotExist);
            await Step("UploadDocument", UploadDocument);
            await Step("UploadDocumentToMIKECloud", UploadDocumentToMIKECloud);
            await Step("GetDocument", GetDocument);
            await Step("GetDocumentFromMIKECloud", GetDocumentFromMIKECloud);
            await Step("GetDocumentMetadata", GetDocumentMetadata);
            await Step("GetAllMetadata", GetAllMetadata);
            await Step("GetMetadataByFilter", GetMetadataByFilter);
            await Step("GetCount", GetCount);
            await Step("GetIds", GetIds);
            await Step("GetFullNames", GetFullNames);
            await Step("GetFullNamesByGroup", GetFullNamesByGroup);
            await Step("GetList", GetList);
            await Step("GetByGroup", GetByGroup);
            await Step("DeleteDocument", DeleteDocument);
            await Step("DeleteDocumentFromMIKECLoud", DeleteDocumentFromMIKECLoud);
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

        private async Task EnsureTestDocDoesNotExist()
        {
            var id = FullNameString.ToUrl(FullId);
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/{id}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var delete = await _client.DeleteAsync($"/api/documents/{ConnectionId}/{id}");
                Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            }
        }

        private async Task UploadDocument()
        {
            var id = FullNameString.ToUrl(FullId);
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("This is integration test")));
            content.Add(streamContent, "file", TestId);

            var response = await _client.PostAsync($"/api/documents/{ConnectionId}/{id}", content);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task UploadDocumentToMIKECloud()
        {
            var id = FullNameString.ToUrl(FullIdDsTest);
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("This is integration test")));
            content.Add(streamContent, "file", TestId);

            var response = await _client.PostAsync($"/api/documents/{ConnectionIdMikeCloud}/{id}", content);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetDocument()
        {
            var id = FullNameString.ToUrl(FullId);
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetDocumentFromMIKECloud()
        {
            var id = FullNameString.ToUrl(FullIdDsTest);
            var response = await _client.GetAsync($"/api/documents/{ConnectionIdMikeCloud}/{id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetDocumentMetadata()
        {
            var id = FullNameString.ToUrl(FullId);
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/{id}/metadata");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAllMetadata()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/metadata");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMetadataByFilter()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/metadata?filter=test");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCount()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetIds()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFullNames()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetFullNamesByGroup()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}/fullnames?group=IntegrationTest");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetList()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByGroup()
        {
            var response = await _client.GetAsync($"/api/documents/{ConnectionId}?group=IntegrationTest");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteDocument()
        {
            var id = FullNameString.ToUrl(FullId);
            var response = await _client.DeleteAsync($"/api/documents/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task DeleteDocumentFromMIKECLoud()
        {
            var id = FullNameString.ToUrl(FullIdDsTest);
            var response = await _client.DeleteAsync($"/api/documents/{ConnectionIdMikeCloud}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
