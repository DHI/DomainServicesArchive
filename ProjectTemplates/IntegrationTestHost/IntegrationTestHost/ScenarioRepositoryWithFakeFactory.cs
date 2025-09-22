namespace BaseWebApi
{
    using DHI.Services;
    using DHI.Services.Models;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ScenarioRepositoryWithFakeFactory : ScenarioRepository, IDerivedScenarioFactory
    {
        public ScenarioRepositoryWithFakeFactory(string filePath) : base(filePath)
        {
        }

        public ScenarioRepositoryWithFakeFactory(string filePath, IEnumerable<JsonConverter> converters) : base(filePath, converters) { }

        public ScenarioRepositoryWithFakeFactory(string filePath, JsonSerializerOptions jsonSerializerOptions) : base(filePath, jsonSerializerOptions)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                jsonSerializerOptions = jsonSerializerOptions ?? JsonSerializerOptions.Default;
            });
        }

        public Scenario Create(string derivedScenarioName, Guid simulationId, Parameters? parameters = null)
        {
            return new Scenario($"ScenarioFromSimulation-{simulationId}", derivedScenarioName, "fakeReader", null, null, null);
        }
    }
}
