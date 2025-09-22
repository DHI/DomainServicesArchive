namespace IntegrationTestHost.Tests
{
    using global::DHI.Services.WebApiCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection.Metadata;
    using System.Security.Claims;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class ControllersFixture : IDisposable
    {
        public HttpClient Client { get; }
        public JsonSerializerOptions SerializerOptions { get; }
        private readonly HttpMessageHandler _handler;

        private readonly IConfiguration _config;
        private ITestOutputHelper? _output;

        public ControllersFixture()
        {
            _config = new ConfigurationBuilder()
           .AddJsonFile("appsettings-test.json", optional: false)
           .Build();

            var baseUri = new Uri("http://localhost:5000");
            bool inCi = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

            _handler = inCi
                ? new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                          HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }
                : new HttpClientHandler();

            Client = new HttpClient(_handler) { BaseAddress = baseUri };
            SerializerOptions = new JsonSerializerOptions(
                DHI.Services.Security.WebApi.SerializerOptionsDefault.Options);
        }

        /// <summary>
        /// Returns a new HttpClient with Bearer token set for an admin
        /// </summary>
        public HttpClient CreateAuthenticatedClientAsAdmin()
        {
            return CreateAuthenticatedClient(GenerateJwt(GetAdminClaims()));
        }

        /// <summary>
        /// Returns a new HttpClient with Bearer token set for a standard user
        /// </summary>
        public HttpClient CreateAuthenticatedClientAsUser()
        {
            return CreateAuthenticatedClient(GenerateJwt(GetUserClaims()));
        }

        public HttpClient CreateAuthenticatedClient(string token)
        {
            var client = new HttpClient(_handler, disposeHandler: false)
            {
                BaseAddress = Client.BaseAddress
            };
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public string GenerateJwt(IEnumerable<Claim> claims)
        {
            var issuer = _config["Tokens:Issuer"];
            var audience = _config["Tokens:Audience"];
            var privateKey = _config["Tokens:PrivateRSAKey"].Resolve();

            var rsa = RSA.BuildSigningKey(privateKey);
            var creds = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        private IEnumerable<Claim> GetAdminClaims()
        {
            return new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "hardcoded_user"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "hardcoded_user"),
                new Claim(ClaimTypes.Name, "Hardcoded User"),
                new Claim(ClaimTypes.GroupSid, "Administrators"),
                new Claim(ClaimTypes.GroupSid, "Editors")
            };
        }

        private IEnumerable<Claim> GetUserClaims()
        {
            return new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "testuser"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.GroupSid, "Editors")
            };
        }

        public void Dispose() => _handler.Dispose();
    }
}
