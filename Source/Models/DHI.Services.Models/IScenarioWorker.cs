namespace DHI.Services.Models
{
    using System;

    /// <summary>
    ///     Interface for a scenario worker
    /// </summary>
    public interface IScenarioWorker
    {
        /// <summary>
        ///     Executes the specified scenario.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns>The simulation identifier.</returns>
        Guid Execute(Scenario scenario);

        /// <summary>
        ///     Cancels the specified simulation.
        /// </summary>
        /// <param name="simulationId">The simulation identifier.</param>
        void Cancel(Guid simulationId);
    }
}