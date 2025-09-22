namespace DHI.Services.TimeSeries.WebApi.Host.Test
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
        private const string _appDataPath = @"..\..\..\..\DHI.Services.TimeSeries.WebApi.Host\App_Data";
        private readonly string _tempAppDataPath;
        private readonly string _tempContentRootPath;

        public ControllersFixture()
        {
            _tempContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _tempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(_tempAppDataPath);
            Directory.CreateDirectory(Path.Combine(_tempAppDataPath, "csv"));
            Directory.CreateDirectory(Path.Combine(_tempAppDataPath, "csv\\mysubfolder"));
            _CopyToTempAppDataPath("SwaggerInfo.md");
            _CopyToTempAppDataPath("csv\\timeseries.csv");
            _CopyToTempAppDataPath("csv\\timeseries2.csv");
            _CopyToTempAppDataPath("csv\\timeseries3.csv");
            _CopyToTempAppDataPath("csv\\mysubfolder\\timeseries4.csv");
            _CopyToTempAppDataPath("csv\\mysubfolder\\timeseries5.csv");
            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());

            SerializerOptions = new JsonSerializerOptions(SerializerOptionsDefault.Options);
        }

        public HttpClient Client { get; }

        public JsonSerializerOptions SerializerOptions { get; }

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

        private void _CopyToTempAppDataPath(string sourceFileName, string sourceDir = _appDataPath)
        {
            var destinationFilePath = Path.Combine(_tempAppDataPath, sourceFileName);
            File.Copy(Path.Combine(sourceDir, sourceFileName), destinationFilePath);
            new FileInfo(destinationFilePath).IsReadOnly = false;
        }
    }
}