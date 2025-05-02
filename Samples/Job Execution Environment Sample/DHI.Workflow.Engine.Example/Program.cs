using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DHI.Services.Jobs.Workflows.Code;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DHI.Workflow.Engine.Template
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Thread.Sleep(10000);
            var initialConfig = new ConfigurationBuilder().AddCommandLine(args).Build();
            var environment = initialConfig.GetValue<string>("Environment") ?? Environment.GetEnvironmentVariable("Environment");

            var releaseFolderName = new DirectoryInfo(Assembly.GetEntryAssembly().Location).Parent.Name;

            var builder = new ConfigurationBuilder().AddCommandLine(args)                
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{environment}.json", true, false)
				.AddUserSecrets<Program>();

            var config = builder.Build();
            
            var jsonFileName = config["WorkflowDefinitionFile"];

            if (!File.Exists(jsonFileName))
            {
                throw new Exception($"The workflow file {jsonFileName} does not exist");
            }

            var jobApiConfig = new JobApiStateUpdater.Config();
            var heartbeatConfig = new Heartbeat.Config();
            config.GetSection("Api").Bind(jobApiConfig);
            config.GetSection("Api").Bind(heartbeatConfig);

            var authConfig = new BearerAuthenticator.AuthenticatorConfig();
            config.GetSection("Authentication").Bind(authConfig);

            var authenticator = new BearerAuthenticator(authConfig, new HttpClientFactory());
            var authenticatedHttpClientFactory = new AuthenticatedHttpClientFactory<BearerAuthenticator>(authenticator);
            
            var workflowDtoReader = new WorkflowDtoReader<CodeWorkflowDTO>(jsonFileName);

            var workflowDto = workflowDtoReader.Get();

            using (var loggerFactory = LoggerFactory.Create(l => l
                .AddConsole().AddSeq(serverUrl: "http://localhost:32404")))
            {
                var logger = loggerFactory.CreateLogger(nameof(CodeWorkflowEngine.CodeWorkflowEngine));
                
                using (var child = logger.BeginScope(new Dictionary<string, object> {
                { "MachineName", Environment.MachineName },
                { "ProcessId", Process.GetCurrentProcess().Id },
                { "JobId", workflowDto.JobId },
                { "Release", releaseFolderName }
                }))
                {
                    JobApiStateUpdater httpJobStateUpdater = null;
                    try
                    {
                        var cancellationTokenSource = new CancellationTokenSource();

                        httpJobStateUpdater = new JobApiStateUpdater(jobApiConfig, authenticatedHttpClientFactory, workflowDto, logger, cancellationTokenSource.Token);

                        var jsonFileInfo = new FileInfo(jsonFileName);
                        var killFile = jsonFileInfo.FullName.Replace(jsonFileInfo.Extension, ".kill");

                        using (var cancellationMonitor = new KillFileMonitor(killFile, cancellationTokenSource, httpJobStateUpdater, logger, 1000))
                        using (var heartbeat = new Heartbeat(heartbeatConfig, authenticatedHttpClientFactory, cancellationTokenSource, logger))
                        {
                            cancellationMonitor.Start();
                            heartbeat.Start(workflowDto.JobId);

                            var workflowEngine = new DHI.Workflow.CodeWorkflowEngine.CodeWorkflowEngine(
                                httpJobStateUpdater, 
                                workflowDtoReader, 
                                logger, 
                                cancellationMonitor,
                                cancellationTokenSource.Token,
                                loopOnCancel: true);

							workflowEngine.Run();
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        httpJobStateUpdater?.OnFailed($"Job execution failed: {ex.Message}");
                        logger.LogCritical(ex, "Job execution failed {jobDefinition} on {machineName}.", jsonFileName, Environment.MachineName);
                        Environment.Exit(1);
                    }
                }
            }
        }
    }
}

