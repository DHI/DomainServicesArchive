namespace DHI.Services.JsonDocuments.WebApi.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class JsonDocumentRepository : FakeGroupedRepository<JsonDocument<string>, string>, IJsonDocumentRepository<string>
    {
        public Maybe<JsonDocument<string>> Get(string id, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var documents = Get(id, user);
            return documents.HasValue ? documents.Value.Filter(dataSelectors).ToMaybe() : Maybe.Empty<JsonDocument<string>>();
        }

        public IEnumerable<JsonDocument<string>> Get(DateTime from, DateTime to, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var documents = _entities.Values.Where(s => s.DateTime > from && s.DateTime < to).ToArray();
            return documents.Select(document => document.Filter(dataSelectors)).ToList();
        }

        public IEnumerable<JsonDocument<string>> Get(Query<JsonDocument<string>> query, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var documents = Get(query.ToExpression());
            return documents.Select(document => document.Filter(dataSelectors)).ToList();
        }

        public IEnumerable<JsonDocument<string>> GetAll(string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var documents = GetAll(user);
            return documents.Select(document => document.Filter(dataSelectors)).ToList();
        }

        public IEnumerable<JsonDocument<string>> GetByGroup(string group, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var documents = GetByGroup(group, user);
            return documents.Select(document => document.Filter(dataSelectors)).ToList();
        }
    }
}