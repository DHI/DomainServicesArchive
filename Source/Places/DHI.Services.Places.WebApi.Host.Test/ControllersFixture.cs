namespace DHI.Services.Places.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Json;
    using DHI.Services.Places.Converters;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using WebApiCore;

    public class ControllersFixture : WebApplicationFactory<Program>
    {
        private const string AppDataPath = @"..\..\..\..\DHI.Services.Places.WebApi.Host\App_data";
        private readonly string _tempAppDataPath;
        private readonly string _tempContentRootPath;

        public ControllersFixture()
        {
            _tempContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _tempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(_tempAppDataPath);
            _CopyToTempAppDataPath("connections.json");
            _CopyToTempAppDataPath("places.json");
            _CopyToTempAppDataPath("timeseries.csv");
            _CopyToTempAppDataPath("SwaggerInfo.md");
            _CopyFolderToTempAppDataPath("shp");
            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());

            SerializerOptions = new JsonSerializerOptions(SerializerOptionsDefault.Options);
            SerializerOptions.AddConverters(new PlaceConverter<string>(),
                    new DataSourceConverter(),
                    new IndicatorConverter(),
                    new TimeIntervalConverter(),
                    new FeatureIdConverter<string>(),
                    new DHI.Services.Converters.DictionaryTypeResolverConverter<string, Place<string>>(isNestedDictionary: true));
        }

        public HttpClient Client { get; }

        public JsonSerializerOptions SerializerOptions { get; }

        public string TempContentRootPath => _tempContentRootPath;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("AppConfiguration:ContentRootPath", _tempContentRootPath);
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings-test.json");
            builder.ConfigureAppConfiguration(config => { config.AddJsonFile(configPath); });
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_tempContentRootPath, "App_Data"));

            base.ConfigureWebHost(builder);
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

        private void _CopyToTempAppDataPath(string sourceFileName, string sourceDir = AppDataPath)
        {
            var destinationFilePath = Path.Combine(_tempAppDataPath, sourceFileName);
            File.Copy(Path.Combine(sourceDir, sourceFileName), destinationFilePath);
            new FileInfo(destinationFilePath).IsReadOnly = false;
        }

        private void _CopyFolderToTempAppDataPath(string sourceFolderName, string sourceDir = AppDataPath)
        {
            var destinationFolderPath = Path.Combine(_tempAppDataPath, sourceFolderName);
            Directory.CreateDirectory(destinationFolderPath);
            var sourceFolder = new DirectoryInfo(Path.Combine(sourceDir, sourceFolderName));
            foreach (var file in sourceFolder.GetFiles())
            {
                file.CopyTo(Path.Combine(destinationFolderPath, file.Name));
                file.IsReadOnly = false;
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Directory.Exists(_tempContentRootPath))
            {
                Directory.Delete(_tempContentRootPath, true);
            }
        }
    }
}