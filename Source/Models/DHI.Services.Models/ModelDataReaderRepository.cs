namespace DHI.Services.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     Json model data reader repository.
    /// </summary>
    public class ModelDataReaderRepository : JsonRepository<IModelDataReader, string>, IModelDataReaderRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReaderRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public ModelDataReaderRepository(string filePath) : base(filePath)
        {
        }

        public virtual IEnumerable<Scenario> GetScenarios(string id, ClaimsPrincipal user = null)
        {
            return new List<Scenario>();
        }

        public virtual IEnumerable<Simulation> GetSimulations(string id, ClaimsPrincipal user = null)
        {
            return new List<Simulation>();
        }
    }
}