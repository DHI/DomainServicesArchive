namespace JobAutomatorWinService
{
    using DHI.Services;
    using DHI.Services.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Threading;
    using System.Threading.Tasks;

    public class WindowsBackgroundService : BackgroundService
    {
        private readonly ILogger<WindowsBackgroundService> _logger;
        private readonly JobAutomator _jobAutomator;

        public WindowsBackgroundService(JobAutomator jobAutomator, ILogger<WindowsBackgroundService> logger)
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
            return base.StartAsync(cancellationToken);
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
