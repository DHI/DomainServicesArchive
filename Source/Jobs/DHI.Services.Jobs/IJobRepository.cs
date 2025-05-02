namespace DHI.Services.Jobs
{
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface IJobRepository
    /// </summary>
    /// <typeparam name="TJobId">The type of the job identifier.</typeparam>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    public interface IJobRepository<TJobId, TTaskId> : IRepository<Job<TJobId, TTaskId>, TJobId>,
        IDiscreteRepository<Job<TJobId, TTaskId>, TJobId>,
        IUpdatableRepository<Job<TJobId, TTaskId>, TJobId>
    {
        /// <summary>
        /// Gets all jobs matching the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;Job&lt;TJobId, TTaskId&gt;&gt;.</returns>
        IEnumerable<Job<TJobId, TTaskId>> Get(Query<Job<TJobId, TTaskId>> query, ClaimsPrincipal user = null);

        /// <summary>
        /// Gets the last (newest) job matching the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="user">The user.</param>
        /// <returns>Job&lt;TJobId, TTaskId&gt;.</returns>
        Job<TJobId, TTaskId> GetLast(Query<Job<TJobId, TTaskId>> query, ClaimsPrincipal user = null);

        /// <summary>
        /// Removes the jobs matching the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="user">The user.</param>
        void Remove(Query<Job<TJobId, TTaskId>> query, ClaimsPrincipal user = null);

        /// <summary>
        /// Updates a single field if that field exists
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="jobId">The job id.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="user">The user.</param>
        void UpdateField<TField>(TJobId jobId, string fieldName, TField fieldValue, ClaimsPrincipal user = null);
    }
}