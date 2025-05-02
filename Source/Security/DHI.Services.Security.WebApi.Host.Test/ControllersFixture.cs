namespace DHI.Services.Security.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Json;
    using DHI.Services.WebApiCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;

    public class ControllersFixture : WebApplicationFactory<Program>
    {
        private readonly string _tempContentRootPath;
        private const string _appDataPath = @"..\..\..\..\DHI.Services.Security.WebApi.Host\App_data";

        public ControllersFixture()
        {
            _tempContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(TempAppDataPath);
            CopyToTempAppDataPath("accounts.json");
            CopyToTempAppDataPath("user-groups.json");
            CopyToTempAppDataPath("mail-templates.json");
            CopyToTempAppDataPath("passwordhistory.json");
            CopyToTempAppDataPath("SwaggerInfo.md");
            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());
            ClientNonAdmin = CreateClient();
            ClientNonAdmin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWTNonAdmin());
            SerializerOptions = new JsonSerializerOptions(SerializerOptionsDefault.Options);

        }

        public HttpClient Client { get; private set; }
        public HttpClient ClientNonAdmin { get; private set; }
        public JsonSerializerOptions SerializerOptions { get; private set; }
        public string TempAppDataPath { get; private set; }



        public void ResetValues()
        {
            // Reinitialize HttpClient instances
            InitializeHttpClient();

            // Reset SerializerOptions if necessary
            SerializerOptions = new JsonSerializerOptions(SerializerOptionsDefault.Options);

            // Reset temporary app data path
            ResetTempAppDataPath();
        }

        private void InitializeHttpClient()
        {
            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());
            ClientNonAdmin = CreateClient();
            ClientNonAdmin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWTNonAdmin());
        }

        public void ResetTempAppDataPath()
        {
            if (Directory.Exists(TempAppDataPath))
            {
                Directory.Delete(TempAppDataPath, true);
            }
            Directory.CreateDirectory(TempAppDataPath);
        }

        public void CopyToTempAppDataPath(string fileName)
        {
            var destinationPath = Path.Combine(TempAppDataPath, fileName);
            File.Copy(Path.Combine(_appDataPath, fileName), destinationPath);
            new FileInfo(destinationPath).IsReadOnly = false;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("AppConfiguration:ContentRootPath", _tempContentRootPath);
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings-test.json");
            builder.ConfigureAppConfiguration(config => { config.AddJsonFile(configPath); });
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_tempContentRootPath, "App_Data"));
            base.ConfigureWebHost(builder);
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartupFilter, CustomStartupFilter>();
                });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Directory.Exists(_tempContentRootPath))
            {
                Directory.Delete(_tempContentRootPath, true);
            }
        }

        private static string _GetJWT()
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "admin"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.GroupSid, "Administrators"),
                new Claim(ClaimTypes.GroupSid, "Editors"),
            };

            var config = new ConfigurationBuilder().AddJsonFile("appsettings-test.json").Build();
            var key = RSA.BuildSigningKey(config["Tokens:PrivateRSAKey"].Resolve());
            var token = new JwtSecurityToken(
                config["Tokens:Issuer"],
                config["Tokens:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string _GetJWTNonAdmin()
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "user"),
                new Claim(ClaimTypes.Name, "User"),
                new Claim(ClaimTypes.GroupSid, "Guest"),
                new Claim(ClaimTypes.GroupSid, "User")
            };

            var config = new ConfigurationBuilder().AddJsonFile("appsettings-test.json").Build();
            var key = RSA.BuildSigningKey(config["Tokens:PrivateRSAKey"].Resolve());
            var token = new JwtSecurityToken(
                config["Tokens:Issuer"],
                config["Tokens:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}