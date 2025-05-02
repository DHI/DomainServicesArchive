namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Scenarios;

    internal class FakeScenarioRepository : FakeRepository<Scenario, string>, IScenarioRepository
    {
        public FakeScenarioRepository()
        {
        }

        public FakeScenarioRepository(IEnumerable<Scenario> scenarioList)
            : base(scenarioList)
        {
        }

        public IEnumerable<Scenario> Get(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _entities.Values.Where(s => s.DateTime > from && s.DateTime < to).ToArray();
        }

        public IEnumerable<Scenario> Get(Query<Scenario> query, ClaimsPrincipal user = null)
        {
            return Get(query.ToExpression());
        }
    }
}