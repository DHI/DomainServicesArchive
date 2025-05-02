namespace DHI.Services.Jobs.Scenarios
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using DHI.Services;

    public interface IScenarioRepository : IRepository<Scenario, string>, IDiscreteRepository<Scenario, string>, IUpdatableRepository<Scenario, string>
    {
        IEnumerable<Scenario> Get(DateTime from, DateTime to, ClaimsPrincipal user = null);

        IEnumerable<Scenario> Get(Query<Scenario> query, ClaimsPrincipal user = null);
    }
}