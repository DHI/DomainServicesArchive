namespace DHI.JobOrchestratorService.Settings
{
    /// <summary>Provides a strongly typed implementation of appsettings.json</summary>
    public class AppSettings
    {
        public string? DataProtectionFolderPath { get; set; }

        public string? ApplicationName { get; set; }

        /// <summary>Set to true to enable verbose logging from within the load balancer.</summary>
        public bool VerboseLogging { get; set; } = false;

        public int ExecutionTimerIntervalInSeconds { get; set; } = 10;

        public int HeartbeatTimerIntervalInSeconds { get; set; } = 10;

        public int WorkflowCancelTimerIntervalInSeconds { get; set; } = 10;

        /// <summary>The default maximum duration of a job. May be overridden by task-specific maximum durations.</summary>
        public TimeSpan JobTimeout { get; set; }

        /// <summary>Jobs not started within this period will have their status set to Error.</summary>
        public TimeSpan StartTimeout { get; set; }

        /// <summary>Job records older than this timespan will be removed.</summary>
        public TimeSpan MaxAge { get; set; }

        public Dictionary<string, WorkerSettings> Workers { get; set; } = new Dictionary<string, WorkerSettings>();

        public string[]? HostGroups { get; set; }

        internal double ExecutionTimerInterval => this.ExecutionTimerIntervalInSeconds * 1000;

        internal double HeartbeatTimerInterval => this.HeartbeatTimerIntervalInSeconds * 1000;
    }
}
