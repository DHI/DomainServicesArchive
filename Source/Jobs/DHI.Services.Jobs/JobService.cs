using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Jobs.Test")]

namespace DHI.Services.Jobs
{
    using Accounts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;
    using Workflows;

    /// <summary>
    ///     Job Service
    /// </summary>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    /// <typeparam name="TTask">The type of the task</typeparam>
    public class JobService<TTask, TTaskId> : BaseUpdatableDiscreteService<Job<Guid, TTaskId>, Guid>, IJobService<TTaskId> where TTask : ITask<TTaskId>
    {
        private readonly IDiscreteService<Account, string> _accountService;
        private readonly IJobRepository<Guid, TTaskId> _repository;
        private readonly ITaskService<TTask, TTaskId> _taskService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobService{TTask, TTaskId}" /> class.
        /// </summary>
        /// <param name="repository">The job repository.</param>
        /// <param name="taskService">The task service.</param>
        /// <param name="accountService">The account service.</param>
        public JobService(IJobRepository<Guid, TTaskId> repository, ITaskService<TTask, TTaskId> taskService = null, IDiscreteService<Account, string> accountService = null)
            : base(repository)
        {
            _repository = repository;
            _taskService = taskService;
            _accountService = accountService;
        }

        /// <summary>
        ///     Occurs when multiple jobs have been deleted.
        /// </summary>
        public event EventHandler<EventArgs<List<QueryCondition>>> DeletedMultiple;

        /// <summary>
        ///     Occurs when deleting multiple jobs.
        /// </summary>
        public event EventHandler<CancelEventArgs<List<QueryCondition>>> DeletingMultiple;

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

        /// <inheritdoc />
        public override void Add(Job<Guid, TTaskId> job, ClaimsPrincipal user = null)
        {
            if (_accountService != null && job.AccountId != null && !_accountService.Exists(job.AccountId))
            {
                throw new KeyNotFoundException($"Account '{job.AccountId}' does not exist.");
            }

            if (_taskService != null)
            {
                if (!_taskService.Exists(job.TaskId))
                {
                    throw new KeyNotFoundException($"Task '{job.TaskId}' does not exist.");
                }

                if (job.Parameters != null)
                {
                    var task = _taskService.Get(job.TaskId);

                    foreach (var parameter in job.Parameters.Keys)
                    {
                        if (!task.Parameters.ContainsKey(parameter))
                        {
                            throw new KeyNotFoundException($"Parameter '{parameter}' does not exist in task '{task.Id}.");
                        }
                    }
                }
            }

            base.Add(job, user);
        }

        /// <inheritdoc />
        public override void Update(Job<Guid, TTaskId> job, ClaimsPrincipal user = null)
        {
            if (_accountService != null && job.AccountId != null && !_accountService.Exists(job.AccountId))
            {
                throw new KeyNotFoundException($"Account '{job.AccountId}' does not exist.");
            }

            if (_taskService != null)
            {
                if (!_taskService.Exists(job.TaskId))
                {
                    throw new KeyNotFoundException($"Task '{job.TaskId}' does not exist.");
                }

                if (job.Parameters != null)
                {
                    var task = _taskService.Get(job.TaskId);
                    foreach (var parameter in job.Parameters.Keys)
                    {
                        if (!task.Parameters.ContainsKey(parameter))
                        {
                            throw new KeyNotFoundException($"Parameter '{parameter}' does not exist in task '{task.Id}.");
                        }
                    }
                }
            }

            if (job.Status == JobStatus.Cancel)
            {
                if (!TryGet(job.Id, out var current))
                {
                    _logger?.LogError("Job '{JobId}' not found.", job.Id);
                    throw new KeyNotFoundException($"Job '{job.Id}' not found.");
                }

                // Starting is not included as we don't know if the host is there to receive the cancel event
                if (current.Status != JobStatus.InProgress && current.Status != JobStatus.Pending)
                {
                    throw new ArgumentException($"Current status of job '{current.Id}' is '{current.Status}'. " +
                                                $"You can only set job status to '{JobStatus.Cancel}' if current status is '{JobStatus.InProgress}' or '{JobStatus.Pending}'.", nameof(job));
                }
            }

            base.Update(job, user);
        }

