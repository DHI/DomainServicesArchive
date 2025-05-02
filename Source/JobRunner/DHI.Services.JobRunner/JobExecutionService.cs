namespace DHI.Services.JobRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using System.Timers;
    using Jobs;
    using Jobs.Workflows;
    using Logging;
    using Properties;
    using Scalars;

    public partial class JobExecutionService : ServiceBase
    {
        private readonly Timer _cleaningTimer;
        private readonly ILogger _logger;
        private readonly Timer _executionTimer;
        private readonly List<JobWorker<Workflow, string>> _jobWorkers;
        private readonly Dictionary<string, JobService<Workflow, string>> _jobServices;
        private readonly GroupedScalarService<string, int> _scalarService;

        public JobExecutionService(List<JobWorker<Workflow, string>> jobWorkers,
            ILogger logger,
            GroupedScalarService<string, int> scalarService = null,
            Dictionary<string, JobService<Workflow, string>> jobServices = null)
        {
            InitializeComponent();

            _jobWorkers = jobWorkers;
            _jobServices = jobServices;
            _scalarService = scalarService;
            _logger = logger;

            _executionTimer = new Timer { Interval = Settings.Default.ExecutionTimerIntervalInSeconds*1000 };
            _executionTimer.Elapsed += ExecutionTimerElapsed;

            if (Settings.Default.CleanningTimerIntervalInMinutes != 0)
            {
                _cleaningTimer = new Timer { Interval = Settings.Default.CleanningTimerIntervalInMinutes * 1000 * 60 };
                _cleaningTimer.Elapsed += CleaningTimerElapsed;
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _executionTimer.Start();
                _cleaningTimer?.Start();
                _logger.Log(new LogEntry(LogLevel.Information, "Timers restarted.", "Job Runner Service"));
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Error, ex.ToString(), "Job Runner Service"));
                throw;
            }
        }

        protected override void OnPause()
        {
            try
            {
                _executionTimer.Stop();
                _cleaningTimer?.Stop();
                _logger.Log(new LogEntry(LogLevel.Information, "Timers paused.", "Job Runner Service"));
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Error, ex.ToString(), "Job Runner Service"));
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _executionTimer.Start();
                _cleaningTimer?.Start();
                _logger.Log(new LogEntry(LogLevel.Information, "Service started.", "Job Runner Service"));
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Error, ex.ToString(), "Job Runner Service"));
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _executionTimer.Stop();
                _cleaningTimer?.Stop();
                _logger.Log(new LogEntry(LogLevel.Information, "Service stopped.", "Job Runner Service"));
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Error, ex.ToString(), "Job Runner Service"));
                throw;
            }
        }

        private void CleaningTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _logger.Log(new LogEntry(LogLevel.Information, $"Cleaning long running jobs for {_jobWorkers.Count} job worker(s).", "Job Runner Service"));
                _cleaningTimer.Stop();
                foreach (var jobWorker in _jobWorkers)
                {
                    jobWorker.CleanLongRunningJobs();
                    jobWorker.Dilute();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Error, ex.ToString(), "Job Runner Service"));
            }
            finally
            {
                _cleaningTimer.Start();
            }
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

                if (!(_scalarService is null))
                {
                    WriteScalars();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry(LogLevel.Error, ex.ToString(), "Job Runner Service"));
            }
            finally
            {
                _executionTimer.Start();
            }
        }

        private void WriteScalars()
        {
            foreach (var kvp in _jobServices)
            {
                var jobWorkerConnectionId = kvp.Key;
                var jobService = kvp.Value;
                var scalarGroup = $"Job Runner/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{jobWorkerConnectionId}";

                var scalarName = "Jobs In Progress";
                var jobsInProgress = jobService.Get(status: JobStatus.InProgress);
                var scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsInProgress.Count(), DateTime.UtcNow));
                _scalarService.TrySetDataOrAdd(scalar);

                scalarName = "Jobs Pending";
                var jobsPending = jobService.Get(status: JobStatus.Pending);
                scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsPending.Count(), DateTime.UtcNow));
                _scalarService.TrySetDataOrAdd(scalar);

                scalarName = "Jobs Completed Last 24 Hours";
                var jobsCompleted24 = jobService.Get(status: JobStatus.Completed, since: DateTime.Now.AddDays(-1)).ToList();
                scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsCompleted24.Count(), DateTime.UtcNow));
                _scalarService.TrySetDataOrAdd(scalar);

                if (jobsCompleted24.Any())
                {
                    scalarName = "Average Execution Time (seconds) for Jobs Completed Last 24 Hours";
                    scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Double", scalarGroup, new ScalarData<int>(jobsCompleted24.Where(job => job.Started.HasValue && job.Finished.HasValue).Average(job => (job.Finished - job.Started).Value.TotalSeconds), DateTime.UtcNow));
                    _scalarService.TrySetDataOrAdd(scalar);
                }

                scalarName = "Jobs With Errors Last 24 Hours";
                var jobsErrors24 = jobService.Get(status: JobStatus.Error, since: DateTime.Now.AddDays(-1));
                scalar = new Scalar<string, int>($"{scalarGroup}/{scalarName}", scalarName, "System.Int32", scalarGroup, new ScalarData<int>(jobsErrors24.Count(), DateTime.UtcNow));
                _scalarService.TrySetDataOrAdd(scalar);
            }
        }
    }
}