namespace AuthorizationServer.Test
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using DHI.Services.Accounts;
    using DHI.Services.Authentication;
    using DHI.Services.Authentication.PasswordHistory;
    using DHI.Services.Authorization;
    using DHI.Services.Mails;
    using DHI.Services.Security.WebApi;
    using System.Linq;
    using DHI.Services;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using System.Security.Claims;

    public class PostgreWebAppFactory : WebApplicationFactory<Program>
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
                services.RemoveAll<IAuthenticationProvider>();
                services.RemoveAll<IPasswordHistoryRepository>();
                services.RemoveAll<IMailTemplateRepository>();

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings-test.json")
                    .Build();

                var connectionString = Environment.GetEnvironmentVariable("TEST_PG_CONN") ?? "Server=localhost;Port=5432;Username=postgres;Password=Solutions!;Database=ProviderTest;";

                services.AddScoped<IAccountRepository>(_ =>
                    new DHI.Services.Provider.PostgreSQL.AccountRepository(
                        connectionString,
                        null,
                        SerializerOptionsDefault.Options.Converters,
                        new LoginAttemptPolicy()
                    ));

                services.AddScoped<IUserGroupRepository>(_ =>
                    new DHI.Services.Provider.PostgreSQL.UserGroupRepository(connectionString, null));

                services.AddScoped<IRefreshTokenRepository>(_ =>
                    new DHI.Services.Provider.PostgreSQL.RefreshTokenRepository(connectionString, null));

                services.AddScoped<IAuthenticationProvider>(_ =>
                    new DHI.Services.Provider.PostgreSQL.AccountRepository(
                        connectionString,
                        null,
                        SerializerOptionsDefault.Options.Converters,
                        new LoginAttemptPolicy()
                    ));

                services.AddScoped<IPasswordHistoryRepository>(_ =>
                    new DHI.Services.Provider.PostgreSQL.PasswordHistoryRepository(
                        connectionString,
                        null,
                        SerializerOptionsDefault.Options.Converters
                    ));

                services.AddScoped<IMailTemplateRepository>(_ =>
                    new DHI.Services.Security.WebApi.MailTemplateRepository("mail-templates.json", SerializerOptionsDefault.Options));
            });
        }
    }
}
