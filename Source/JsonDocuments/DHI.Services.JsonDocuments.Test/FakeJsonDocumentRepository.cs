namespace DHI.Services.JsonDocuments.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    internal class FakeJsonDocumentRepository : FakeGroupedRepository<JsonDocument<string>, string>, IJsonDocumentRepository<string>
    {
        public FakeJsonDocumentRepository()
        {
        }

        public FakeJsonDocumentRepository(IEnumerable<JsonDocument<string>> jsonDocuments)
            : base(jsonDocuments)
        {
        }

        public IEnumerable<JsonDocument<string>> Get(DateTime from, DateTime to, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            return _entities.Values.Where(s => s.DateTime > from && s.DateTime < to).ToArray();
        }

        public Maybe<JsonDocument<string>> Get(string id, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            return Get(id, user);
        }

        public IEnumerable<JsonDocument<string>> Get(Query<JsonDocument<string>> query, string[] __ = null, ClaimsPrincipal ___ = null)
        {
            return Get(query.ToExpression());
        }

        public IEnumerable<JsonDocument<string>> GetAll(string[] _ = null, ClaimsPrincipal user = null)
        {
            return GetAll(user);
        }

        public IEnumerable<JsonDocument<string>> GetByGroup(string group, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user);
        }
    }
}