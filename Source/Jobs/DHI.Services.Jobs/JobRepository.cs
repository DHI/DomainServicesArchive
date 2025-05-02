namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    /// <summary>
    ///     Class JobRepository.
    /// </summary>
    /// <typeparam name="TJobId">The type of the job identifier.</typeparam>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    public class JobRepository<TJobId, TTaskId> : JsonRepository<Job<TJobId, TTaskId>, TJobId>, IJobRepository<TJobId, TTaskId>
    {
        private static readonly Func<JsonConverter[]> _requiredConverters = () =>
        {
            return new JsonConverter[]
            {
                new DictionaryTypeResolverConverter<Guid, Job<TJobId, TTaskId>>()
            };
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobRepository{TJobId, TTaskId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public JobRepository(string filePath)
            : base(filePath, _requiredConverters())
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File Not Found", filePath);
            }
        }

        /// <summary>
        ///     Gets all jobs.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;Job&lt;TJobId, TTaskId&gt;&gt;.</returns>
        public override IEnumerable<Job<TJobId, TTaskId>> GetAll(ClaimsPrincipal user = null)
        {
            return base.GetAll(user).OrderByDescending(j => j.Requested);
        }

        /// <summary>
        ///     Gets all jobs matching the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;Job&lt;TJobId, TTaskId&gt;&gt;.</returns>
        public IEnumerable<Job<TJobId, TTaskId>> Get(Query<Job<TJobId, TTaskId>> query, ClaimsPrincipal user = null)
        {
            return base.Get(query.ToExpression(), user).OrderByDescending(j => j.Requested);
        }

        /// <summary>
        ///     Gets the last (newest) job matching the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="user">The user.</param>
        /// <returns>Job&lt;TJobId, TTaskId&gt;.</returns>
        public Job<TJobId, TTaskId> GetLast(Query<Job<TJobId, TTaskId>> query, ClaimsPrincipal user = null)
        {
            return Get(query, user).FirstOrDefault();
        }

        /// <summary>
        ///     Removes the jobs matching the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="user">The user.</param>
        public void Remove(Query<Job<TJobId, TTaskId>> query, ClaimsPrincipal user = null)
        {
            Remove(query.ToExpression(), user);
        }

        /// <summary>
        ///     Gets all jobs matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;Job&lt;TJobId, TTaskId&gt;&gt;.</returns>
        public override IEnumerable<Job<TJobId, TTaskId>> Get(Expression<Func<Job<TJobId, TTaskId>, bool>> predicate, ClaimsPrincipal user = null)
        {
            return base.Get(predicate, user).OrderByDescending(j => j.Requested);
        }

        /// <summary>
        ///     Updates a single field if that field exists
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="jobId"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="user"></param>
        public void UpdateField<TField>(TJobId jobId, string fieldName, TField fieldValue, ClaimsPrincipal user = null)
        {
            var entity = base.Get(jobId, user);

            if (entity.HasValue)
            {
                var property = typeof(Job).GetProperty(fieldName);

                if (property != null)
                {
                    property.SetValue(entity.Value, fieldValue, null);
                    Update(entity.Value, user);
                }
            }
        }
    }

    public class JobRepository : JobRepository<Guid, string>
    {
        public JobRepository(string filePath) : base(filePath)
        {
        }
    }
}