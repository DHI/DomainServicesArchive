namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DHI.Services.Security.WebApi.DTOs;
    using Xunit.Abstractions;

    public class MailTemplatesControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string TestTemplateId = "TestTemplate";

        public MailTemplatesControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full MailTemplatesController integration suite")]
        public async Task Run_MailTemplatesController_IntegrationFlow()
        {
            await Step("EnsureTestTemplateNotExists", EnsureTestTemplateNotExists);
            await Step("AddTemplate", AddTemplate);
            await Step("GetTemplateById", GetTemplateById);
            await Step("UpdateTemplate", UpdateTemplate);
            await Step("GetAllTemplates", GetAllTemplates);
            await Step("GetTemplateCount", GetTemplateCount);
            await Step("GetTemplateIds", GetTemplateIds);
            await Step("DeleteTemplate", DeleteTemplate);
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

        private async Task EnsureTestTemplateNotExists()
        {
            var getResponse = await _client.GetAsync($"api/mailtemplates/{TestTemplateId}");
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var deleteResponse = await _client.DeleteAsync($"api/mailtemplates/{TestTemplateId}");
                if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception($"Could not delete existing test template '{TestTemplateId}'");
                }
            }
        }

        private async Task AddTemplate()
        {
            var template = new MailTemplateDTO
            {
                Id = TestTemplateId,
                Name = "Test Template",
                Subject = "Test Subject",
                Body = "Test Body",
                From = "test@dhi.local",
                FromDisplayName = "Test Sender"
            };

            var response = await _client.PostAsJsonAsync("api/mailtemplates", template);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetTemplateById()
        {
            var response = await _client.GetAsync($"api/mailtemplates/{TestTemplateId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UpdateTemplate()
        {
            var updated = new MailTemplateDTO
            {
                Id = TestTemplateId,
                Name = "Updated Template",
                Subject = "Updated Subject",
                Body = "Updated Body",
                From = "updated@dhi.local",
                FromDisplayName = "Updated Sender"
            };

            var response = await _client.PutAsJsonAsync("api/mailtemplates", updated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAllTemplates()
        {
            var response = await _client.GetAsync("api/mailtemplates");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTemplateCount()
        {
            var response = await _client.GetAsync("api/mailtemplates/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetTemplateIds()
        {
            var response = await _client.GetAsync("api/mailtemplates/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteTemplate()
        {
            var response = await _client.DeleteAsync($"api/mailtemplates/{TestTemplateId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var confirm = await _client.GetAsync($"api/mailtemplates/{TestTemplateId}");
            Assert.Equal(HttpStatusCode.NotFound, confirm.StatusCode);
        }
    }
}
