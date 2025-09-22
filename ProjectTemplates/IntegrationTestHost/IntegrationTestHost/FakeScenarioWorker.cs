namespace BaseWebApi
{
    using DHI.Services.Models;

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
