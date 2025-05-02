namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TimeSeries;

    /// <summary>
    ///     Interface for model data reader
    /// </summary>
    public interface IModelDataReader : INamedEntity<string>, DHI.Services.ICloneable
    {
        /// <summary>
        ///     Gets the name of the type.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        ///     Gets the model type.
        /// </summary>
        string ModelType { get; set; }

        /// <summary>
        ///     Gets the list of available parameters (identifier and type).
        /// </summary>
        /// <returns>
        ///     IDictionary&lt;System.String, Type&gt;.<br />
        ///     Example: { { "simulationStart", typeof(DateTime) }, { "simulationEnd", typeof(DateTime) } }
        /// </returns>
        IDictionary<string, Type> GetParameterList();

        /// <summary>
        ///     Gets the value of the specified parameter.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter.</typeparam>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <example>
        /// <code>
        ///     var simulationStart = GetParameterValue&lt;DateTime&gt;("simulationStart");
        /// </code>
        /// </example>
        /// <returns>TParameter.</returns>
        TParameter GetParameterValue<TParameter>(string parameterId);

        /// <summary>
        ///     Gets the list of available input time series (key and description).
        /// </summary>
        /// <returns>
        ///     IDictionary&lt;System.String, System.String&gt;.<br />
        ///     Example: { { "gate1-level", "Level at gate1." }, { "gate2-level", "Level at gate2." }, ... }
        /// </returns>
        IDictionary<string, string> GetInputTimeSeriesList();

        /// <summary>
        ///     Gets the values for the specified input time series
        /// </summary>
        /// <param name="timeSeriesKey">The time series key.</param>
        /// <example>
        /// <code>
        ///     var data = GetInputTimeSeriesValues("gate1-level");
        /// </code>
        /// </example>
        /// <returns>Task&lt;ITimeSeriesData&lt;System.Double&gt;&gt;.</returns>
        Task<ITimeSeriesData<double>> GetInputTimeSeriesValues(string timeSeriesKey);

        /// <summary>
        ///     Gets the list of available output time series (key and description).
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
        /// <returns>Task&lt;ITimeSeriesData&lt;System.Double&gt;&gt;.</returns>
        Task<ITimeSeriesData<double>> GetOutputTimeSeriesValues(Guid simulationId, string timeSeriesKey);
    }
}