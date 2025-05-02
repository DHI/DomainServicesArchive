namespace DHI.Services.Jobs;

using System;
using System.Collections.Generic;
using System.Security.Claims;

public interface IJobService<TTaskId> :
    IService<Job<Guid, TTaskId>, Guid>,
    IDiscreteService<Job<Guid, TTaskId>, Guid>,
    IUpdatableService<Job<Guid, TTaskId>, Guid>
{
    /// <summary>
    ///     Updates the status of a job with the specified identifier.
    /// </summary>
    /// <remarks>
    ///     Bypasses validation of various job properties (AccountId, TaskId and Parameters)
    /// </remarks>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="jobStatus">The job status.</param>
    /// <param name="statusMessage">The job status message.</param>
    /// <param name="progress">The job progress.</param>
    /// <param name="user">The user.</param>
    void UpdateStatus(Guid jobId, JobStatus jobStatus, string statusMessage = null, int? progress = null, ClaimsPrincipal user = null);

    /// <summary>
    ///     Updates the heartbeat of a job with the specified identifier to server time now.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>    
    /// <param name="user">The user.</param>
    void UpdateHeartbeat(Guid jobId, ClaimsPrincipal user = null);

    /// <summary>
    ///     Gets the jobs meeting the criteria specified by the given query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="user">The user.</param>
    /// <returns>IEnumerable&lt;Job&lt;Guid, TTaskId&gt;&gt;.</returns>
    IEnumerable<Job<Guid, TTaskId>> Get(Query<Job<Guid, TTaskId>> query, ClaimsPrincipal user = null);

    /// <summary>
    ///     Gets the jobs meeting the criteria specified by the given parameters.
    /// </summary>
    /// <param name="accountId">an account identifier.</param>
    /// <param name="since">A date/time.</param>
    /// <param name="status">A job status.</param>
    /// <param name="taskId">A task identifier.</param>
    /// <param name="tag">A tag.</param>
    /// <param name="user">The user.</param>
    /// <returns>IEnumerable&lt;Job&lt;Guid, TTaskId&gt;&gt;.</returns>
    IEnumerable<Job<Guid, TTaskId>> Get(string accountId = null, DateTime? since = null, JobStatus? status = null, TTaskId taskId = default, string tag = null, ClaimsPrincipal user = null);

    /// <summary>
    ///     Gets the last job meeting the criteria specified by the given parameters.
    /// </summary>
    /// <param name="accountId">An account identifier.</param>
    /// <param name="status">A status.</param>
    /// <param name="taskId">A task identifier.</param>
    /// <param name="tag">A tag.</param>
    /// <param name="user">The user.</param>
    /// <returns>Job&lt;Guid, TTaskId&gt;.</returns>
    Job<Guid, TTaskId> GetLast(string accountId = null, JobStatus? status = null, TTaskId taskId = default, string tag = null, ClaimsPrincipal user = null);

    /// <summary>
    ///     Gets the jobs not finished (pending or in progress) on the host with the given ID.
    /// </summary>
    /// <param name="hostId">The host identifier.</param>
    /// <returns>IEnumerable&lt;Job&lt;Guid, TTaskId&gt;&gt;.</returns>
    IEnumerable<Job<Guid, TTaskId>> GetJobsNotFinished(string hostId);

    /// <summary>
    ///     Gets the jobs not finished (starting, pending or in progress).
    /// </summary>
    /// <returns>IEnumerable&lt;Job&lt;Guid, TTaskId&gt;&gt;.</returns>
    IEnumerable<Job<Guid, TTaskId>> GetJobsNotFinished();

    /// <summary>
    ///     Removes all the jobs meeting the criteria specified by the given parameters.
    /// </summary>
    /// <param name="accountId">An account identifier.</param>
    /// <param name="before">A date/time.</param>
    /// <param name="taskId">A task identifier.</param>
    /// <param name="tag">A tag.</param>
    /// <param name="user">The user.</param>
    void Remove(string accountId = null, DateTime? before = null, TTaskId taskId = default, string tag = null, ClaimsPrincipal user = null);
}