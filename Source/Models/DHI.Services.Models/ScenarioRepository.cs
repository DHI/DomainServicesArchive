namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Json scenario repository.
    /// </summary>
    public class ScenarioRepository : JsonRepository<Scenario, string>, IScenarioRepository
    {
        public ScenarioRepository(string filePath) : base(filePath)
        {
        }

        public ScenarioRepository(string filePath, IEnumerable<JsonConverter> converters) : base(filePath, converters) { }

        public ScenarioRepository(string filePath, JsonSerializerOptions jsonSerializerOptions) : base(filePath, jsonSerializerOptions)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                jsonSerializerOptions = jsonSerializerOptions ?? JsonSerializerOptions.Default;
            });
        }

        public IEnumerable<string> GetInputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Simulation GetLatestSimulation(string scenarioId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetOutputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<Guid, Guid> GetSimulationModelObjectAssociations(string simulationId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetSimulationModelObjectProperties(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Simulation[] GetSimulations(string scenarioId, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSimulationSchematics(string simulationId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }
}