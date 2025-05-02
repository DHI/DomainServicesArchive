namespace DHI.Services.Jobs
{
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Workflows;

    /// <summary>
    ///     Job worker for job execution
    /// </summary>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    /// <typeparam name="TTask">The type of the task</typeparam>
    public class JobWorker<TTask, TTaskId> : IJobWorker<TTaskId> where TTask : ITask<TTaskId>
    {
        private readonly IHostService _hostService;
        private readonly JobService<TTask, TTaskId> _jobService;
        private readonly ILoadBalancer _loadBalancer;
        private readonly ILogger _logger;
        private readonly TimeSpan _maxAge;
        private readonly ITaskService<TTask, TTaskId> _taskService;
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _startTimeout;
        private readonly TimeSpan _heartbeatTimeout;
        private readonly IWorker<Guid, TTaskId> _worker;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobWorker{TTask, TTaskId}" /> class.
        /// </summary>
        /// <param name="id">An identifier of the JobWorker instance.</param>
        /// <param name="worker">The worker.</param>
        /// <param name="taskService">The task service.</param>
        /// <param name="jobService">The job service.</param>
        /// <param name="hostService">The host service.</param>
        /// <param name="loadBalancer">The load balancer used to delegate jobs to job hosts.</param>
        /// <param name="timeout">The default maximum duration of a job. May be overridden by task-specific maximum durations.</param>
        /// <param name="startTimeout">The default maximum duration from a host is asked to start a job until it returns the status InProgress</param>
        /// <param name="maxAge">The maximum time to keep a job record. Jobs older than this will be automatically removed from the job repository.</param>
        /// <param name="heartbeatTimeout">The maximum time a job can be in progress before the heartbeat record is updated.</param>
        /// <param name="logger">The logger.</param>
        public JobWorker(string id,
            IWorker<Guid, TTaskId> worker,
            ITaskService<TTask, TTaskId> taskService,
            JobService<TTask, TTaskId> jobService,
            IHostService hostService = null,
            ILoadBalancer loadBalancer = null,
            TimeSpan timeout = default,
            TimeSpan startTimeout = default,
            TimeSpan maxAge = default,
            TimeSpan heartbeatTimeout = default,
            ILogger logger = null)
        {
            Id = id;
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            _jobService.Added += JobService_Added;
            _jobService.Updated += JobService_Updated;
            _jobService.Deleted += JobService_Deleted;
            _jobService.DeletedMultiple += JobService_DeletedMultiple;
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));
            _worker.Executing += Worker_Executing;
            _worker.Executed += Worker_Executed;
            _worker.Cancelling += Worker_Cancelling;
            _worker.Cancelled += Worker_Cancelled;
            _worker.ProgressChanged += Worker_ProgressChanged;

            if (_worker is IRemoteWorker<Guid, string>)
            {
                ((IRemoteWorker<Guid, string>)worker).HostNotAvailable += Worker_HostNotAvailable;
            }

            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _hostService = hostService;
            _loadBalancer = _hostService is not null && loadBalancer is null ? new LoadBalancer<TTask, TTaskId>(id, worker, jobService, hostService) : loadBalancer;
            _startTimeout = startTimeout != default ? startTimeout : TimeSpan.FromMinutes(2);
            _timeout = timeout != default ? timeout : TimeSpan.FromHours(24);
            _maxAge = maxAge != default ? maxAge : TimeSpan.FromDays(30);
            _heartbeatTimeout = heartbeatTimeout != default ? heartbeatTimeout : TimeSpan.FromSeconds(15);
            if (logger != null)
            {
                _logger = logger;
            }
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public event EventHandler<EventArgs<Tuple<Guid, JobStatus, TTaskId, string>>> Executed;

        /// <inheritdoc />
        public event EventHandler<EventArgs<Job<Guid, TTaskId>>> Executing;

        /// <inheritdoc />
        public event EventHandler<EventArgs<Tuple<Guid, string>>> Cancelled;

        /// <inheritdoc />
        public event EventHandler<EventArgs<Job<Guid, TTaskId>>> Cancelling;

        /// <inheritdoc />
        public event EventHandler<EventArgs<Job<Guid, TTaskId>>> ProgressChanged;

        /// <inheritdoc />
        public event EventHandler<EventArgs<Job<Guid, TTaskId>>> OnHeartbeatThresholdPassed;

        /// <summary>
        ///     Gets an array of repository types compatible with this service.
        /// </summary>
        /// <param name="path">The path where to look for compatible repository types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetJobRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IJobRepository<Guid, TTaskId>>(path);
        }

        /// <summary>
        ///     Gets an array of task repository types compatible with the task service.
        /// </summary>
        /// <param name="path">The path where to look for compatible task repository types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetTaskRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<ITaskRepository<TTask, TTaskId>>(path);
        }

        /// <summary>
        ///     Gets an array of worker types compatible with the job service.
        /// </summary>
        /// <param name="path">The path where to look for compatible worker types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetWorkerTypes(string path = null)
        {
            return Service.GetProviderTypes<IWorker<Guid, TTaskId>>(path);
        }

        /// <summary>
        ///     Gets an array of host repository types compatible with the host service.
        /// </summary>
        /// <param name="path">The path where to look for compatible host repository types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetHostRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IHostRepository>(path);
        }

        /// <summary>
        ///     Gets an array of load balancer types.
        /// </summary>
        /// <param name="path">The path where to look for load balancer types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetLoadBalancerTypes(string path = null)
        {
            return Service.GetProviderTypes<ILoadBalancer>(path);
        }

        /// <inheritdoc />
        public void CleanLongRunningJobs()
        {
            var jobsInProgress = _jobService.Get(status: JobStatus.InProgress).ToArray();
            _logger?.LogInformation("Cleaning long running jobs. Current number of jobs in progress: {JobsInProgress}.", jobsInProgress.Length);
            foreach (var job in jobsInProgress)
            {
                var task = _taskService.Get(job.TaskId);
                var timeout = task.Timeout ?? _timeout;
                if (DateTime.UtcNow - job.Started!.Value > timeout)
                {
                    _logger?.LogWarning("Job '{JobId}' exceeded the timeout. Status will be set to Error.", job.Id);
                    _worker.Cancel(job.Id);
                    UpdateJobStatus(job.Id, JobStatus.Error, nameof(CleanLongRunningJobs), "Status set to Error by CleanLongRunningJobs process.");
                }
            }
        }

        /// <inheritdoc />
        public void CleanNotStartedJobs()
        {
            var jobsStarting = _jobService.Get(status: JobStatus.Starting).ToArray();
            _logger?.LogInformation("Cleaning jobs that has not started. Current number of starting jobs: {JobsStartingCount}.", jobsStarting.Length);
            foreach (var job in jobsStarting)
            {
                if (DateTime.UtcNow - job.Starting!.Value > _startTimeout)
                {
                    _logger?.LogWarning("Job '{JobId}' exceeded the start timeout. Status will be set to Error.", job.Id);
                    UpdateJobStatus(job.Id, JobStatus.Error, nameof(CleanNotStartedJobs), "Status set to Error by CleanNotStarted jobs process.");
                }
            }
        }

        /// <inheritdoc />
        public void MonitorInProgressHeartbeat()
        {
            var jobsInProgress = _jobService.Get(status: JobStatus.InProgress).ToArray();
            _logger?.LogDebug("Monitoring in progress jobs. Current number of InProgress jobs: {JobsInProgressCount}.", jobsInProgress.Length);

            foreach (var job in jobsInProgress)
            {
                if (!job.Heartbeat.HasValue || DateTime.UtcNow - job.Heartbeat!.Value > _heartbeatTimeout)
                {
                    _logger?.LogWarning("Job '{JobId}' exceeded the heartbeat timeout. Status will be set to Error.", job.Id);
                    UpdateJobStatus(job.Id, JobStatus.Error, nameof(MonitorInProgressHeartbeat), "Status set to Error by MonitorInProgressHeartbeat process.");
                    OnHeartbeatThresholdPassed?.Invoke(this, new EventArgs<Job<Guid, TTaskId>>(job));
                }
            }
        }

        /// <inheritdoc />
        public void MonitorTimeouts()
        {
            var jobsInProgress = _jobService.Get(status: JobStatus.InProgress).ToArray();
            if (!jobsInProgress.Any())
            {
                _logger?.LogDebug("No InProgress jobs.");
                return;
            }

            _logger?.LogDebug("Monitoring in progress jobs. Current number of InProgress jobs: {InProgressJobsCount}", jobsInProgress.Length);

            foreach (var job in jobsInProgress)
            {
                var task = _taskService.Get(job.TaskId);

                TimeSpan? timeout = null;

                if (task.Parameters != null &&
                    task.Parameters.ContainsKey("WorkflowTimeout") &&
                    task.Parameters["WorkflowTimeout"].ToString() == typeof(TimeSpan).FullName &&
                    job.Parameters != null &&
                    job.Parameters.ContainsKey("WorkflowTimeout") &&
                    TimeSpan.TryParse(job.Parameters["WorkflowTimeout"].ToString(), out var workflowTimeout))
                {
                    timeout = workflowTimeout;
                }
                else if (task.Timeout.HasValue && task.Timeout.Value > TimeSpan.FromMilliseconds(1)) //default timespan has a tiny non zero value
                {
                    timeout = task.Timeout.Value;
                }

                if (timeout.HasValue && DateTime.UtcNow - job.Started!.Value > timeout)
                {
                    _logger?.LogWarning("Job '{JobId}' exceeded the workflow timeout. Status will be set to TimedOut.", job.Id);
                    UpdateJobStatus(job.Id, JobStatus.TimedOut, nameof(MonitorTimeouts), "Status set to TimedOut by MonitorTimeouts process.");

                    _worker.Timeout(job.Id, job.HostId);
                }
            }
        }

        /// <inheritdoc />
        public void ExecutePending()
        {
            var pendingJobs = _jobService.Get(status: JobStatus.Pending).ToArray();
            if (!pendingJobs.Any())
            {
                _logger?.LogInformation("No pending jobs to execute.");
                return;
            }

            var jobs = pendingJobs.OrderBy(j => j.Priority).ThenBy(j => j.Requested).ToList();
            _logger?.LogInformation("Executing pending jobs. Current number of pending jobs: {JobsCount}. {JobsList}", jobs.Count, string.Join(", ", jobs.Select(job => $"'{job.Id}'")));
            foreach (var job in jobs)
            {
                if (!_taskService.Exists(job.TaskId))
                {
                    _logger?.LogError("Task with id '{TaskId}' was not found.", job.TaskId);
                    UpdateJobStatus(job.Id, JobStatus.Error, $"{nameof(ExecutePending)}: when task is not found", $"Status set to error as task {job.TaskId} was not found");
                    return;
                }

                var task = _taskService.Get(job.TaskId);

                if (_hostService == null)
                {
                    _logger?.LogInformation("No host service is available. Executing job '{JobId}' locally using '{WorkerType}'.", job.Id, _worker.GetType().FullName);
                    _worker.Execute(job.Id, task, job.Parameters);
                }
                else
                {
                    var hostGroup = job.HostGroup ?? task.HostGroup;

                    if (_hostService is IGroupedHostService hostService)
                    {
                        if (!hostService.GroupExists(hostGroup))
                        {
                            _logger?.LogError("HostGroup with id '{HostGroup}' was not found.", hostGroup);
                            UpdateJobStatus(job.Id, JobStatus.Error, $"{nameof(ExecutePending)}: when hostgroup is not found", $"Status set to error as hostgroup {hostGroup} was not found");
                            continue;
                        }
                    }

                    Host host;
                    // host unassigned
                    if (job.HostId is null)
                    {
                        host = _loadBalancer.GetHost(job.Id, hostGroup).Value;
                    }
                    else
                    {
                        if (_hostService is IGroupedHostService)
                        {
                            // grouped host previously assigned
                            // your host id must equal host name because a grouped query uses group and name
                            host = _hostService.Get($"{hostGroup}/{job.HostId}");
                        }
                        else
                        {
                            // ungrouped host previously assigned
                            host = _hostService.Get(job.HostId);
                        }
                    }

                    if (host is null)
                    {
                        job.Rejected = DateTime.UtcNow;
                        UpdateJob(job, $"{nameof(ExecutePending)}: when host is not available");
                        _logger.LogInformation("No host was available for job '{JobId}'. Still pending.", job.Id);
                        continue;
                    }

                    if (host.IsCloudInstance() &&
                        host.CloudInstanceHandler.GetStatus() != CloudInstanceStatus.Running &&
                        !((IRemoteWorker<Guid, string>)_worker).IsHostAvailable(host.Id))
                    {
                        job.HostId = host.Id;
                        job.Rejected = DateTime.UtcNow;
                        UpdateJob(job, $"{nameof(ExecutePending)}: when waiting for cloud host");
                        _logger?.LogInformation("Job '{JobId}' is waiting for host '{HostId}' cloud instance to be ready. Still pending.", job.Id, host.Id);
                    }
                    else
                    {
                        _logger?.LogInformation("Executing job '{JobId}' using '{WorkerType}' on host '{HostId}'.", job.Id, _worker.GetType().FullName, host.Id);
                        job.HostId = host.Id;
                        job.Status = JobStatus.Starting;
                        job.Starting = DateTime.UtcNow;
                        UpdateJob(job, nameof(ExecutePending));
                        _worker.Execute(job.Id, task, job.Parameters, host.Id);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Cancel()
        {
            var pendingCancellations = _jobService.Get(status: JobStatus.Cancel).ToArray();

            if (!pendingCancellations.Any())
            {
                _logger?.LogInformation("No pending cancellations.");
                return;
            }

            _logger?.LogInformation("Cancelling jobs. Current number of jobs to cancel: {PendingCancellationsCount}.", pendingCancellations.Length);
            var job = pendingCancellations.OrderBy(j => j.Requested).First();

            if (!job.Heartbeat.HasValue || DateTime.UtcNow - job.Heartbeat!.Value > _heartbeatTimeout)
            {
                _worker.Cancel(job.Id, job.HostId);
                _logger?.LogWarning("Job '{JobId}' exceeded the heartbeat timeout. Status will be set to Cancelled.", job.Id);
                UpdateJobStatus(job.Id, JobStatus.Cancelled, nameof(MonitorInProgressHeartbeat), "Status set to Cancelled by Cancel process due to heartbeat timeout or no heartbeat.");
            }
            else
            {
                _logger?.LogInformation("Cancelling job '{JobId}' on host '{HostId}'", job.Id, string.IsNullOrEmpty(job.HostId) ? job.HostId : "None");
                _worker.Cancel(job.Id, job.HostId);
            }
        }

        private void Worker_Executed(object sender, EventArgs<Tuple<Guid, JobStatus, string>> e)
        {
            var jobId = e.Item.Item1;
            var status = e.Item.Item2;

            _logger?.LogInformation("Job '{JobId}' executed. Status is '{Status}'.", jobId, status);
            var message = e.Item.Item3;
            if (!_jobService.TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            job.Status = status;
            job.Finished = DateTime.UtcNow;
            job.Progress = null;
            UpdateJob(job, nameof(Worker_Executed));
            if (!_jobService.GetJobsNotFinished().Any() && !(_hostService == null || _hostService.Count() == 0))
            {
                _logger?.LogInformation("No more jobs in progress or pending.");
                foreach (var host in _hostService.GetAll())
                {
                    if (host.CloudInstanceHandler?.GetStatus() == CloudInstanceStatus.Running)
                    {
                        _logger?.LogInformation("Stopping cloud instance '{HostId}'.", host.Id);
                        host.CloudInstanceHandler.Stop();
                    }
                }
            }

            Executed?.Invoke(this, new EventArgs<Tuple<Guid, JobStatus, TTaskId, string>>(new Tuple<Guid, JobStatus, TTaskId, string>(jobId, status, job.TaskId, message)));
        }

        private void Worker_Executing(object sender, EventArgs<Tuple<Guid, string>> e)
        {
            var jobId = e.Item.Item1;
            _logger?.LogInformation("Job '{JobId}' executing...", jobId);
            if (!_jobService.TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            job.Status = JobStatus.InProgress;
            job.Started = DateTime.UtcNow;
            UpdateJob(job, nameof(Worker_Executing));
            Executing?.Invoke(this, new EventArgs<Job<Guid, TTaskId>>(job));
        }

        private void Worker_Cancelled(object sender, EventArgs<Tuple<Guid, string>> e)
        {
            var jobId = e.Item.Item1;
            _logger?.LogInformation("Job '{JobId}' cancelled.", jobId);
            var message = e.Item.Item2;
            if (!_jobService.TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            job.Status = JobStatus.Cancelled;
            job.Progress = null;
            UpdateJob(job, nameof(Worker_Cancelled));
            if (!_jobService.GetJobsNotFinished().Any() && !(_hostService == null || _hostService.Count() == 0))
            {
                _logger?.LogInformation("No more jobs in progress or pending.");
                foreach (var host in _hostService.GetAll())
                {
                    if (host.CloudInstanceHandler?.GetStatus() == CloudInstanceStatus.Running)
                    {
                        _logger?.LogInformation("Stopping cloud instance '{HostId}'.", host.Id);
                    }
                }
            }

            Cancelled?.Invoke(this, new EventArgs<Tuple<Guid, string>>(new Tuple<Guid, string>(jobId, message)));
        }

        private void Worker_Cancelling(object sender, EventArgs<Guid> e)
        {
            var jobId = e.Item;
            _logger?.LogInformation("Job '{JobId}' cancelling...");
            if (!_jobService.TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            job.Status = JobStatus.Cancelling;
            UpdateJob(job, nameof(Worker_Cancelling));
            Cancelling?.Invoke(this, new EventArgs<Job<Guid, TTaskId>>(job));
        }

        private void Worker_HostNotAvailable(object sender, EventArgs<Guid> e)
        {
            var jobId = e.Item;
            _logger?.LogInformation("Host is not available. Job '{JobId}' is still pending.");
            if (!_jobService.TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            job.Status = JobStatus.Pending;
            job.Requested = DateTime.UtcNow;
            UpdateJob(job, nameof(Worker_HostNotAvailable));
        }

        private void Worker_ProgressChanged(object sender, EventArgs<Tuple<Guid, Progress>> e)
        {
            var jobId = e.Item.Item1;
            var progress = e.Item.Item2;
            if (!_jobService.TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            job.Progress = progress.Value;
            job.StatusMessage = progress.Message;
            UpdateJob(job, nameof(Worker_ProgressChanged));
            ProgressChanged?.Invoke(this, new EventArgs<Job<Guid, TTaskId>>(job));
        }

        private void JobService_Deleted(object sender, EventArgs<Guid> e)
        {
            _logger?.LogInformation("Job '{JobId}' is deleted.", e.Item);
        }

        private void JobService_DeletedMultiple(object sender, EventArgs<List<QueryCondition>> e)
        {
            var query = new Query<Job<Guid, TTaskId>>(e.Item);
            _logger?.LogInformation("Jobs meeting the criteria '{Query}' are deleted.", query);
        }

        private void JobService_Updated(object sender, EventArgs<Job<Guid, TTaskId>> e)
        {
            var job = e.Item;
            _logger?.LogInformation("Job '{JobId}' with status '{JobStatus}' and progress {JobProgress} updated.", job.Id, job.Status, job.Progress);
        }

        private void JobService_Added(object sender, EventArgs<Job<Guid, TTaskId>> e)
        {
            var job = e.Item;
            _logger?.LogInformation("Job '{JobId}' with status '{JobStatus}' added.", job.Id, job.Status);
        }

        private void UpdateJob(Job<Guid, TTaskId> job, string source)
        {
            try
            {
                _jobService.Update(job);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Exception in {source}.");
                throw;
            }
        }

        private void UpdateJobStatus(Guid jobId, JobStatus status, string source, string statusMessage)
        {
            try
            {
                _jobService.UpdateStatus(jobId, status);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Exception in {source}.");
                throw;
            }
        }
    }

    public class JobWorker : JobWorker<CodeWorkflow, string>
    {
        public JobWorker(string id,
            IWorker<Guid, string> worker,
            ITaskService<CodeWorkflow, string> taskService,
            JobService<CodeWorkflow, string> jobService,
            IHostService hostService = null,
            ILoadBalancer loadBalancer = null,
            TimeSpan timeout = default,
            TimeSpan startTimeout = default,
            TimeSpan maxAge = default,
            TimeSpan heartbeatTimeout = default,
            ILogger logger = null) : base(id, worker, taskService, jobService, hostService, loadBalancer, timeout, startTimeout, maxAge, heartbeatTimeout, logger)
        {
        }
    }
}