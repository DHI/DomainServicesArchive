namespace DHI.Services.Models.WebApi.Host
{
    using System;

    public class ScenarioRepositoryWithFakeFactory : ScenarioRepository, IDerivedScenarioFactory
    {
        public ScenarioRepositoryWithFakeFactory(string filePath) : base(filePath)
        {
        }

        public Scenario Create(string derivedScenarioName, Guid simulationId, Parameters parameters = null)
        {
            return new Scenario($"ScenarioFromSimulation-{simulationId}", derivedScenarioName, "fakeReader", null, null, null);
        }
    }
}