namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TimeSeries;

    /// <summary>
    ///     Interface for model output reader
    /// </summary>
    public interface IModelOutputReader
    {
        /// <summary>
        ///     Gets the list of available output time series (identifier and description).
        /// </summary>
        /// <returns>
        ///     IDictionary&lt;System.String, System.String&gt;.<br />
        ///     Example: { { "discharge-location1", "Discharge at location 1" }, { "discharge-location2", "Discharge at location 2" }, ... }
        /// </returns>
        IDictionary<string, string> GetOutputTimeSeriesList();

        /// <summary>
        ///     Gets the available simulations for the specified scenario.
        /// </summary>
        /// <param name="scenarioId">The scenario identifier.</param>
        /// <returns>IEnumerable&lt;Simulation&gt;.</returns>
        IEnumerable<Simulation> GetSimulations(string scenarioId);

        /// <summary>
        ///     Gets the values from the specified simulation for the specified output time series.
        /// </summary>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="timeSeriesKey">The time series key.</param>
        /// <example>
        /// <code>
        ///     var simulationId = new Guid("8f8f0105-e9e7-49a6-af15-174d230d915e");
        ///     var data = GetOutputTimeSeriesValues(simulationId, "discharge-location1");
        /// </code>
        /// </example>
        /// <returns>Task&lt;Maybe&lt;ITimeSeriesData&lt;System.Double&gt;&gt;&gt;.</returns>
        Task<Maybe<ITimeSeriesData<double>>> GetOutputTimeSeriesValues(Guid simulationId, string timeSeriesKey);
    }
}