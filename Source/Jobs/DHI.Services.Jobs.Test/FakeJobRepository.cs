namespace DHI.Services.Jobs.Test
{
    using Jobs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    internal class FakeJobRepository : FakeRepository<Job<Guid, string>, Guid>, IJobRepository<Guid, string>
    {
        public FakeJobRepository()
        {
        }

        public FakeJobRepository(List<Job<Guid, string>> jobList)
            : base(jobList)
        {
        }

        public IEnumerable<Job<Guid, string>> Get(Query<Job<Guid, string>> query, ClaimsPrincipal user = null)
        {
            return Get(query.ToExpression());
        }

        public Job<Guid, string> GetLast(Query<Job<Guid, string>> query, ClaimsPrincipal user = null)
        {
            return Get(query).OrderByDescending(j => j.Requested).FirstOrDefault();
        }

        public void Remove(Query<Job<Guid, string>> query, ClaimsPrincipal user = null)
        {
            Remove(query.ToExpression());
        }

        public void UpdateField<TField>(Guid jobId, string fieldName, TField fieldValue, ClaimsPrincipal user = null)
        {
        }
    }
}