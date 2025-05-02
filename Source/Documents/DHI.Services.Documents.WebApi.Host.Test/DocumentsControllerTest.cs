namespace DHI.Services.Documents.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    [Collection("Controllers collection")]
    public class DocumentsControllerTest
    {
        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly string _tempAppDataPath;
        private readonly JsonSerializerOptions _options;
        private const string ConnectionId = "mclite";

        public DocumentsControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _tempAppDataPath = fixture.TempAppDataPath;
            _options = fixture.SerializerOptions;
        }

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/documents/{ConnectionId}/New group|child group|hdf.pdf");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/NonExistingDocument");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("/New group/child group/hdf.pdf", ids);
            Assert.Equal(3, ids.Length);
        }

        [Fact]
        public async Task GetMetadataIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/New group|OPERA_hdf_description_2014.pdf/metadata");
            var json = await response.Content.ReadAsStringAsync();
            var metadata = JsonSerializer.Deserialize<Parameters>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Title", metadata.Keys);
            Assert.Equal("OPERA_hdf_description_2014.pdf", metadata["Title"]);
            Assert.Contains("IsPublic", metadata.Keys);
            Assert.True(bool.Parse(metadata["IsPublic"]));
        }

        [Fact]
        public async Task GetAllMetadataIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/metadata");
            var json = await response.Content.ReadAsStringAsync();
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, Parameters>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, dictionary.Count);
            Assert.Contains("/New group/OPERA_hdf_description_2014.pdf", dictionary.Keys);

            var metadata = dictionary["/New group/OPERA_hdf_description_2014.pdf"];
            Assert.Contains("Title", metadata.Keys);
            Assert.Equal("OPERA_hdf_description_2014.pdf", metadata["Title"]);
            Assert.Contains("IsPublic", metadata.Keys);
            Assert.True(bool.Parse(metadata["IsPublic"]));
        }

        [Fact]
        public async Task GetMetadataByFilterIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/metadata?filter=hdf");
            var json = await response.Content.ReadAsStringAsync();
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, Parameters>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, dictionary.Count);
            Assert.Contains("/New group/OPERA_hdf_description_2014.pdf", dictionary.Keys);

            var metadata = dictionary["/New group/OPERA_hdf_description_2014.pdf"];
            Assert.Contains("Title", metadata.Keys);
            Assert.Equal("OPERA_hdf_description_2014.pdf", metadata["Title"]);
            Assert.Contains("IsPublic", metadata.Keys);
            Assert.True(bool.Parse(metadata["IsPublic"]));
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/New group|child group|hdf.pdf");
            var stream = await response.Content.ReadAsStreamAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/pdf", response.Content.Headers.ContentType.ToString());
            Assert.Equal(366372, stream.Length);
        }

        [Fact]
        public async Task FailedValidationReturns400BadRequest()
        {
            HttpResponseMessage response;
            using (var file = File.OpenRead(Path.Combine(_tempAppDataPath, "HowdyWorld.txt")))
            using (var content = new StreamContent(file))
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(content, "file", "HowdyWorld.txt");
                response = await _client.PostAsync($"api/documents/{ConnectionId}/my-document", formData);
            }

            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Document does not contain the word", json);
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullNames = JsonSerializer.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, fullNames.Length);
        }

        [Fact]
        public async Task GetFullNamesByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}/fullnames?group=New group");
            var json = await response.Content.ReadAsStringAsync();
            var fullNames = JsonSerializer.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("/test root doc.pdf", fullNames);
            Assert.Equal(2, fullNames.Length);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var documents = JsonSerializer.Deserialize<List<Document<string>>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(documents, document => document.FullName == "/New group/OPERA_hdf_description_2014.pdf");
            Assert.Equal(3, documents.Count);
        }

        [Fact]
        public async Task GetByGroupIsOk()
        {
            var response = await _client.GetAsync($"api/documents/{ConnectionId}?group=New group");
            var json = await response.Content.ReadAsStringAsync();
            var documents = JsonSerializer.Deserialize<List<Document<string>>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(documents, document => document.FullName == "/New group/OPERA_hdf_description_2014.pdf");
            Assert.Contains(documents, document => document.FullName == "/New group/child group/hdf.pdf");
            Assert.Equal(2, documents.Count);
        }
    }
}