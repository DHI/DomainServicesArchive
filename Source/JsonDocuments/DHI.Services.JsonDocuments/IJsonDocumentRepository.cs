namespace DHI.Services.JsonDocuments
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IJsonDocumentRepository<TId> : IRepository<JsonDocument<TId>, TId>,
        IDiscreteRepository<JsonDocument<TId>, TId>,
        IGroupedRepository<JsonDocument<TId>>,
        IUpdatableRepository<JsonDocument<TId>, TId>
    {
        Maybe<JsonDocument<TId>> Get(TId id, string[] dataSelectors = null, ClaimsPrincipal user = null);

        IEnumerable<JsonDocument<TId>> GetByGroup(string group, string[] dataSelectors = null, ClaimsPrincipal user = null);

        IEnumerable<JsonDocument<TId>> GetAll(string[] dataSelectors = null, ClaimsPrincipal user = null);

        IEnumerable<JsonDocument<TId>> Get(DateTime from, DateTime to, string[] dataSelectors = null, ClaimsPrincipal user = null);

        IEnumerable<JsonDocument<TId>> Get(Query<JsonDocument<TId>> query, string[] dataSelectors = null, ClaimsPrincipal user = null);
    }
}