using JobOrchestratorWinService;
using System;
using System.Collections.Generic;

namespace DHI.JobOrchestratorService.Settings
{
    /// <summary>Provides a strongly typed implementation of appsettings.json</summary>
    public class AppSettings
    {
        /// <summary>Set to true to enable verbose logging from within the load balancer.</summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Interval at which the job queue will be processed.
        /// </summary>
        public int ExecutionTimerIntervalInSeconds { get; set; } = 10;

        /// <summary>
        /// Interval at which the job orchestrator will check for heartbeat timeouts.
        /// </summary>
        public int HeartbeatTimerIntervalInSeconds { get; set; } = 10;

        /// <summary>
        /// TimeoutInterval to check timeout in seconds.
        /// </summary>
        public int TimeoutIntervalInSeconds { get; set; } = 10;

        /// <summary>The default maximum duration of a job. May be overridden by task-specific maximum durations.</summary>
        public TimeSpan JobTimeout { get; set; }

        /// <summary>Jobs not started within this period will have their status set to Error.</summary>
        public TimeSpan StartTimeout { get; set; }

        /// <summary>Job records older than this timespan will be removed.</summary>
        public TimeSpan MaxAge { get; set; }

        /// <summary>
        /// The set of job workers to be created by the orchestrator. 
        /// </summary>
        public Dictionary<string, WorkerSettings> Workers { get; set; } = new Dictionary<string, WorkerSettings>();

        /// <summary>
        /// The set of valid host groups that the orchestrator will accept.
        /// </summary>
        public string[]? ValidHostGroups { get; set; }

        internal double ExecutionTimerInterval => this.ExecutionTimerIntervalInSeconds * 1000;

        internal double HeartbeatTimerInterval => this.HeartbeatTimerIntervalInSeconds * 1000;

        internal double TimeoutInterval => this.TimeoutIntervalInSeconds * 1000;
    }
}
