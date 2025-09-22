namespace DHI.Services.Places.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
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
    using System.IdentityModel.Tokens.Jwt;
    using WebApiCore;
    using System.Security.Cryptography;
    using System.Text;

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
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings-test.json")
                .Build();
            var issuer = config["Tokens:Issuer"];
            var audience = config["Tokens:Audience"];
            var rsaKey = WebApiCore.RSA.BuildSigningKey(config["Tokens:PrivateRSAKey"].Resolve());
            var rsa = rsaKey.Rsa;

            var header = new Dictionary<string, object>
            {
                ["alg"] = SecurityAlgorithms.RsaSha256,
                ["typ"] = "JWT"
            };

            var now = DateTimeOffset.UtcNow;
            var payload = new Dictionary<string, object>
            {
                ["iss"] = issuer,
                ["aud"] = audience,
                ["sub"] = "john.doe",
                ["name"] = "john.doe",
                ["http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid"]
                         = new[] { "Administrators", "Editors" },
                ["nbf"] = now.ToUnixTimeSeconds(),
                ["exp"] = now.AddMinutes(5).ToUnixTimeSeconds()
            };

            static string ToBase64Url(object obj)
            {
                var json = JsonSerializer.Serialize(obj);
                var utf8Bytes = Encoding.UTF8.GetBytes(json);
                return Base64UrlEncoder.Encode(utf8Bytes);
            }

            var encodedHeader = ToBase64Url(header);
            var encodedPayload = ToBase64Url(payload);
            var signingInput = $"{encodedHeader}.{encodedPayload}";

            var signatureBytes = rsa.SignData(
                Encoding.UTF8.GetBytes(signingInput),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );
            var encodedSignature = Base64UrlEncoder.Encode(signatureBytes);

            return $"{signingInput}.{encodedSignature}";
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