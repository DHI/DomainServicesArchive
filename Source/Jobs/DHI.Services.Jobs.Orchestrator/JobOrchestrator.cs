namespace DHI.Services.Jobs.Orchestrator
{
    using Scalars;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Orchestration of job workers.
    /// </summary>
    /// <remarks>
    ///     If a scalarService (and a jobServices dictionary) is injected,
    ///     various scalar metrics, based on jobServices queries, will be calculated.
    /// </remarks>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    public class JobOrchestrator<TTaskId> : IDisposable
    {
        private readonly Timer _executionTimer;
        private readonly Timer _heartbeatTimer;
        private readonly Timer _timeoutTimer;
        private readonly IDictionary<string, IJobService<TTaskId>>? _jobServices;
        private readonly IEnumerable<IJobWorker<TTaskId>> _jobWorkers;
        private readonly ILogger _logger;
        private readonly GroupedScalarService<string, int>? _scalarService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobOrchestrator{TTaskId}"/> class.
        /// </summary>
        /// <param name="jobWorkers">The job workers.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="executionTimerInterval">The execution timer interval.</param>
        /// <param name="heartbeatTimerInterval">The heartbeat timer interval.</param>
        /// <param name="timeoutTimerInterval">The timeout timer interval.</param>
        public JobOrchestrator(IEnumerable<IJobWorker<TTaskId>> jobWorkers,
            ILogger logger,
            double executionTimerInterval,
            double heartbeatTimerInterval,
            double timeoutTimerInterval)
        {
            Guard.Against.NullOrEmpty(jobWorkers, nameof(jobWorkers));
            Guard.Against.Null(logger, nameof(logger));
            Guard.Against.NegativeOrZero(executionTimerInterval, nameof(executionTimerInterval));
            Guard.Against.NegativeOrZero(heartbeatTimerInterval, nameof(heartbeatTimerInterval));
            Guard.Against.NegativeOrZero(timeoutTimerInterval, nameof(timeoutTimerInterval));

            _jobWorkers = jobWorkers;
            _logger = logger;

            _executionTimer = new Timer { Interval = executionTimerInterval };
            _executionTimer.Elapsed += ExecutionTimerElapsed;

            _heartbeatTimer = new Timer { Interval = heartbeatTimerInterval };
            _heartbeatTimer.Elapsed += HeartbeatTimerElapsed;

            _timeoutTimer = new Timer { Interval = timeoutTimerInterval };
            _timeoutTimer.Elapsed += TimeoutTimerElapsed;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobOrchestrator{TTaskId}"/> class.
        /// </summary>
        /// <param name="jobWorkers">The job workers.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="executionTimerInterval">The execution timer interval.</param>
        /// <param name="heartbeatTimerInterval">The heartbeat timer interval.</param>
        /// <param name="timeoutTimerInterval">the timeout timer interval.</param>
        /// <param name="scalarService">A scalar service.</param>
        /// <param name="jobServices">A job services dictionary. The keys in the dictionary are IDs of the associated jobWorkers.</param>
        public JobOrchestrator(IEnumerable<IJobWorker<TTaskId>> jobWorkers,
            ILogger logger,
            double executionTimerInterval,
            double heartbeatTimerInterval,
            double timeoutTimerInterval,
            GroupedScalarService<string, int> scalarService,
            IDictionary<string, IJobService<TTaskId>> jobServices) : this(jobWorkers, logger, executionTimerInterval, heartbeatTimerInterval, timeoutTimerInterval)
        {
            Guard.Against.Null(scalarService, nameof(scalarService));
            Guard.Against.NullOrEmpty(jobServices, nameof(jobServices));
            _scalarService = scalarService;
            _jobServices = jobServices;
        }

        /// <summary>
        ///     Start the timers.
        /// </summary>
        public void Start()
        {
            try
            {
                _executionTimer.Start();
                _heartbeatTimer.Start();
                _timeoutTimer.Start();
                _logger.LogInformation("Timers started.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting timers.");
                throw;
            }
        }

        /// <summary>
        ///     Stop the timers.
        /// </summary>
        public void Stop()
        {
            try
            {
                _executionTimer.Stop();
                _heartbeatTimer.Stop();
                _timeoutTimer.Stop();
                _logger.LogInformation("Timers stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping timers.");
                throw;
            }
        }

        /// <summary>
        ///     Verifies if scalars are enabled.
        /// </summary>
        /// <returns><c>true</c> if scalars are enabled, <c>false</c> otherwise.</returns>
        public bool ScalarsEnabled()
        {
            return _scalarService is not null;
        }

        /// <summary>
        ///     Verifies if service is running (timers are started).
        /// </summary>
        /// <returns><c>true</c> if timers are enabled, <c>false</c> otherwise.</returns>
        public bool IsRunning()
        {
            return _executionTimer.Enabled;
        }

        private void ExecutionTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _executionTimer.Stop();
                Parallel.ForEach(_jobWorkers, jobWorker =>
                {
                    jobWorker.CleanNotStartedJobs();
                    jobWorker.ExecutePending();
                    jobWorker.Cancel();
                });

                if (_scalarService is not null)
                {
                    WriteScalars();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing jobs.");
            }
            finally
            {
                _executionTimer.Start();
            }
        }

        private void HeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _heartbeatTimer.Stop();
                Parallel.ForEach(_jobWorkers, jobWorker => { jobWorker.MonitorInProgressHeartbeat(); });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing heartbeat.");
            }
            finally
            {
                _heartbeatTimer.Start();
            }
        }

        private void TimeoutTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timeoutTimer.Stop();
                Parallel.ForEach(_jobWorkers, jobWorker => { jobWorker.MonitorTimeouts(); });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring time outs");
            }
            finally
            {
                _timeoutTimer.Start();
            }
        }

        private void WriteScalars()
        {
            foreach (var kvp in _jobServices!)
            {
                var jobWorkerId = kvp.Key;
                var jobService = kvp.Value;
                var scalarGroup = $"Job Orchestrator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{jobWorkerId}";

                var scalarName = "Jobs In Progress";
                var jobsInProgress = jobService.Get(status: JobStatus.InProgress);
                var scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsInProgress.Count(), DateTime.UtcNow));
                _scalarService?.TrySetDataOrAdd(scalar);

                scalarName = "Jobs Pending";
                var jobsPending = jobService.Get(status: JobStatus.Pending);
                scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsPending.Count(), DateTime.UtcNow));
                _scalarService?.TrySetDataOrAdd(scalar);

                scalarName = "Jobs Completed Last 24 Hours";
                var jobsCompleted24 = jobService.Get(status: JobStatus.Completed, since: DateTime.Now.AddDays(-1)).ToList();
                scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsCompleted24.Count, DateTime.UtcNow));
                _scalarService?.TrySetDataOrAdd(scalar);

                if (jobsCompleted24.Any())
                {
                    scalarName = "Average Execution Time (seconds) for Jobs Completed Last 24 Hours";
                    scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Double", scalarGroup, new ScalarData<int>(jobsCompleted24.Where(job => job.Started.HasValue && job.Finished.HasValue).Average(job => (job.Finished - job.Started)!.Value.TotalSeconds), DateTime.UtcNow));
                    _scalarService?.TrySetDataOrAdd(scalar);
                }

                scalarName = "Jobs With Errors Last 24 Hours";
                var jobsErrors24 = jobService.Get(status: JobStatus.Error, since: DateTime.Now.AddDays(-1));
                scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsErrors24.Count(), DateTime.UtcNow));
                _scalarService?.TrySetDataOrAdd(scalar);
            }
        }

        public void Dispose()
        {
            _executionTimer?.Dispose();
            _heartbeatTimer?.Dispose();
            _timeoutTimer?.Dispose();
        }
    }

    /// <inheritdoc />>
    public class JobOrchestrator : JobOrchestrator<string>
    {
        /// <inheritdoc />>
        public JobOrchestrator(IEnumerable<IJobWorker<string>> jobWorkers, ILogger logger, double executionTimerInterval, double heartbeatTimerInterval, double timeoutTimerInterval) : base(jobWorkers, logger, executionTimerInterval, heartbeatTimerInterval, timeoutTimerInterval)
        {
        }

        /// <inheritdoc />>
        public JobOrchestrator(IEnumerable<IJobWorker<string>> jobWorkers, ILogger logger, double executionTimerInterval, double heartbeatTimerInterval, double timeoutTimerInterval, GroupedScalarService<string, int> scalarService, IDictionary<string, IJobService<string>> jobServices) : base(jobWorkers, logger, executionTimerInterval, heartbeatTimerInterval, timeoutTimerInterval, scalarService, jobServices)
        {
        }
    }
}