        /// <inheritdoc />
        public void UpdateStatus(Guid jobId, JobStatus jobStatus, string statusMessage = null, int? progress = null, ClaimsPrincipal user = null)
        {
            if (!TryGet(jobId, out var job))
            {
                _logger?.LogError("Job '{JobId}' not found.", jobId);
                throw new KeyNotFoundException($"Could not find job with id: {jobId}");
            }

            switch (jobStatus)
            {
                case JobStatus.Pending:
                case JobStatus.Starting:
                case JobStatus.Unknown:
                case JobStatus.Error:
                    if (job.Status != JobStatus.Error)
                    {
                        job.Finished = DateTime.UtcNow;
                    }

                    break;
                case JobStatus.InProgress:
                    if (job.Status != JobStatus.InProgress)
                    {
                        job.Started = DateTime.UtcNow;
                    }

                    break;
                case JobStatus.Completed:
                    if (job.Status != JobStatus.Completed)
                    {
                        job.Finished = DateTime.UtcNow;
                    }

                    break;
                case JobStatus.Cancel:
                    // Starting is not included as we don't know if the host is there to receive the cancel event
                    if (job.Status != JobStatus.InProgress && job.Status != JobStatus.Pending)
                    {
                        throw new ArgumentException($"Current status of job '{job.Id}' is '{job.Status}'. " +
                                                    $"You can only set job status to '{JobStatus.Cancel}' if current status is '{JobStatus.InProgress}' or '{JobStatus.Pending}'.", nameof(jobStatus));
                    }

                    break;
                case JobStatus.Cancelling:
                    if (job.Status == JobStatus.TimingOut)
                    {
                        return;
                    }

                    break;
                case JobStatus.Cancelled:
                case JobStatus.TimingOut:
                    if (job.Status != JobStatus.TimingOut)
                    {
                        job.Finished = DateTime.UtcNow;
                    }
                    break;
                case JobStatus.TimedOut:
                    if (job.Status != JobStatus.TimedOut)
                    {
                        job.Finished = DateTime.UtcNow;
                    }
                    break;
                default:
                    break;
            }

            var cancelEventArgs = new CancelEventArgs<Job<Guid, TTaskId>>(job);
            OnUpdating(cancelEventArgs);
            if (!cancelEventArgs.Cancel)
            {
                job.Status = jobStatus;

                if (!string.IsNullOrEmpty(statusMessage))
                {
                    job.StatusMessage = statusMessage;
                }

                if (progress.HasValue)
                {
                    job.Progress = progress;
                }

                job.Updated = DateTime.UtcNow;
                _repository.Update(job, user);
                OnUpdated(job);
            }
        }

        public void UpdateHeartbeat(Guid jobId, ClaimsPrincipal user = null)
        {
            _repository.UpdateField<DateTime?>(jobId, nameof(Job.Heartbeat), DateTime.UtcNow, user);
        }

        /// <inheritdoc />
        public IEnumerable<Job<Guid, TTaskId>> Get(Query<Job<Guid, TTaskId>> query, ClaimsPrincipal user = null)
        {
            return _repository.Get(query, user);
        }

        /// <inheritdoc />
        public IEnumerable<Job<Guid, TTaskId>> Get(string accountId = null, DateTime? since = null, JobStatus? status = null, TTaskId taskId = default, string tag = null, ClaimsPrincipal user = null)
        {
            var query = BuildQuery(accountId, since, status: status, taskId: taskId, tag: tag);
            return query.Any() ? _repository.Get(query, user) : _repository.GetAll(user);
        }

        /// <inheritdoc />
        public Job<Guid, TTaskId> GetLast(string accountId = null, JobStatus? status = null, TTaskId taskId = default, string tag = null, ClaimsPrincipal user = null)
        {
            var query = BuildQuery(accountId, status: status, taskId: taskId, tag: tag);
            return query.Any() ? _repository.GetLast(query, user) : _repository.GetAll(user).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<Job<Guid, TTaskId>> GetJobsNotFinished(string hostId)
        {
            return GetJobsNotFinished().Where(job => job.HostId == hostId);
        }

        /// <inheritdoc />
        public IEnumerable<Job<Guid, TTaskId>> GetJobsNotFinished()
        {
            var jobsStarting = Get(status: JobStatus.Starting).ToArray();
            var jobsPending = Get(status: JobStatus.Pending).ToArray();
            var jobsInProgress = Get(status: JobStatus.InProgress).ToArray();
            var jobs = jobsStarting.Concat(jobsPending.Concat(jobsInProgress)).ToArray();
            return jobs;
        }

        /// <inheritdoc />
        public void Remove(string accountId = null, DateTime? before = null, TTaskId taskId = default, string tag = null, ClaimsPrincipal user = null)
        {
            var query = BuildQuery(accountId, before: before, taskId: taskId, tag: tag);
            if (query.Any())
            {
                var cancelEventArgs = new CancelEventArgs<List<QueryCondition>>(query.ToList());
                _OnDeletingMultiple(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Remove(query, user);
                    _OnDeletedMultiple(query.ToList());
                }
            }
        }

        private void _OnDeletedMultiple(List<QueryCondition> filter)
        {
            DeletedMultiple?.Invoke(this, new EventArgs<List<QueryCondition>>(filter));
        }

        private void _OnDeletingMultiple(CancelEventArgs<List<QueryCondition>> e)
        {
            DeletingMultiple?.Invoke(this, e);
        }

        internal static Query<Job<Guid, TTaskId>> BuildQuery(
            string accountId = null,
            DateTime? since = null,
            DateTime? before = null,
            JobStatus? status = null,
            TTaskId taskId = default,
            string tag = null)
        {
            var query = new Query<Job<Guid, TTaskId>>();
            if (accountId != null)
            {
                query.Add(new QueryCondition("AccountId", accountId));
            }

            if (since != null)
            {
                query.Add(new QueryCondition("Requested", QueryOperator.GreaterThanOrEqual, since));
            }

            if (before != null)
            {
                query.Add(new QueryCondition("Requested", QueryOperator.LessThan, before));
            }

            if (status != null)
            {
                query.Add(new QueryCondition("Status", status));
            }

            if (taskId != null)
            {
                query.Add(new QueryCondition("TaskId", taskId));
            }

            if (tag != null)
            {
                query.Add(new QueryCondition("Tag", tag));
            }

            return query;
        }
    }

    public class JobService : JobService<CodeWorkflow, string>
    {
        public JobService(IJobRepository<Guid, string> repository, ITaskService<CodeWorkflow, string> taskService = null, IDiscreteService<Account, string> accountService = null)
            : base(repository, taskService, accountService)
        {
        }
    }
}