namespace JobOrchestratorWinService
{
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;
    using DHI.Services.Jobs.Orchestrator;
    using Microsoft.Extensions.Logging;

    public class WindowsBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly JobOrchestrator _jobOrchestrator;
        
        public WindowsBackgroundService(JobOrchestrator jobOrchestrator, ILogger logger)
        {
            _jobOrchestrator = jobOrchestrator;
            _logger = logger;
            var scalarsStatus = _jobOrchestrator.ScalarsEnabled() ? "enabled" : "disabled";
            _logger.LogInformation("Scalars are {scalarsStatus}.", scalarsStatus);
        }

        public static string ServiceName => "DHI Job Orchestrator";

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _jobOrchestrator.Start();
            _logger.LogInformation("Background service started.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _jobOrchestrator.Stop();
            _logger.LogInformation("Background service stopped.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
