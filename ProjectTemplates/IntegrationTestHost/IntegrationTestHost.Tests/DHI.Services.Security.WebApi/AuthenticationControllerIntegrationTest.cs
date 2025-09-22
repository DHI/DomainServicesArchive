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

    public class AuthenticationControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string HardcodedUserId = "hardcoded_user_auth";
        private const string HardcodedUserPassword = "S3cur3P@sswordSecure!";

        public AuthenticationControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full AuthenticationController integration suite")]
        public async Task Run_AuthenticationController_IntegrationFlow()
        {
            await Step("EnsureHardcodedUserExists", EnsureHardcodedUserExists);
            await Step("CreateToken", CreateToken);
            await Step("ValidateToken", ValidateToken);
            await Step("ValidateAccount", ValidateAccount);
            await Step("RefreshToken", RefreshToken);
            await Step("RegisterForOtp", RegisterForOtp);
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

        private async Task CreateToken()
        {
            var request = new ValidationDTO
            {
                Id = HardcodedUserId,
                Password = HardcodedUserPassword
            };

            var response = await _client.PostAsJsonAsync("api/tokens", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task ValidateToken()
        {
            var request = new ValidationDTO
            {
                Id = HardcodedUserId,
                Password = HardcodedUserPassword
            };

            var tokenResponse = await _client.PostAsJsonAsync("api/tokens", request);
            var json = await tokenResponse.Content.ReadAsStringAsync();

            var tokens = JsonSerializer.Deserialize<TokensDTO>(json, _options);
            var token = tokens?.AccessToken?.Token ?? throw new Exception("Token generation failed");

            var validateResponse = await _client.PostAsJsonAsync("api/tokens/validation", token);
            Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
        }

        private async Task ValidateAccount()
        {
            var body = new ValidationDTO
            {
                Id = HardcodedUserId,
                Password = HardcodedUserPassword
            };

            var response = await _client.PostAsJsonAsync("api/accounts/validation", body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task RefreshToken()
        {
            var loginBody = new ValidationDTO
            {
                Id = HardcodedUserId,
                Password = HardcodedUserPassword
            };

            var loginResponse = await _client.PostAsJsonAsync("api/tokens", loginBody);
            var loginJson = await loginResponse.Content.ReadAsStringAsync();
            var tokens = JsonSerializer.Deserialize<TokensDTO>(loginJson, _options);

            var refreshBody = tokens?.RefreshToken?.Token ?? throw new Exception("Refresh token missing");

            var refreshResponse = await _client.PostAsJsonAsync("api/tokens/refresh", refreshBody);
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        }

        private async Task RegisterForOtp()
        {
            var body = new OtpRegistrationDTO
            {
                Id = HardcodedUserId,
                Password = HardcodedUserPassword
            };

            var response = await _client.PostAsJsonAsync("api/tokens/otp/registration", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
