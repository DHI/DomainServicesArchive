using DHI.JobOrchestrator.Docker;
using DHI.JobOrchestratorService.Settings;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Jobs.WorkflowWorker;
using Microsoft.AspNetCore.SignalR;

namespace DHI.JobOrchestratorService.Docker
{
    public class JobOrchestratorFactory
    {
        private readonly AppSettings _appSettings;
        private readonly Microsoft.Extensions.Logging.ILogger _mslogger;
        private readonly Dictionary<string, JobService<CodeWorkflow, string>> _jobServices;
        private readonly Dictionary<string, CodeWorkflowService> _workflowServices;
        private readonly IHubContext<WorkerHub> _hubContext;
        private readonly AvailableCache _availableCache;
        private readonly SignalRHostService _signalRHostService;
        private readonly System.Timers.Timer _timer;
        public JobOrchestratorFactory(
            AppSettings appSettings,
            Microsoft.Extensions.Logging.ILogger mslogger,
            Dictionary<string, JobService<CodeWorkflow, string>> jobServices,
            Dictionary<string, CodeWorkflowService> workflowServices,
            IHubContext<WorkerHub> hubContext,
            AvailableCache availableCache,
            SignalRHostService signalRHostService)
        {
            _appSettings = appSettings;
            _mslogger = mslogger;
            _jobServices = jobServices;
            _workflowServices = workflowServices;
            _hubContext = hubContext;
            _availableCache = availableCache;
            _signalRHostService = signalRHostService;
            _timer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 5000,
                Enabled = true
            };
        }

        public DHI.Services.Jobs.Orchestrator.JobOrchestrator Create()
        {
            var jobWorkers = new List<IJobWorker<string>>();
            // Create job workers
            // Get configuration values
            foreach (var workerSettingsPair in _appSettings.Workers)
            {
                var name = workerSettingsPair.Key;
                var environmentQualifiedName = workerSettingsPair.Value.HostGroup ?? workerSettingsPair.Value.EnvironmentQualifiedName(name);

                var worker = new SignalRWorkflowWorker(_hubContext, _availableCache, _mslogger);

                var loadBalancer = new LoadBalancer<CodeWorkflow, string>(environmentQualifiedName, worker, _jobServices[workerSettingsPair.Key], _signalRHostService, _appSettings.VerboseLogging ? _mslogger : null);

                var jobWorker = new JobWorker<CodeWorkflow, string>(
                    environmentQualifiedName,
                    worker,
                    _workflowServices[workerSettingsPair.Key],
                    _jobServices[workerSettingsPair.Key],
                    _signalRHostService,
                    loadBalancer,
                    heartbeatTimeout: System.TimeSpan.FromSeconds(60),
                    logger: _mslogger
                    );
                jobWorkers.Add(jobWorker);
                _timer.Elapsed += (s, e) => jobWorker.MonitorInProgressHeartbeat();
                _timer.Elapsed += (s, e) => jobWorker.MonitorTimeouts();
            }

            // Create job orchestrator                               
            return new DHI.Services.Jobs.Orchestrator.JobOrchestrator(jobWorkers, _mslogger, _appSettings.ExecutionTimerInterval, _appSettings.HeartbeatTimerInterval, _appSettings.WorkflowCancelTimerIntervalInSeconds);
        }
    }
}
