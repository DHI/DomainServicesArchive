namespace DHI.Services.Models.WebApi.Host
{
    using System;

    public class FakeScenarioWorker : IScenarioWorker
    {
        public Guid Execute(Scenario scenario)
        {
            throw new NotImplementedException();
        }

        public void Cancel(Guid scenarioId)
        {
            throw new NotImplementedException();
        }
    }
}