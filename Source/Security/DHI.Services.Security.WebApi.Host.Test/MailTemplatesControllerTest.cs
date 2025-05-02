namespace DHI.Services.Security.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DTOs;
    using Mails;
    using Xunit;

    [Collection("Controllers collection")]
    public class MailTemplatesControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ControllersFixture _fixture = new ControllersFixture();
        public MailTemplatesControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
            _fixture = fixture;
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/mailtemplates/NonExistingHostName");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/mailtemplates",
                Body = new MailTemplateDTO
                {
                    Id = "NonExisting",
                    Name = "Account activation email template",
                    Subject = "DHI WaterData - Sign up",
                    Body = "Hello World",
                    From = "noreply@dhigroup.com",
                    FromDisplayName = "DHI WaterData"
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
            _fixture.ResetValues(); // Reset values before the test

            var request = new
            {
                Url = "/api/mailtemplates",
                Body = new MailTemplateDTO
                {
                    Id = "AccountActivation",
                    Name = "Account activation email template",
                    Subject = "DHI WaterData - Sign up",
                    Body = "Hello World",
                    From = "noreply@dhigroup.com",
                    FromDisplayName = "DHI WaterData"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task AddWithMissingFromDisplayNameReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/mailtemplates",
                Body = new MailTemplateDTO
                {
                    Id = "AccountActivation",
                    Name = "Account activation email template",
                    Subject = "DHI WaterData - Sign up",
                    Body = "Hello World",
                    From = "noreply@dhigroup.com",
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The FromDisplayName field is required", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/mailtemplates/NonExistingMailTemplate");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/mailtemplates/AccountActivation");
            var json = await response.Content.ReadAsStringAsync();
            var template = JsonSerializer.Deserialize<MailTemplate>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("noreply@dhigroup.com", template.From);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/mailtemplates");
            var json = await response.Content.ReadAsStringAsync();
            var templates = JsonSerializer.Deserialize<IEnumerable<MailTemplate>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, templates.Length);
            Assert.Contains(templates, t => t.Id == "PasswordReset");
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync("api/mailtemplates/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, ids.Length);
            Assert.Contains("PasswordReset", ids);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/mailtemplates/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            _fixture.ResetValues(); // Reset values before the test

            var request = new
            {
                Url = "/api/mailtemplates",
                Body = new MailTemplateDTO
                {
                    Id = "MyAccountActivation",
                    Name = "Account activation email template",
                    Subject = "My Service - Sign up",
                    Body = "Hello World",
                    From = "noreply@dhigroup.com",
                    FromDisplayName = "DHI WaterData"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var template = JsonSerializer.Deserialize<MailTemplate>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/mailtemplates/{request.Body.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, template.Id);

            // Update
            request.Body.Body = "Howdy World";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            template = JsonSerializer.Deserialize<MailTemplate>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(request.Body.Body, template.Body);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{request.Body.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{request.Body.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}