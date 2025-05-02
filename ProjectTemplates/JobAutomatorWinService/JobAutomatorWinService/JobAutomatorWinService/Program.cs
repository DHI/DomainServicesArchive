using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Host = Microsoft.Extensions.Hosting.Host;
using JobAutomatorWinService;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Provider.DS;
using CodeWorkflowRepository = DHI.Services.Provider.DS.CodeWorkflowRepository;
using JobRepository = DHI.Services.Provider.DS.JobRepository;
using JobService = DHI.Services.Jobs.JobService;
using AutomationRepository = DHI.Services.Jobs.Automations.AutomationRepository;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(GetBasePath())
    .AddJsonFile("appsettings.json", false, true)
    .Build();
const int executionTimerIntervalInMilliseconds = 30 * 1000;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();
});

ILogger logger = loggerFactory.CreateLogger("Job Automator");
ILogger<JobAutomator> jobAutoLogger = loggerFactory.CreateLogger<JobAutomator>();

try
{
    string userName = configuration["AuthServer:UserName"];
    string password = configuration["AuthServer:Password"];
    string authServerUrl = configuration["AuthServer:Url"];
    string apiServerUrl = configuration["ApiServerUrl"];

    var tokenProvider = new AccessTokenProvider($"baseUrl={authServerUrl};userName={userName};password={password}", logger);

    // Tasks
    var taskRepository = new CodeWorkflowRepository($"{apiServerUrl}/api/tasks/wf-tasks", tokenProvider, 3, logger);
    var taskService = new CodeWorkflowService(taskRepository);

    // Jobs
    var jobRepository = new JobRepository($"{apiServerUrl}/api/jobs/wf-jobs", tokenProvider, 3, logger);
    var jobService = new JobService(jobRepository, taskService);

    // Automations
    var automationFile = configuration["AutomationFile"];
    var automationRepository = new AutomationRepository(automationFile);
    var automationService = new AutomationService(automationRepository);

    string postgres_scalar_connStr = configuration["postgres-scalar:ConnectionString"];
    var scalarRepository = new DHI.Services.Provider.PostgreSQL.ScalarRepository(postgres_scalar_connStr);
    var scalarService = new DHI.Services.Provider.PostgreSQL.ScalarService(scalarRepository, logger);

    var flag = configuration["EnableTriggerStatusLog"];
    bool enableTriggerStatusLog = (flag == null) ? false : bool.Parse(flag);

    var jobAutomator = new JobAutomator(jobAutoLogger, automationService, jobService, scalarService, executionTimerIntervalInMilliseconds, enableTriggerStatusLog);


    // Create the Windows service host
    using IHost host = Host.CreateDefaultBuilder(args)
        .UseWindowsService(options =>
        {
            options.ServiceName = WindowsBackgroundService.ServiceName;
        })
        .ConfigureServices(services =>
        {
            services.AddHostedService<WindowsBackgroundService>();
            services.AddScoped(_ => logger);
            services.AddScoped(_ => jobAutomator);
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    jobAutoLogger.LogError(ex, "Fatal error in Program");
    throw;
}

string? GetBasePath()
{
    using var processModule = Process.GetCurrentProcess().MainModule;
    return Path.GetDirectoryName(processModule?.FileName);
}
