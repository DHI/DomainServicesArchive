namespace DHI.Services.GIS.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Json;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using WebApiCore;

    public class ControllersFixture : WebApplicationFactory<Program>
    {
        private const string _appDataPath = @"..\..\..\..\..\DHI.Services.GIS.WebApi.Host\App_data";
        private readonly string _tempContentRootPath;

        public ControllersFixture()
        {
            _tempContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(TempAppDataPath);
            CopyFileToTempAppDataPath("SwaggerInfo.md");
            CopyFileToTempAppDataPath("KBHEC3dF012.dfsu");
            CopyFileToTempAppDataPath("MCSQLiteTest.sqlite");
            _CopyFolderToTempAppDataPath("dfs2");
            _CopyFolderToTempAppDataPath("shp");
            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());

            SerializerOptions = new JsonSerializerOptions(SerializerOptionsDefault.Options);
        }

        public HttpClient Client { get; }
        public JsonSerializerOptions SerializerOptions { get; }
        public string TempAppDataPath { get; }

        public void CopyFileToTempAppDataPath(string sourceFileName, string sourceDir = _appDataPath)
        {
            var destinationFilePath = Path.Combine(TempAppDataPath, sourceFileName);
            File.Copy(Path.Combine(sourceDir, sourceFileName), destinationFilePath, true);
            new FileInfo(destinationFilePath).IsReadOnly = false;
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
            if (Directory.Exists(_tempContentRootPath))
            {
                Directory.Delete(_tempContentRootPath, true);
            }

            base.Dispose(disposing);
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

        private void _CopyFolderToTempAppDataPath(string sourceFolderName, string sourceDir = _appDataPath)
        {
            var destinationFolderPath = Path.Combine(TempAppDataPath, sourceFolderName);
            Directory.CreateDirectory(destinationFolderPath);
            var sourceFolder = new DirectoryInfo(Path.Combine(sourceDir, sourceFolderName));
            foreach (var file in sourceFolder.GetFiles())
            {
                file.CopyTo(Path.Combine(destinationFolderPath, file.Name));
                file.IsReadOnly = false;
            }
        }
    }
}