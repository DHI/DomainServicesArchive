namespace DHI.Services.Security.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DHI.Services.Accounts;
    using DTOs;
    using Xunit;

    [Collection("Controllers collection")]
    public class AuthenticationControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ControllersFixture _fixture = new ControllersFixture();
        public AuthenticationControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
            _fixture =  fixture;
        }

        [Fact]
        public async Task ValidationWithWrongPasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts/validation",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "wrongPassword"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Account validation failed" ?? "Account password is expired", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ValidationWithNonExistingAccountReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts/validation",
                Body = new ValidationDTO
                {
                    Id = "NonExisting",
                    Password = "Password"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Account validation failed" ?? "Account password is expired", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TokenGenerationWithWrongPasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "wrongPassword"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Account validation failed" ?? "Account password is expired", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TokenGenerationWithNonExistingAccountReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "NonExisting",
                    Password = "Password"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Account validation failed", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TokenRefreshWithNonExistingRefreshTokenReturns404NotFound()
        {
            var refreshRequest = new
            {
                Url = "/api/tokens/refresh",
                Body = "NonExistingRefreshToken"
            };

            var response = await _client.PostAsync(refreshRequest.Url, ContentHelper.GetStringContent(refreshRequest.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task OtpRegistrationWithOtpNotEnabledReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/tokens/otp/registration",
                Body = new OtpRegistrationDTO
                {
                    Id = "admin",
                    Password = "webapi"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidationIsOk()
        {
            _fixture.ResetValues(); // Reset values before the test

            var request = new
            {
                Url = "/api/accounts/validation",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "webapi"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TokenGenerationIsOk()
        {
            _fixture.ResetValues(); // Reset values before the test

            var request = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "webapi"
                }
            };
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var tokenDTO = JsonSerializer.Deserialize<TokensDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(tokenDTO.AccessToken.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public async Task TokenValidateIsOk()
        {
            _fixture.ResetValues(); // Reset values before the test

            var tokenRequest = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "webapi"
                }
            };

            var response = await _client.PostAsync(tokenRequest.Url, ContentHelper.GetStringContent(tokenRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var tokenDTO = JsonSerializer.Deserialize<TokensDTO>(json, _options);
            var token = tokenDTO.AccessToken.Token;

            var tokenValidationRequest = new
            {
                Url = "/api/tokens/validation",
                Body = token
            };

            var tokenValidationResponse = await _client.PostAsync(tokenValidationRequest.Url, ContentHelper.GetStringContent(tokenValidationRequest.Body));

            Assert.Equal(HttpStatusCode.OK, tokenValidationResponse.StatusCode);
        }

        [Fact]
        public async Task TokenValidateIsNotOk()
        {
            var tokenRequest = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "webapi"
                }
            };

            var response = await _client.PostAsync(tokenRequest.Url, ContentHelper.GetStringContent(tokenRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var tokenDTO = JsonSerializer.Deserialize<TokenDTO>(json, _options);
            var token = tokenDTO.Token;

            var tokenValidationRequest = new
            {
                Url = "/api/tokens/validation",
                Body = token + "ToMakeItBad"
            };

            var tokenValidationResponse = await _client.PostAsync(tokenValidationRequest.Url, ContentHelper.GetStringContent(tokenValidationRequest.Body));

            Assert.Equal(HttpStatusCode.BadRequest, tokenValidationResponse.StatusCode);
        }

        [Fact]
        public async Task TokenRefreshIsOk()
        {
            var request = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "editor",
                    Password = "webapi"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var tokenDTO = JsonSerializer.Deserialize<TokensDTO>(json, _options);
            var accessTokenDTO = tokenDTO.AccessToken;
            var refreshTokenDTO = tokenDTO.RefreshToken;

            var refreshRequest = new
            {
                Url = "/api/tokens/refresh",
                Body = refreshTokenDTO.Token
            };

            Thread.Sleep(1000);

            var refreshResponse = await _client.PostAsync(refreshRequest.Url, ContentHelper.GetStringContent(refreshRequest.Body));
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
            var refreshJson = await refreshResponse.Content.ReadAsStringAsync();

            var newTokenDTO = JsonSerializer.Deserialize<TokensDTO>(refreshJson, _options);
            var newAccessTokenDTO = newTokenDTO.AccessToken;
            var newRefreshTokenDTO = newTokenDTO.RefreshToken;

            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
            Assert.True(newAccessTokenDTO != accessTokenDTO);
            Assert.True(newRefreshTokenDTO != refreshTokenDTO);
            Assert.True(newAccessTokenDTO.Expiration > DateTime.UtcNow);
            Assert.True(newRefreshTokenDTO.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public async Task TokenGenerationIfEnabledStatusFalse()
        {
            _fixture.ResetValues(); // Reset values before the test

            // Arrange
            var request = new
            {
                Url = "/api/tokens",
                Body = new ValidationDTO
                {
                    Id = "admin",
                    Password = "webapi"
                }
            };

            // Act
            /// Get Account
            var responseGetAccount = await _client.GetAsync("api/accounts/" + request.Body.Id);
            var jsonGetAccount = await responseGetAccount.Content.ReadAsStringAsync();
            //_options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            var account = JsonSerializer.Deserialize<AccountDTO>(jsonGetAccount, _options);

            var requestAccount = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO(account.Id, account.Name)
                {
                    Enabled = false
                }
            };

            /// Update Account
            var responseUpdateAccount = await _client.PutAsync(requestAccount.Url, ContentHelper.GetStringContent(requestAccount.Body));
            var jsonUpdateAccount = await responseUpdateAccount.Content.ReadAsStringAsync();
            account = JsonSerializer.Deserialize<AccountDTO>(jsonUpdateAccount, _options);

            /// Create Token Validation
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Account is disabled", await response.Content.ReadAsStringAsync());
        }
    }
}