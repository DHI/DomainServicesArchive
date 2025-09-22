namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DHI.Services.Security.WebApi.DTOs;
    using Xunit.Abstractions;

    public class AccountsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string HardcodedUserId = "hardcoded_user";
        private const string HardcodedUserPassword = "S3cur3P@ssword!";
        private string _testUserId = "";

        public AccountsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full AccountsController integration suite")]
        public async Task Run_AccountsController_IntegrationFlow()
        {
            await Step("EnsureHardcodedUserExists", EnsureHardcodedUserExists);
            await Step("AddTestUser", AddTestUser);
            await Step("GetAccountById", GetAccountById);
            await Step("UpdateAccount", UpdateAccount);
            await Step("GetMe", GetMe);
            await Step("UpdateMe", UpdateMe);
            await Step("GetPasswordPolicy", GetPasswordPolicy);
            await Step("GetLoginAttemptPolicy", GetLoginAttemptPolicy);
            await Step("GetAccountList", GetAccountList);
            await Step("GetAccountCount", GetAccountCount);
            await Step("DeleteTestUser", DeleteTestUser);
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

        private async Task EnsureHardcodedUserExists()
        {
            var getResponse = await _client.GetAsync($"api/accounts/{HardcodedUserId}");
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var deleteResponse = await _client.DeleteAsync($"api/accounts/{HardcodedUserId}");
                if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to delete existing hardcoded user. Status: {deleteResponse.StatusCode}, Content: {deleteContent}");
                }
            }

            var body = new AccountDTO(HardcodedUserId, "Hardcoded User")
            {
                Password = HardcodedUserPassword,
                Email = "hardcoded@dhi.local",
                Company = "DHI",
                UserGroups = new List<string> { "Editors" }
            };

            var createResponse = await _client.PostAsJsonAsync("api/accounts", body);
            var createContent = await createResponse.Content.ReadAsStringAsync();

            if (createResponse.StatusCode != HttpStatusCode.Created)
                throw new Exception($"Failed to create hardcoded user. Status: {createResponse.StatusCode}, Content: {createContent}");
        }

        private async Task AddTestUser()
        {
            _testUserId = $"testuser_{Guid.NewGuid():N}";

            var body = new AccountDTO(_testUserId, "Test User")
            {
                Password = "S3cur3P@ssword!",
                Email = $"{_testUserId}@dhi.local",
                Company = "Test Inc",
                UserGroups = new List<string> { "Editors", "Administrators" }
            };

            var response = await _client.PostAsJsonAsync("api/accounts", body);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"Create user failed. Status: {response.StatusCode}, Content: {content}");
        }

        private async Task GetAccountById()
        {
            var response = await _client.GetAsync($"api/accounts/{_testUserId}");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine($"Account details: {content}");
        }

        private async Task UpdateAccount()
        {
            var updateBody = new AccountDTO(_testUserId, "Updated User")
            {
                Password = "S3cur3P@ssword!",
                Email = $"{_testUserId}@updated.local",
                Company = "Updated Inc",
                UserGroups = new List<string> { "Editors" }
            };

            var response = await _client.PutAsJsonAsync("api/accounts", updateBody);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetMe()
        {
            var token = await GetAccessToken();
            var authClient = _fixture.CreateAuthenticatedClient(token);

            var response = await authClient.GetAsync("api/accounts/me");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UpdateMe()
        {
            var token = await GetAccessToken();
            var authClient = _fixture.CreateAuthenticatedClient(token);

            var get = await authClient.GetAsync("api/accounts/me");
            var json = await get.Content.ReadAsStringAsync();
            _output.WriteLine(json);
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options) ?? throw new Exception("Me endpoint returned null");

            var update = new MeDTO
            {
                Id = account.Id,
                Name = account.Name,
                Email = account.Email,
                Password = "S3cur3P@ssword!",
                Company = "UpdatedMe Inc"
            };

            var response = await authClient.PutAsJsonAsync("api/accounts/me", update);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine(content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetPasswordPolicy()
        {
            var response = await _client.GetAsync("api/accounts/passwordpolicy");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetLoginAttemptPolicy()
        {
            var response = await _client.GetAsync("api/accounts/loginattemptpolicy");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAccountList()
        {
            var response = await _client.GetAsync("api/accounts");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAccountCount()
        {
            var response = await _client.GetAsync("api/accounts/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteTestUser()
        {
            var response = await _client.DeleteAsync($"api/accounts/{_testUserId}");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task<string> GetAccessToken()
        {
            var tokenRequest = new
            {
                id = HardcodedUserId,
                password = HardcodedUserPassword,
                otp = "ignored",
                otpAuthenticator = "ignored"
            };

            var response = await _client.PostAsJsonAsync("/api/tokens", tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Token request failed. Status: {response.StatusCode}, Content: {content}");

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("AccessToken").GetProperty("token").GetString();
            return token ?? throw new Exception("AccessToken.token is null");
        }

    }
}
