namespace DHI.Services.Jobs.Automations.Triggers;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

[Serializable]
public class JobCompletedTrigger<TJobId, TTaskId> : BaseTrigger
{
    private readonly IJobRepository<TJobId, TTaskId> _jobRepository;

    public JobCompletedTrigger(string id, TTaskId taskId, Type repositoryType, string connectionString) : base(id, $"Job of type '{taskId}' has completed.")
    {
        Guard.Against.Null(id, nameof(id));
        Guard.Against.Null(taskId, nameof(taskId));
        Guard.Against.Null(repositoryType, nameof(repositoryType));
        Guard.Against.Null(connectionString, nameof(connectionString));
        if (!typeof(IJobRepository<TJobId, TTaskId>).IsAssignableFrom(repositoryType))
        {
            throw new ArgumentException($"Repository type '{repositoryType}' does not implement interface '{nameof(IJobRepository<TJobId, TTaskId>)}'.", nameof(repositoryType));
        }

        TaskId = taskId;
        RepositoryType = repositoryType;
        ConnectionString = connectionString;

        try
        {
            _jobRepository = (IJobRepository<TJobId, TTaskId>)Activator.CreateInstance(repositoryType, connectionString.Resolve());
        }
        catch (TargetInvocationException ex)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
            throw;
        }
    }

    public TTaskId TaskId { get; }

    public Type RepositoryType { get; }

    public string ConnectionString { get; }

    public override AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null)
    {
        var query = new Query<Job<TJobId, TTaskId>>(new QueryCondition("TaskId", TaskId));
        var job = _jobRepository.GetLast(query);
        return job == null || job.Status != JobStatus.Completed
            ? AutomationResult.NotMet() 
            : AutomationResult.Met();
    }
}

public class JobCompletedTrigger : JobCompletedTrigger<Guid, string>, IJobCompletedTriggerParameters
{
    public JobCompletedTrigger(string id, string taskId, Type repositoryType, string connectionString) : base(id, taskId, repositoryType, connectionString)
    {
    }
}