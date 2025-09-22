using DHI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace JobAutomator
{
    public class AutomatorBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly JobAutomator _jobAutomator;

        public AutomatorBackgroundService(JobAutomator jobAutomator, ILogger<AutomatorBackgroundService> logger)
        {
            Guard.Against.Null(jobAutomator, nameof(jobAutomator));
            Guard.Against.Null(logger, nameof(logger));
            _jobAutomator = jobAutomator;
            _logger = logger;
        }

        public static string ServiceName => "DHI Job Automator";

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _jobAutomator.Start();
            _logger.LogInformation("Background service started.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _jobAutomator.Stop();
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
