using System;
using System.Diagnostics;
using System.IO;
using DHI.Services.Jobs.Orchestrator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JobOrchestratorWinService;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using DHI.Services.Jobs.WorkflowWorker;
using Microsoft.Extensions.Hosting.WindowsServices;
using DHI.JobOrchestratorService.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.EventLog;

ILogger? logger = null;

try
{
    // Get configuration values
    IConfiguration configuration = new ConfigurationBuilder().SetBasePath(GetBasePath()).AddJsonFile("appsettings.json", false, true).Build();

    var options = new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = WindowsServiceHelpers.IsWindowsService()
                                     ? AppContext.BaseDirectory : default
    };

    var builder = WebApplication.CreateBuilder(options);

#warning Select an appropriate logger. By default a Windows Event logger is configured. In production systems, a PostgreSQL based log repository or similar should be used
#warning When using multiple PostgreSQL log repositories, only the first will have database tables automatically created, so you may need to temporarily change the position of your first logger to have the table created.

    builder.Services.AddSingleton(context =>
    {
        var loggerFactory = LoggerFactory.Create(builder => { 
            var configuration = context.GetRequiredService<IConfiguration>();
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddEventLog(configuration.GetSection("Logging:EventLog").Get<EventLogSettings>());
        });
        return loggerFactory.CreateLogger("JobOrchestrator");
    });

    builder.Services.AddSingleton<JobOrchestrator>(c =>
    {
        var appSettings = new AppSettings();
        configuration.Bind(appSettings);

        var hubContext = c.GetRequiredService<IHubContext<WorkerHub>>();

        var availableCache = new AvailableCache();

        ILogger jobOrchestatorLogger = c.GetRequiredService<ILogger>();

        return JobOrchestratorFactory.Create(appSettings, hubContext, availableCache, jobOrchestatorLogger);
    });

    // job orchestrator signalr plumbing
    builder.Services.AddSignalR(o =>
    {
        o.EnableDetailedErrors = true;
    });
    builder.Services.AddSingleton<IUserIdProvider, MachineNameUserIdProvider>();
    builder.Services.AddSingleton<AvailableCache>();
    builder.Services.AddSingleton<ReportCache>();

#warning create a new RSA key pair for production use https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/faq/#how-to-create-a-pair-of-rsa-signing-keys-for-generation-and-validation-of-jwt-access-tokens
    builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Tokens:Issuer"],
            ValidAudience = configuration["Tokens:Audience"],
            IssuerSigningKey = RSA.BuildSigningKey(configuration["Tokens:PublicRSAKey"])
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Headers["Authorization"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/WorkerHub")))
                {
                    context.Token = accessToken.ToString().Replace("Bearer ", string.Empty);

                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddHostedService<WindowsBackgroundService>();

    builder.Host.UseWindowsService();

    var app = builder.Build();

    logger = app.Services.GetRequiredService<ILogger>();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHub<WorkerHub>("/WorkerHub");

    // Create the Windows service host
    await app.RunAsync();
}
catch (Exception e)
{    
    logger?.LogCritical(e, "JobOrchestrator fatal exception occured");

    throw;
}

string? GetBasePath()
{
    using var processModule = Process.GetCurrentProcess().MainModule;
    return Path.GetDirectoryName(processModule?.FileName);
}
