using DHI.Services;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Workflows;
using DHI.Services.Scalars;
using JobAutomator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Timers;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Scalar = DHI.Services.Provider.DS.Scalar;
using ScalarData = DHI.Services.Provider.DS.ScalarData;
using Timer = System.Timers.Timer;

namespace JobAutomator
{
    public class JobAutomator
    {
        private readonly Timer _executionTimer;
        private readonly ILogger _logger;
        private readonly AutomationService _automationService;
        private readonly ScalarService<int> _scalarService;
        private readonly JobAutomatorSettings _settings;
        private readonly AutomationExecutor _automationExecutor;
        private readonly AccessTokenProvider _tokenProvider;
        private readonly IJobServiceFactory _jobServiceFactory;
        private Timer _refreshTimer;
        private DateTime _lastCycleStartUtc = DateTime.MinValue;

        public JobAutomator(ILogger logger, AutomationService automationService, IJobServiceFactory jobServiceFactory,
                            ScalarService<int> scalarService, AccessTokenProvider tokenProvider, JobAutomatorSettings automationSettings)
        {
            Guard.Against.Null(logger, nameof(logger));
            Guard.Against.Null(automationService, nameof(automationService));
            Guard.Against.Null(jobServiceFactory, nameof(jobServiceFactory));
            Guard.Against.Null(scalarService, nameof(scalarService));
            Guard.Against.NegativeOrZero(automationSettings.ExecutionIntervalSeconds, nameof(automationSettings));
            Guard.Against.Null(tokenProvider, nameof(tokenProvider));

            _tokenProvider = tokenProvider;
            _logger = logger;
            _automationService = automationService;
            _jobServiceFactory = jobServiceFactory;
            _scalarService = scalarService;
            _settings = automationSettings;
            _automationExecutor = new AutomationExecutor(logger, scalarService, $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}");

            _executionTimer = new Timer { Interval = _settings.ExecutionIntervalSeconds * 1000 };
            _executionTimer.Elapsed += ExecutionTimerElapsed;
            _executionTimer.AutoReset = false;
            _executionTimer.Enabled = false;
        }

