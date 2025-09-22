namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using WebApiCore;

    public class ControllersFixture : WebApplicationFactory<Program>
    {
        private const string AppDataPath = @"../../../../DHI.Services.Jobs.WebApi.Host/App_data";
        private readonly string _tempContentRootPath;

        public ControllersFixture()
        {
            _tempContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(TempAppDataPath);
            CopyToTempAppDataPath("automations.json");
            CopyToTempAppDataPath("grouped_hosts.json");
            CopyToTempAppDataPath("jobs.json", Directory.GetCurrentDirectory());
            CopyToTempAppDataPath("jobs2.json");
            CopyToTempAppDataPath("workflows.json");
            CopyToTempAppDataPath("workflows2.json");
            CopyToTempAppDataPath("scenarios.json");
            CopyToTempAppDataPath("SwaggerInfo.md");
            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());
        }

        public HttpClient Client { get; }
        public string TempAppDataPath { get; }

        public void CopyToTempAppDataPath(string sourceFileName, string sourceDir = AppDataPath)
        {
            var destinationFilePath = Path.Combine(TempAppDataPath, sourceFileName);
            File.Copy(Path.Combine(sourceDir, sourceFileName), destinationFilePath);
            new FileInfo(destinationFilePath).IsReadOnly = false;
        }

        public void DeleteFromTempAppDataPath(string fileName)
        {
            var filePath = Path.Combine(TempAppDataPath, fileName);
            if (File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                if (info.IsReadOnly) info.IsReadOnly = false;
                info.Delete();
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("AppConfiguration:ContentRootPath", _tempContentRootPath);
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings-test.json");
            builder.ConfigureAppConfiguration(config => { config.AddJsonFile(configPath); });
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_tempContentRootPath, "App_Data"));

            base.ConfigureWebHost(builder);
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
                new Claim(JwtRegisteredClaimNames.Sub, "john.doe"),
                new Claim(ClaimTypes.Name, "john.doe"),
                new Claim(ClaimTypes.GroupSid, "Administrators"),
                new Claim(ClaimTypes.GroupSid, "Editors")
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