namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TimeSeries;

    /// <summary>
    ///     Generic model data reader
    /// </summary>
    /// <typeparam name="TModelDataReader">The type of the model data reader.</typeparam>
    [Serializable]
    public class ModelDataReader<TModelDataReader> : BaseNamedEntity<string>, IModelDataReader where TModelDataReader : IModelInputReader, IModelOutputReader
    {
        private readonly TModelDataReader _modelDataReader;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReader{TModelDataReader}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public ModelDataReader(string id, string name) : base(id, name)
        {
            _modelDataReader = Activator.CreateInstance<TModelDataReader>();
        }

        /// <inheritdoc />
        public string TypeName => typeof(TModelDataReader).FullName;

        public string ModelType { get; set; }

        /// <inheritdoc />
        public IDictionary<string, Type> GetParameterList()
        {
            return _modelDataReader.GetParameterList();
        }

        /// <inheritdoc />
        public TParameter GetParameterValue<TParameter>(string parameterId)
        {
            Guard.Against.NullOrEmpty(parameterId, nameof(parameterId));
            if (!GetParameterList().ContainsKey(parameterId))
            {
                throw new KeyNotFoundException($"Parameter with id '{parameterId}' was not found.");
            }

            return _modelDataReader.GetParameterValue<TParameter>(parameterId);
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetInputTimeSeriesList()
        {
            return _modelDataReader.GetInputTimeSeriesList();
        }

        /// <inheritdoc />
        public async Task<ITimeSeriesData<double>> GetInputTimeSeriesValues(string timeSeriesKey)
        {
            Guard.Against.NullOrEmpty(timeSeriesKey, nameof(timeSeriesKey));
            if (!GetInputTimeSeriesList().ContainsKey(timeSeriesKey))
            {
                throw new KeyNotFoundException($"Time series with key '{timeSeriesKey}' was not found.");
            }

            return await _modelDataReader.GetInputTimeSeriesValues(timeSeriesKey);
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetOutputTimeSeriesList()
        {
            return _modelDataReader.GetOutputTimeSeriesList();
        }

        /// <inheritdoc />
        public IEnumerable<Simulation> GetSimulations(string scenarioId)
        {
            return _modelDataReader.GetSimulations(scenarioId);
        }

        /// <inheritdoc />
        public async Task<ITimeSeriesData<double>> GetOutputTimeSeriesValues(Guid simulationId, string timeSeriesKey)
        {
            Guard.Against.NullOrEmpty(timeSeriesKey, nameof(timeSeriesKey));
            if (!GetOutputTimeSeriesList().ContainsKey(timeSeriesKey))
            {
                throw new KeyNotFoundException($"Time series with key '{timeSeriesKey}' was not found.");
            }

            var maybe = await _modelDataReader.GetOutputTimeSeriesValues(simulationId, timeSeriesKey);
            if (maybe.HasValue)
            {
                return maybe.Value;
            }

            throw new Exception($"Output time series values could not be retrieved for time series with id '{timeSeriesKey}' for simulation with id '{simulationId}'.");
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}