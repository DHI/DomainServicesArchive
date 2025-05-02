using System.Diagnostics;
using System.Net;
using DHI.Services.Jobs.Workflows.Code;
using DHI.Services.Scalars;
using DHI.Workflow;
using DHI.Workflow.Host;
using DHI.Workflow.Host.Example;
using DHI.Workflow.Host.Interfaces;
using DHI.Workflow.Host.JobCancellationManagement;
using Microsoft.AspNetCore.SignalR.Client;
using static DHI.Workflow.Host.WindowsUpdate;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hbc, services) =>
    {

    // ---------------  SignalR Dependencies
    services.AddHostedService<ConnectionManager<string>>();
    // 
    services.AddSingleton(b =>
    {
        var model = new SignalRAuthentication.Config();
        b.GetRequiredService<IConfiguration>().GetSection("Tokens").Bind(model);
        return model;
    });

    services.AddSingleton<SignalRAuthentication>();

    services.AddSingleton<HubConnection>(sb =>
    {
        ServicePointManager.ServerCertificateValidationCallback = (context, certificate, chain, chainName) => { return true; };
        var config = sb.GetRequiredService<IConfiguration>();
        var url = config["JobOrchestratorUrl"];
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("JobOrchestratorUrl must be configured");
        }
        return new HubConnectionBuilder()
       .WithUrl(url, options =>
       {
           options.AccessTokenProvider = async () => await sb.GetRequiredService<SignalRAuthentication>().GenerateToken();
       })
       .Build();
    });

    services.AddSingleton<IWorkerHubClient, WorkerHubClient<CodeWorkflowDTO>>();

    // --------------- Create Logger
    services.AddLogging(lb =>
    {
        lb.AddConsole().AddSeq(serverUrl: "http://localhost:32404").SetMinimumLevel(LogLevel.Trace);
    });

    services.AddSingleton<ILogger>(s =>
        s.GetRequiredService<ILoggerFactory>().CreateLogger("WorkflowHost"));

    services.AddSingleton<StaticReleaseFile>(c => new StaticReleaseFile("../net6.0"));

    services.AddSingleton(b =>
    {
        var model = new HostAvailability.Config();
        b.GetRequiredService<IConfiguration>().GetSection("Availability").Bind(model);
        return model;
    });
    services.AddSingleton<HostAvailability>();
    services.AddSingleton<IHostAvailability>(c =>
    {
        var hostAvailability = c.GetRequiredService<HostAvailability>();
        hostAvailability.Start();
        return hostAvailability;
    });

    services.AddSingleton<Dictionary<string, ReleaseFile>>(c =>
    {
        var retry = c.GetRequiredService<IFunctionRetryPolicy>();
        var logger = c.GetRequiredService<ILogger>();

        var result = new Dictionary<string, ReleaseFile>
        {
             ["Static"] = c.GetRequiredService<StaticReleaseFile>(),
        };

        return result;
    });

    // --------------- Workflow Execution Engine Runner
    services.AddTransient<IWorkflowDeserializer<CodeWorkflowDTO>, CodeWorkflowDeserializer>();

    services.AddSingleton(b =>
    {
        var model = new EngineRunner.Config();
        b.GetRequiredService<IConfiguration>().GetSection("EngineRunner").Bind(model);
        return model;
    });
    services.AddSingleton<IEngineRunner, EngineRunner>();

    // -------------- Utilities
    services.AddSingleton<IFunctionRetryPolicy, FileAccessRetryPolicy>();

    services.AddSingleton(b =>
    {
        var model = new WindowsUpdate.Config();
        b.GetRequiredService<IConfiguration>().GetSection("WindowsUpdate").Bind(model);
        return model;
    });

    services.AddTransient<ISystemInformationWrapper, WindowsSystemInformation>();

    services.AddHostedService<WindowsUpdate>();

    // ----------- Job Cancellation management

    services.AddSingleton(c =>
    {
        var model = new PersistentJobCache.Config();
        c.GetRequiredService<IConfiguration>().GetSection("PersistentJobCache").Bind(model);
        return model;
    });
    services.AddSingleton<IPersistentJobCache, PersistentJobCache>();
    services.AddHostedService<PersistentJobCache>(c => (PersistentJobCache)c.GetRequiredService<IPersistentJobCache>());

    services.AddSingleton<JobCancellationManager.Config>((c) =>
    {
        var model = new JobCancellationManager.Config();
        c.GetRequiredService<IConfiguration>().GetSection("JobCancellationManager").Bind(model);
        return model;
    });

    services.AddSingleton<JobCancellationManager>();
    services.AddSingleton<IJobCancellationManager, JobCancellationManager>();
    services.AddHostedService<JobCancellationManager>(c => (JobCancellationManager)c.GetRequiredService<IJobCancellationManager>());
   
    // -------------- Scalar Service

    var scalarConfig = new UpdatesScalarService.Config();
    hbc.Configuration.GetSection("Scalars").Bind(scalarConfig);

    if (scalarConfig.Enabled)
    {
        services.AddSingleton<GroupedScalarService<string, int>>(c =>
            new GroupedScalarService<string, int>(new DHI.Services.Provider.PostgreSQL.ScalarRepository(c.GetRequiredService<IConfiguration>()["Scalars:ConnectionString"]))
            );
        services.AddSingleton(scalarConfig);
        services.AddSingleton<IUpdatesScalarService>(c => {
            var config = c.GetRequiredService<UpdatesScalarService.Config>();
            var releaseFiles = c.GetRequiredService<Dictionary<string, ReleaseFile>>();
            var groupedScalarService = c.GetRequiredService<GroupedScalarService<string, int>>();
            return new UpdatesScalarService(groupedScalarService, config, releaseFiles["Static"]);
        });
    }


}).Build();

using (host.Services.GetRequiredService<ILogger>().BeginScope(new Dictionary<string, object> {
                { "MachineName", Environment.MachineName },
                { "ProcessId", Process.GetCurrentProcess().Id },
                { "Unit", "JobHost" }
            }))
{
    var workerhubClient = host.Services.GetRequiredService<IWorkerHubClient>();
    var hubConnection = host.Services.GetRequiredService<HubConnection>();

    await workerhubClient.MapConnection(hubConnection);

    await host.RunAsync();
}
