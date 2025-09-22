namespace DHI.Services.Jobs.Automations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class ReadOnlyCompositeJobRepository : IJobRepository<Guid, string>
    {
        private readonly IEnumerable<IJobRepository<Guid, string>> _repositories;

        public ReadOnlyCompositeJobRepository(IEnumerable<IJobRepository<Guid, string>> repositories)
        {
            _repositories = repositories;
        }

        public Maybe<Job<Guid, string>> Get(Guid id, ClaimsPrincipal user = null)
        {
            foreach (var repo in _repositories)
            {
                var result = repo.Get(id, user);
                if (result.HasValue)
                    return result;
            }

            return Maybe.Empty<Job<Guid, string>>();
        }

        public IEnumerable<Job<Guid, string>> GetAll(ClaimsPrincipal user = null)
        {
            return _repositories.SelectMany(r => r.GetAll(user));
        }

        public IEnumerable<Job<Guid, string>> Get(Query<Job<Guid, string>> query, ClaimsPrincipal user = null)
        {
            return _repositories.SelectMany(r =>
            {
                try { return r.Get(query, user); }
                catch { return Enumerable.Empty<Job<Guid, string>>(); }
            });
        }

        public Job<Guid, string> GetLast(Query<Job<Guid, string>> query, ClaimsPrincipal user = null)
        {
            return _repositories
                .Select(r =>
                {
                    try { return r.GetLast(query, user); }
                    catch { return null; }
                })
                .Where(j => j != null)
                .OrderByDescending(j => j.Requested)
                .FirstOrDefault();
        }

        public IEnumerable<Guid> GetIds(ClaimsPrincipal user = null)
        {
            return _repositories.SelectMany(r => r.GetIds(user));
        }

        public int Count(ClaimsPrincipal user = null)
        {
            return _repositories.Sum(r => r.Count(user));
        }

        public bool Contains(Guid id, ClaimsPrincipal user = null)
        {
            return _repositories.Any(r => r.Contains(id, user));
        }

        public void Add(Job<Guid, string> entity, ClaimsPrincipal user = null) => throw new NotSupportedException("Add is not supported in read-only mode.");
        public void Remove(Guid id, ClaimsPrincipal user = null) => throw new NotSupportedException("Remove is not supported in read-only mode.");
        public void Update(Job<Guid, string> entity, ClaimsPrincipal user = null) => throw new NotSupportedException("Update is not supported in read-only mode.");
        public void Remove(Query<Job<Guid, string>> query, ClaimsPrincipal user = null) => throw new NotSupportedException("Remove is not supported in read-only mode.");
        public void UpdateField<TField>(Guid jobId, string fieldName, TField fieldValue, ClaimsPrincipal user = null) => throw new NotSupportedException("UpdateField is not supported in read-only mode.");
    }
}
