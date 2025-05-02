namespace DHI.Services.Models
{
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface for model data reader repository
    /// </summary>
    public interface IModelDataReaderRepository : IRepository<IModelDataReader, string>,
        IDiscreteRepository<IModelDataReader, string>,
        IUpdatableRepository<IModelDataReader, string>
    {
        IEnumerable<Scenario> GetScenarios(string id, ClaimsPrincipal user = null);
        IEnumerable<Simulation> GetSimulations(string id, ClaimsPrincipal user = null);
    }
}