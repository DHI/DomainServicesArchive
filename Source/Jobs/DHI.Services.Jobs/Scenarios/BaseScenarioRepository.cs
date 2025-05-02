namespace DHI.Services.Jobs.Scenarios
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using DHI.Services;

    public abstract class BaseScenarioRepository : BaseDiscreteRepository<Scenario, string>, IScenarioRepository
    {
        public abstract IEnumerable<Scenario> Get(DateTime from, DateTime to, ClaimsPrincipal user = null);

        public abstract IEnumerable<Scenario> Get(Query<Scenario> query, ClaimsPrincipal user = null);

        public abstract void Add(Scenario entity, ClaimsPrincipal user = null);

        public abstract void Remove(string id, ClaimsPrincipal user = null);

        public abstract void Update(Scenario entity, ClaimsPrincipal user = null);
    }
}