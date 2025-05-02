using DHI.JobOrchestratorService.Settings;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Jobs.WorkflowWorker;
using DHI.Services.Scalars;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace JobOrchestratorWinService
{
    internal static class JobOrchestratorFactory
    {
        public static DHI.Services.Jobs.Orchestrator.JobOrchestrator Create(AppSettings appSettings, IHubContext<WorkerHub> hubContext, AvailableCache availableCache, Microsoft.Extensions.Logging.ILogger logger)
        {
            var timer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 5000,
                Enabled = true
            };

            var workers = appSettings.Workers;

            var hostCollection = new SignalRHostRepository(logger);
            var signalRHostService = new SignalRHostService(hostCollection, appSettings.ValidHostGroups);
            var jobWorkers = new List<IJobWorker<string>>();

            //optionally add a dictionary to hold the job services if scalars are in use
            Dictionary<string, IJobService<string>> jobServices = null; // new Dictionary<string, IJobService<string>>();

            // Create job workers based on the configured set.
            foreach (var workerSettingsPair in workers)
            {
                var taskService = new CodeWorkflowService(new CodeWorkflowRepository(workerSettingsPair.Value.WorkflowRepositoryConnectionString));

                var environmentQualifiedName = workerSettingsPair.Value.HostGroup;

                var worker = new SignalRWorkflowWorker(hubContext, availableCache, logger);

                var jobRepository = new DHI.Services.Provider.DS.JobRepository(workerSettingsPair.Value.JobRepositoryConnectionString);
                var jobService = new JobService<CodeWorkflow, string>(jobRepository, taskService);

                //add the job services to the dictionary only if scalars are in use
                //jobServices.Add(workerSettingsPair.Value.HostGroup!, jobService);

                var loadBalancer = new LoadBalancer<CodeWorkflow, string>(environmentQualifiedName, worker, jobService, signalRHostService, appSettings.VerboseLogging ? logger : null);

                var jobWorker = new JobWorker<CodeWorkflow, string>(
                    environmentQualifiedName,
                    worker,
                    taskService,
                    jobService,
                    hostService: signalRHostService,
                    loadBalancer: loadBalancer,
                    timeout: appSettings.JobTimeout,
                    startTimeout: appSettings.StartTimeout,
                    maxAge: appSettings.MaxAge,
                    heartbeatTimeout: System.TimeSpan.FromSeconds(60 * 5),
                    logger: logger);

                jobWorkers.Add(jobWorker);
                timer.Elapsed += (s, e) => jobWorker.MonitorInProgressHeartbeat();
            }


            // Configure a scalar service (optional)
            GroupedScalarService? scalarService = null;

#warning Comment in if the scalar service should be used. The Scalar service enables updating of scalars such as the number of workflows running on the host etc. The scalar respository should in production systems be changed to e.g. the PostgreSQL based scalar repository
            // var scalarRepository = new ScalarRepository("scalars.json");

#warning Comment in to use the scalar service without logging
            // scalarService = new GroupedScalarService(scalarRepository, logger);

#warning Comment in to use the scalar service without logging
            // scalarService = new GroupedScalarService(scalarRepository)

            // Create job orchestrator
            return new DHI.Services.Jobs.Orchestrator.JobOrchestrator(jobWorkers, logger, appSettings.ExecutionTimerInterval, appSettings.HeartbeatTimerInterval, appSettings.TimeoutInterval, scalarService: scalarService, jobServices: jobServices);
        }
    }
}
