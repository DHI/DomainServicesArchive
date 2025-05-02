namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TimeSeries;

    /// <summary>
    ///     Interface for model input reader
    /// </summary>
    public interface IModelInputReader
    {
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
        ///     Gets the list of available input time series (identifier and description).
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
    }
}