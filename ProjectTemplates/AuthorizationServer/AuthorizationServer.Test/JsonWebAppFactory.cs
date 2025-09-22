using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DHI.Services.Accounts;
using DHI.Services.Authorization;
using DHI.Services.Authentication;
using DHI.Services.Mails;
using DHI.Services.Authentication.PasswordHistory;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using DHI.Services.Security.WebApi;
using DHI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthorizationServer.Test
{
    public class JsonWebAppFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var testProjectRoot = Path.GetFullPath("../../../", AppContext.BaseDirectory);
            builder.UseContentRoot(testProjectRoot);
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, cfg) =>
            {
                cfg.AddJsonFile("appsettings-test.json", optional: false, reloadOnChange: false);
            });

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("TestScheme")
                                            .RequireAuthenticatedUser()
                                            .Build();

                    options.AddPolicy("AdministratorsOnly",
                        p => p.AddAuthenticationSchemes("TestScheme")
                              .RequireClaim(ClaimTypes.GroupSid, "Administrators"));
                    options.AddPolicy("EditorsOnly",
                        p => p.AddAuthenticationSchemes("TestScheme")
                              .RequireClaim(ClaimTypes.GroupSid, "Editors"));
                });

                services.RemoveAll<IAccountRepository>();
                services.RemoveAll<IUserGroupRepository>();
                services.RemoveAll<IRefreshTokenRepository>();
                services.RemoveAll<IPasswordHistoryRepository>();
                services.RemoveAll<IAuthenticationProvider>();
                services.RemoveAll<IMailTemplateRepository>();

                services.AddScoped<IAccountRepository>(_ =>
                    new DHI.Services.Accounts.AccountRepository("accounts.json", SerializerOptionsDefault.Options, null, null, new LoginAttemptPolicy()));
                services.AddScoped<IUserGroupRepository>(_ =>
                    new DHI.Services.Security.WebApi.UserGroupRepository("user-groups.json", SerializerOptionsDefault.Options));
                services.AddScoped<IRefreshTokenRepository>(_ =>
                    new DHI.Services.Security.WebApi.RefreshTokenRepository("refresh-tokens.json", SerializerOptionsDefault.Options));
                services.AddScoped<IPasswordHistoryRepository>(_ =>
                    new DHI.Services.Authentication.PasswordHistory.PasswordHistoryRepository("passwordhistory.json", SerializerOptionsDefault.Options));
                services.AddScoped<IAuthenticationProvider>(_ =>
                    new DHI.Services.Accounts.AccountRepository("accounts.json", SerializerOptionsDefault.Options, null, null, new LoginAttemptPolicy()));
                services.AddScoped<IMailTemplateRepository>(_ =>
                    new DHI.Services.Security.WebApi.MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options));
            });
        }
    }
}
