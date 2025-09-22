using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace DHI.Services.Notifications.WebApi.Host.Test
{
    public class ControllersFixture : WebApplicationFactory<Program>
    {
        private const string _appDataPath = @"..\..\..\..\DHI.Services.Notifications.WebApi.Host\App_data";
        private readonly string _tempContentRootPath;
        private readonly string _tempAppDataPath;

        public ControllersFixture()
        {
            _tempContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _tempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(_tempAppDataPath);
            _CopyToTempAppDataPath("SwaggerInfo.md");
            _CopyToTempAppDataPath("notifications.json");

            Client = CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _GetJWT());
        }

        public HttpClient Client { get; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("AppConfiguration:ContentRootPath", _tempContentRootPath);
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings-test.json");
            builder.ConfigureAppConfiguration(config => config.AddJsonFile(configPath));
            base.ConfigureWebHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Directory.Exists(_tempContentRootPath))
                Directory.Delete(_tempContentRootPath, true);
        }

        private static string _GetJWT()
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, "unit.test"),
            new Claim(ClaimTypes.Name, "unit.test"),
            new Claim(ClaimTypes.GroupSid, "Administrators"),
            new Claim(ClaimTypes.GroupSid, "Editors")
        };

            var config = new ConfigurationBuilder().AddJsonFile("appsettings-test.json").Build();
            var key = RSA.BuildSigningKey(config["Tokens:PrivateRSAKey"].Resolve());
            var token = new JwtSecurityToken(
                config["Tokens:Issuer"],
                config["Tokens:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.RsaSha256));
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
