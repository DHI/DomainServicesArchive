namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Scenario entity type.
    /// </summary>
    [Serializable]
    public class Scenario : BaseNamedEntity<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Scenario" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="modelDataReaderId">The identifier for the model data reader for reading the simulation results.</param>
        /// <param name="parameterValues">
        ///     The input parameter values (keys and values).<br />
        ///     E.g. { "simulationStart", new DateTime(2021, 04, 28) }
        /// </param>
        /// <param name="inputTimeSeriesValues">
        ///     The input time series values (keys and time series identifiers)..<br />
        ///     E.g. {"gate1-level", "telemetry/reservoirs/gate1"}
        /// </param>
        /// <param name="metadata">The metadata.</param>
        [JsonConstructor]
        public Scenario(
            string id,
            string name,
            string modelDataReaderId,
            IDictionary<string, object>? parameterValues,
            IDictionary<string, string>? inputTimeSeriesValues,
            IDictionary<object, object>? metadata) : base(id, name)
        {
            Guard.Against.NullOrEmpty(modelDataReaderId, nameof(modelDataReaderId));
            ModelDataReaderId = modelDataReaderId;
            ParameterValues = parameterValues ?? new Dictionary<string, object>();
            InputTimeSeriesValues = inputTimeSeriesValues ?? new Dictionary<string, string>();
            Metadata = metadata ?? new Dictionary<object, object>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Scenario" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="modelDataReaderId">The identifier for the model data reader for reading the simulation results.</param>
        /// <param name="parameterValues">
        ///     The input parameter values (keys and values).<br />
        ///     E.g. { "simulationStart", new DateTime(2021, 04, 28) }
        /// </param>
        /// <param name="inputTimeSeriesValues">
        ///     The input time series values (keys and time series identifiers)..<br />
        ///     E.g. {"gate1-level", "telemetry/reservoirs/gate1"}
        /// </param>
        public Scenario(
            string id,
            string name,
            string modelDataReaderId,
            IDictionary<string, object>? parameterValues,
            IDictionary<string, string>? inputTimeSeriesValues) : this(id, name, modelDataReaderId, parameterValues, inputTimeSeriesValues, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Scenario" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="modelDataReaderId">The model data reader identifier.</param>
        public Scenario(string id, string name, string modelDataReaderId) : this(id, name, modelDataReaderId, null, null, null)
        {
        }

        /// <summary>
        ///     Gets the model data reader identifier.
        /// </summary>
        public string ModelDataReaderId { get; }

        /// <summary>
        ///     Gets the parameter values (keys and values).
        /// </summary>
        public IDictionary<string, object> ParameterValues { get; }

        /// <summary>
        ///     Gets the input time series values (keys and time series identifiers).
        /// </summary>
        /// <value>The input time series values.</value>
        public IDictionary<string, string> InputTimeSeriesValues { get; }

        /// <summary>
        ///     Gets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public new IDictionary<object, object> Metadata { get; }
    }
}