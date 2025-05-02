namespace DHI.Services.Models
{
    using System;

    public interface IDerivedScenarioFactory
    {
        /// <summary>
        ///    Creates a derived scenario from an existing scenario based on data from the specified simulation.
        /// </summary>
        /// <param name="derivedScenarioName">The derived scenario name.</param>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The derived Scenario.</returns>
        Scenario Create(string derivedScenarioName, Guid simulationId, Parameters? parameters = null);
    }
}