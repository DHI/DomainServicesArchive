namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface for scenario repository
    /// </summary>
    public interface IScenarioRepository : IRepository<Scenario, string>,
        IDiscreteRepository<Scenario, string>,
        IUpdatableRepository<Scenario, string>
    {
        Simulation[] GetSimulations(string scenarioId, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null);
        Simulation GetLatestSimulation(string scenarioId, ClaimsPrincipal user = null);
        IEnumerable<string> GetSimulationSchematics(string simulationId, ClaimsPrincipal user = null);
        IDictionary<Guid, Guid> GetSimulationModelObjectAssociations(string simulationId, ClaimsPrincipal user = null);
        IEnumerable<string> GetInputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null);
        IEnumerable<string> GetOutputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null);
        IDictionary<string, object> GetSimulationModelObjectProperties(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null);
    }
}