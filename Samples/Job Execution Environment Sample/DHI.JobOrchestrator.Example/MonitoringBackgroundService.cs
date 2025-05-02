using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DHI.JobOrchestrators
{
    public class MonitoringBackgroundService : BackgroundService
    {
        public const string ServiceName = "DHI Services Job Orchestrator";

        private readonly DHI.Services.Jobs.Orchestrator.JobOrchestrator _jobOrchestrator;


        public MonitoringBackgroundService(DHI.Services.Jobs.Orchestrator.JobOrchestrator jobOrchestrator)
        {
            _jobOrchestrator = jobOrchestrator;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _jobOrchestrator.Start();
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _jobOrchestrator.Stop();

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