        public void Start()
        {
            try
            {
                _logger.LogInformation("Authenticating...");
                _tokenProvider.GetAccessToken().Wait();

                _executionTimer.Start();
                _logger.LogInformation("Execution timer started.");

                _refreshTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
                _refreshTimer.Elapsed += async (_, __) =>
                {
                    try
                    {
                        _logger.LogDebug("Refreshing access token...");
                        await _tokenProvider.GetAccessTokenInternal(forceRefresh: true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Token refresh failed.");
                    }
                };
                _refreshTimer.AutoReset = true;
                _refreshTimer.Enabled = true;
                _logger.LogInformation("Token refresh timer started.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start JobAutomator.");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _executionTimer?.Stop();
                _refreshTimer?.Stop();
                _logger.LogInformation("Timers stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during stop.");
            }
        }

        public bool IsRunning() => _executionTimer.Enabled;

        private void ExecutionTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            var cycleNow = DateTime.UtcNow;

            var bootstrapSeconds = 60.0;
            var cyclePrev = _lastCycleStartUtc == DateTime.MinValue
                ? cycleNow - TimeSpan.FromSeconds(bootstrapSeconds)
                : _lastCycleStartUtc;

            var runParams = new Dictionary<string, string>
            {
                ["utcNow"] = cycleNow.ToString("o", CultureInfo.InvariantCulture),
                ["utcPrev"] = cyclePrev.ToString("o", CultureInfo.InvariantCulture),
                ["toleranceSeconds"] = "30"
            };

            try
            {
                _executionTimer.Stop();

                var automations = _automationService.GetAll().ToArray();
                _logger.LogTrace("Looking at {Count} automations", automations.Length);

                foreach (var automation in automations)
                {
                    using var loggerScope = _logger.BeginScope(automation.Id);
                    try
                    {
                        _logger.LogTrace("Starting");
                        if (!automation.IsEnabled)
                        {
                            _logger.LogTrace("Is disabled, skipping");
                            continue;
                        }

                        var automationResult = _automationExecutor.Execute(automation, runParams);

                        if (_settings.LogTriggerStatus)
                            _logger.LogInformation("\"{AutomationId}\" Trigger condition met? {IsMet}", automation.Id, automationResult.IsMet);

                        var jobService = _jobServiceFactory.GetJobService(automation.HostGroup);

                        if (automationResult.IsMet)
                        {
                            var lastJobId = ReadLastJobIdScalar(automation.Id, automation.HostGroup);

                            if (lastJobId == null)
                            {
                                _logger.LogDebug("\"{AutomationId}\" No last job id found. Adding a new job to the queue.", automation.Id);
                                CreateNewJob(automation, automationResult, jobService);
                            }
                            else
                            {
                                if (!jobService.TryGet(lastJobId.Value, out var lastJob))
                                {
                                    _logger.LogError("Should have found a job with id {LastJobId}, but it was not found", lastJobId);
                                    continue;
                                }

                                var lastJobStatus = lastJob.Status;

                                if (lastJobStatus is JobStatus.Pending or JobStatus.InProgress or JobStatus.Starting)
                                {
                                    _logger.LogInformation("\"{AutomationId}\" Last job is still {LastJobStatus}. Not adding a new job to the queue.", automation.Id, lastJobStatus);
                                }
                                else
                                {
                                    _logger.LogDebug("\"{AutomationId}\" Last job is {LastJobStatus}. Adding a new job to the queue.", automation.Id, lastJobStatus);
                                    CreateNewJob(automation, automationResult, jobService);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "\"{AutomationId}\" failed to execute", automation.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job Automator failed");
            }
            finally
            {
                _lastCycleStartUtc = cycleNow;
                _executionTimer.Start();
            }
        }

        private void CreateNewJob(Automation<string> automation, AutomationResult automationResult, JobService<CodeWorkflow, string> jobService)
        {
            var job = new Job(Guid.NewGuid(), automation.TaskId);
            job.HostGroup = automation.HostGroup;
            job.Priority = automation.Priority;
            job.Tag = automation.Tag;

            // replace the default job.Tag from the automationResult job tag
            if (!string.IsNullOrEmpty(automationResult.JobTag))
            {
                job.Tag = automationResult.JobTag;
            }

            foreach (var (key, value) in automation.TaskParameters)
            {
                job.Parameters[key] = value;
            }

            foreach (var (key, value) in automationResult.TaskParameters)
            {
                job.Parameters[key] = value;
            }

            try
            {
                jobService.Add(job);
                WriteLastJobIdScalar(automation.Id, job.Id.ToString(), automation.HostGroup);
                _logger.LogInformation("Job added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding a job");
            }
        }

        private void WriteLastJobIdScalar(string automationId, string jobId, string hostId)
        {
            var hostIdOrEmpty = hostId.IsNullOrEmpty() ? "Empty" : hostId;
            var scalarGroup = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{automationId}";
            var scalarName = $"{hostIdOrEmpty}/Last Job Id";
            var scalarData = new ScalarData(jobId, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
            var scalar = new Scalar($"{scalarGroup}/{scalarName}", scalarName, "System.String", scalarGroup, scalarData);
            _scalarService?.TrySetDataOrAdd(scalar);
        }

        private Guid? ReadLastJobIdScalar(string automationId, string hostId)
        {
            var hostIdOrEmpty = hostId.IsNullOrEmpty() ? "Empty" : hostId;
            var scalarGroup = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{automationId}";
            var scalarName = $"{hostIdOrEmpty}/Last Job Id";
            var scalarId = $"{scalarGroup}/{scalarName}";
            if (!_scalarService.TryGet(scalarId, out var scalar))
                return null;

            var scalarData = scalar?.GetData();
            var scalarValue = (scalarData?.HasValue == true) ? scalarData.Value.Value.Value.ToString() : null;
            var succeed = Guid.TryParse(scalarValue, out var lastJobId);
            return succeed ? lastJobId : null;
        }
    }
}
