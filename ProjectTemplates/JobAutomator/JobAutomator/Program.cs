using DHI.Services.Notifications;
using JobAutomator;
using JobAutomator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

var initialConfig = new ConfigurationBuilder().AddCommandLine(args).Build();
var environment = initialConfig.GetValue<string>("Environment")
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("Environment")
                  ?? "Prod";

Log.Information("Starting Job Automator in environment={Environment}", environment);

var builder = new ConfigurationBuilder()
             .AddCommandLine(args)
             .AddJsonFile("appsettings.json", false, false)
             .AddJsonFile($"appsettings.{environment}.json", true, false)
             .AddUserSecrets<Program>();

var configuration = builder.Build();

var seriLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

var msLogger = new SerilogLoggerProvider(seriLogger).CreateLogger(nameof(Program));
//var msLogger = new SimpleLogger("C:\\Work\\Code\\WaterForecastTree\\Trigger\\JobAutomator\\log.log");

var appSettings = new JobAutomatorSettings();
configuration.GetSection("JobAutomator").Bind(appSettings);

msLogger.LogInformation("Running with automator interval of {JobAutomatorInterval} seconds", appSettings.ExecutionIntervalSeconds);

try
{
    var runMode = configuration["RunMode"];
    IDependencyFactory dependencyFactory =
        string.Equals(runMode, "Local", StringComparison.OrdinalIgnoreCase)
            ? new LocalDependencyFactory(configuration, msLogger)
            : new DependencyFactory(configuration, msLogger);

    var jobsConnectionString = configuration.GetConnectionString("Jobs");
    if (string.IsNullOrWhiteSpace(jobsConnectionString))
    {
        msLogger.LogError("Domain Services connection string is missing");
        throw new Exception("Domain Services connection string is missing");
    }

    var jobService = dependencyFactory.GetJobService(jobsConnectionString);

    var automationConnectionString = configuration.GetConnectionString("Automations");
    if (string.IsNullOrWhiteSpace(automationConnectionString))
    {
        msLogger.LogError("Automations connection string is missing");
        throw new Exception("Automations connection string is missing");
    }

    var scalarsConnectionString = configuration.GetConnectionString("Scalars");
    if (string.IsNullOrWhiteSpace(scalarsConnectionString))
    {
        msLogger.LogError("Postgres connection string is missing");
        throw new Exception("Postgres connection string is missing");
    }

    var scalarService = dependencyFactory.GetScalarService(scalarsConnectionString);

    var tokenProvider = dependencyFactory.TokenProvider;

    var automationService = dependencyFactory.GetAutomationService(automationConnectionString, scalarsConnectionString, jobsConnectionString);

    var jobServiceFactory = dependencyFactory.GetJobServiceFactory();

    var jobAutomator = new JobAutomator.JobAutomator(msLogger, automationService, jobServiceFactory, scalarService, tokenProvider, appSettings);

    var applicationBuilder = Host.CreateApplicationBuilder(args);
    applicationBuilder.Services.AddWindowsService(options => { options.ServiceName = AutomatorBackgroundService.ServiceName; });
    applicationBuilder.Services.AddHostedService<AutomatorBackgroundService>();
    applicationBuilder.Services.AddSerilog(seriLogger);
    applicationBuilder.Services.AddSingleton(msLogger);
    applicationBuilder.Services.AddSingleton(jobAutomator);

    var host = applicationBuilder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Job Automator Failed");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
