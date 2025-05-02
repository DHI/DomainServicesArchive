namespace JobAutomatorWinService
{
    using DHI.Services;
    using DHI.Services.Jobs.Automations;
    using System.Timers;
    using DHI.Services.Jobs;
    using System;
    using ScalarService = DHI.Services.Provider.PostgreSQL.ScalarService;
    using DHI.Services.Provider.PostgreSQL;
    using Microsoft.Extensions.Logging;

    public class JobAutomator
    {
        private readonly Timer _executionTimer;
        private readonly ILogger<JobAutomator> _logger;
        private readonly AutomationService _automationService;
        private readonly JobService _jobService;
        private readonly ScalarService _scalarService;
        private readonly bool _enableTriggerStatusLog;
        private readonly AutomationExecutor _automationExecutor;

        public JobAutomator(ILogger<JobAutomator> logger, AutomationService automationService, JobService jobService, ScalarService scalarService, double executionTimerInterval, bool enableTriggerStatusLog)
        {
            Guard.Against.Null(logger, nameof(logger));
            Guard.Against.Null(automationService, nameof(automationService));
            Guard.Against.Null(jobService, nameof(jobService));
            Guard.Against.Null(scalarService, nameof(scalarService));
            Guard.Against.NegativeOrZero(executionTimerInterval, nameof(executionTimerInterval));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _automationService = automationService;
            _jobService = jobService;
            _scalarService = scalarService;
            _enableTriggerStatusLog = enableTriggerStatusLog;
            _automationExecutor = new AutomationExecutor(logger, scalarService, $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}");

            _executionTimer = new Timer { Interval = executionTimerInterval };
            _executionTimer.Elapsed += ExecutionTimerElapsed;
            _executionTimer.AutoReset = true;
            _executionTimer.Enabled = true;
        }

        public void Start()
        {
            try
            {
                _executionTimer.Start();
                _logger.LogInformation("Timer started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting timer");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _executionTimer.Stop();
                _logger.LogInformation("Timer stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping timer");
                throw;
            }
        }

        public bool IsRunning()
        {
            return _executionTimer.Enabled;
        }

        private void ExecutionTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                _executionTimer.Stop();
                var automations = _automationService.GetAll();

                foreach (var automation in automations)
                {
                    try
                    {
                        if (!automation.IsEnabled)
                        {
                            continue;
                        }

                        var automationResult = _automationExecutor.Execute(automation);

                        var isTriggerNow = false;
                        if (automation.Parameters is not null)
                        {
                            if (automation.Parameters.TryGetValue("triggerNow", out var value) && bool.TryParse(value, out isTriggerNow))
                            {
                                _logger.LogDebug("{AutomationId} Trigger-now override? {TriggerNow}", automation.Id, isTriggerNow);
                            }
                        }

                        if (_enableTriggerStatusLog)
                        {
                            _logger.LogInformation(
                                "{AutomationId} Condition met? {IsMet}, Trigger-now? {TriggerNow}",
                                automation.Id, automationResult.IsMet, isTriggerNow
                            );
                        }

                        if (automationResult.IsMet || isTriggerNow)
                        {
                            var lastJobId = ReadLastJobIdScalar(automation.Id);

                            if (lastJobId == null)
                            {
                                _logger.LogDebug("{AutomationId} No previous job; enqueuing new job", automation.Id);
                                CreateNewJob(automation);
                            }
                            else
                            {
                                var lastJob = _jobService.Get((Guid)lastJobId);
                                var lastJobStatus = lastJob.Status;

                                if (lastJobStatus is JobStatus.Pending or JobStatus.InProgress or JobStatus.Starting)
                                {
                                    _logger.LogInformation(
                                        "{AutomationId} Last job still {Status}; skipping enqueue",
                                        automation.Id, lastJob.Status
                                    );
                                }
                                else
                                {
                                    _logger.LogDebug(
                                        "{AutomationId} Last job {Status}; enqueuing new job",
                                        automation.Id, lastJob.Status
                                    );
                                    CreateNewJob(automation);
                                }
                            }
                        }

                        automation.Parameters = new Parameters { { "utcNow", $"{DateTime.UtcNow.ToString("HH:mm:ss")}" }, { "triggerNow", "false" } };
                        _automationService.Update(automation);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing automation {AutomationId}", automation.Id);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in timer loop");
            }
            finally
            {
                _executionTimer.Start();
            }
        }

        private void CreateNewJob(Automation<string> automation)
        {
            var job = new Job(Guid.NewGuid(), automation.TaskId, "RTN");
            job.HostGroup = automation.HostGroup;
            job.Priority = automation.Priority;

            foreach (var (key, value) in automation.TaskParameters)
            {
                job.Parameters[key] = value;
            }

            try
            {
                _jobService.Add(job);
                WriteLastJobIdScalar(automation.Id, job.Id.ToString());
                _logger.LogInformation("Job added; JobId={JobId}", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding job for automation {AutomationId}", automation.Id);
            }
        }

        private void WriteLastJobIdScalar(string automationId, string jobId)
        {
            var scalarGroup = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{automationId}";
            var scalarName = "Last Job Id";
            var scalarData = new ScalarData(jobId, DateTime.UtcNow);
            var scalar = new Scalar($"{scalarGroup}/{scalarName}", scalarName, "System.String", scalarGroup, scalarData);
            _scalarService?.TrySetDataOrAdd(scalar);
        }

        private Guid? ReadLastJobIdScalar(string automationId)
        {
            var scalarGroup = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{automationId}";
            var scalarName = "Last Job Id";
            var scalarId = $"{scalarGroup}/{scalarName}";
            var scalar = _scalarService.Get(scalarId);
            var scalarData = scalar?.GetData();
            var scalarValue = (scalarData?.HasValue == true) ? scalarData.Value.Value.Value.ToString() : null;
            var succeed = Guid.TryParse(scalarValue, out var lastJobId);
            return succeed ? lastJobId : null;
        }
    }
}