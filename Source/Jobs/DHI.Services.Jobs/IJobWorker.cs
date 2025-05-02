namespace DHI.Services.Jobs;

using System;

public interface IJobWorker<TTaskId>
{
    /// <summary>
    ///     Gets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    string Id { get; }

    /// <summary>
    ///     Occurs when a job has been executed.
    /// </summary>
    event EventHandler<EventArgs<Tuple<Guid, JobStatus, TTaskId, string>>> Executed;

    /// <summary>
    ///     Occurs when executing a job.
    /// </summary>
    event EventHandler<EventArgs<Job<Guid, TTaskId>>> Executing;

    /// <summary>
    ///     Occurs when a job has been cancelled.
    /// </summary>
    event EventHandler<EventArgs<Tuple<Guid, string>>> Cancelled;

    /// <summary>
    ///     Occurs when cancelling a job.
    /// </summary>
    event EventHandler<EventArgs<Job<Guid, TTaskId>>> Cancelling;

    /// <summary>
    ///     Occurs when the progress of an executing job has changed.
    /// </summary>
    event EventHandler<EventArgs<Job<Guid, TTaskId>>> ProgressChanged;

    /// <summary>
    ///     Occurs when the overall heartbeat threshold has passed.
    /// </summary>
    event EventHandler<EventArgs<Job<Guid, TTaskId>>> OnHeartbeatThresholdPassed;

    /// <summary>
    ///     Changes the status of all jobs in progress exceeding the maximum duration to Error.
    ///     Job cancellation is attempted.
    /// </summary>
    void CleanLongRunningJobs();

    /// <summary>
    ///     Changes the status of all starting jobs to Error if they have not started within the timeout timespan.
    /// </summary>
    void CleanNotStartedJobs();

    /// <summary>
    ///     Executes the next pending job (if any).
    /// </summary>
    void ExecutePending();

    /// <summary>
    ///   Cancel the next job marked for cancelling (if any) 
    /// </summary>
    void Cancel();

    /// <summary>
    ///   Monitor in progress jobs for late heartbeat, and ste to Error in this case.
    /// </summary>
    void MonitorInProgressHeartbeat();

    /// <summary>
    ///  Monitor timeouts for jobs in progress.
    /// </summary>
    void MonitorTimeouts();
}