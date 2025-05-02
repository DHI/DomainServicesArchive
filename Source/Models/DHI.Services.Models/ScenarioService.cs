namespace DHI.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Argon;
    using TimeSeries;

    /// <summary>
    ///     Scenario service.
    /// </summary>
    public class ScenarioService : BaseUpdatableDiscreteService<Scenario, string>
    {
        private readonly ModelDataReaderService _modelDataReaderService;
        private readonly IScenarioRepository _repository;
        private readonly DiscreteTimeSeriesService? _timeSeriesService;
        private readonly IScenarioWorker _worker;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScenarioService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="modelDataReaderService">The model data reader service.</param>
        /// <param name="worker">The scenario worker.</param>
        /// <param name="timeSeriesService">The time series service. Used to check if scenario input time series are valid.</param>
        public ScenarioService(IScenarioRepository repository,
            ModelDataReaderService modelDataReaderService,
            IScenarioWorker worker,
            DiscreteTimeSeriesService? timeSeriesService = null) : base(repository)
        {
            Guard.Against.Null(modelDataReaderService, nameof(modelDataReaderService));
            Guard.Against.Null(worker, nameof(worker));
            _repository = repository;
            _modelDataReaderService = modelDataReaderService;
            _worker = worker;
            _timeSeriesService = timeSeriesService;
        }

        /// <inheritdoc />
        public override void Add(Scenario scenario, ClaimsPrincipal? user = null)
        {
            ValidateScenario(scenario, user);
            base.Add(scenario, user);
        }

        /// <inheritdoc />
        public override bool TryAdd(Scenario scenario, ClaimsPrincipal? user = null)
        {
            ValidateScenario(scenario, user);
            return base.TryAdd(scenario, user);
        }

        /// <inheritdoc />
        public override void AddOrUpdate(Scenario scenario, ClaimsPrincipal? user = null)
        {
            ValidateScenario(scenario, user);
            base.AddOrUpdate(scenario, user);
        }

        /// <inheritdoc />
        public override void Update(Scenario scenario, ClaimsPrincipal? user = null)
        {
            ValidateScenario(scenario, user);
            base.Update(scenario, user);
        }

        /// <inheritdoc />
        public override bool TryUpdate(Scenario scenario, ClaimsPrincipal? user = null)
        {
            ValidateScenario(scenario, user);
            return base.TryUpdate(scenario, user);
        }

        /// <summary>
        ///     Creates a derived scenario from an existing scenario based on data from the specified simulation.
        /// </summary>
        /// <param name="derivedScenarioName">The derived scenario name.</param>
        /// <param name="scenarioId">The existing scenario identifier.</param>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="user">The user.</param>
        public Scenario CreateAndAdd(string derivedScenarioName, string scenarioId, Guid simulationId, Parameters? parameters = null, ClaimsPrincipal? user = null)
        {
            if (_repository is not IDerivedScenarioFactory factory)
            {
                throw new Exception($"Cannot create scenario since no derived-scenario factory ({nameof(IDerivedScenarioFactory)}) was injected into {nameof(ScenarioService)}.");
            }

            var scenario = Get(scenarioId, user);
            var modelDataReader = _modelDataReaderService.Get(scenario.ModelDataReaderId, user);
            if (modelDataReader.GetSimulations(scenarioId).All(simulation => simulation.Id != simulationId))
            {
                throw new KeyNotFoundException($"Simulation with id '{simulationId}' was not found.");
            }

            var derivedScenario = factory.Create(derivedScenarioName, simulationId, parameters);
            if (!Exists(derivedScenario.Id))
            {
                Add(derivedScenario, user);
            }

            return derivedScenario;
        }

        public async Task<ITimeSeriesData<double>> GetInputTimeSeriesData(string scenarioId, string timeSeriesKey, ClaimsPrincipal? user = null)
        {
            var scenario = Get(scenarioId, user);
            var modelDataReader = _modelDataReaderService.Get(scenario.ModelDataReaderId, user);
            if (!modelDataReader.GetInputTimeSeriesList().ContainsKey(timeSeriesKey))
            {
                throw new ArgumentException($"The time series '{timeSeriesKey}' is not a valid input time series for model data reader '{scenario.ModelDataReaderId}'.", nameof(scenario));
            }

            return await modelDataReader.GetInputTimeSeriesValues(timeSeriesKey);
        }

        /// <summary>
        ///     Executes the specified scenario.
        /// </summary>
        /// <param name="scenarioId">The scenario identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Guid.</returns>
        public virtual Guid Execute(string scenarioId, ClaimsPrincipal? user = null)
        {
            if (!Exists(scenarioId, user))
            {
                throw new KeyNotFoundException($"Scenario with id '{scenarioId}' was not found.");
            }

            var scenario = Get(scenarioId, user);
            return _worker.Execute(scenario);
        }

        /// <summary>
        ///     Gets all simulations for the specified scenario.
        /// </summary>
        /// <param name="scenarioId">The scenario identifier.</param>
        /// <param name="user">The user.</param>
        public Simulation[] GetSimulations(string scenarioId, ClaimsPrincipal? user = null)
        {
            var scenario = Get(scenarioId, user);
            var modelDataReader = _modelDataReaderService.Get(scenario.ModelDataReaderId, user);
            return modelDataReader.GetSimulations(scenarioId).ToArray();
        }

        /// <summary>
        ///     Get the latest simulation for the specified scenario.
        /// </summary>
        /// <param name="scenarioId">The scenario identifier.</param>
        /// <param name="user">The user.</param>
        public Simulation GetLatestSimulation(string scenarioId, ClaimsPrincipal? user = null)
        {
            return _repository.GetLatestSimulation(scenarioId, user);
        }

        /// <summary>
        ///     Gets the specified output time series data from the specified simulation of the specified scenario.
        /// </summary>
        /// <param name="scenarioId">The scenario identifier.</param>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="timeSeriesKey">The time series key.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Threading.Tasks.Task&lt;DHI.Services.TimeSeries.ITimeSeriesData&lt;System.Double&gt;&gt;.</returns>
        public async Task<ITimeSeriesData<double>> GetSimulationData(string scenarioId, Guid simulationId, string timeSeriesKey, ClaimsPrincipal? user = null)
        {
            var scenario = Get(scenarioId, user);
            var modelDataReader = _modelDataReaderService.Get(scenario.ModelDataReaderId, user);
            if (modelDataReader.GetSimulations(scenarioId).All(simulation => simulation.Id != simulationId))
            {
                throw new KeyNotFoundException($"Simulation with id '{simulationId}' was not found.");
            }

            if (!modelDataReader.GetOutputTimeSeriesList().ContainsKey(timeSeriesKey))
            {
                throw new ArgumentException($"The time series '{timeSeriesKey}' is not a valid output time series for model data reader '{scenario.ModelDataReaderId}'.", nameof(scenario));
            }

            return await modelDataReader.GetOutputTimeSeriesValues(simulationId, timeSeriesKey);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IScenarioRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string? path)
        {
            return Service.GetProviderTypes<IScenarioRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wild card
        ///     (* and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IScenarioRepository>(path, searchPattern);
        }

        /// <summary>
        ///     Gets an array of worker types compatible with the scenario service.
        /// </summary>
        /// <param name="path">The path where to look for compatible worker types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetWorkerTypes(string? path = null)
        {
            return Service.GetProviderTypes<IScenarioWorker>(path);
        }

        private void ValidateScenario(Scenario scenario, ClaimsPrincipal? user = null)
        {
            if (!_modelDataReaderService.Exists(scenario.ModelDataReaderId, user))
            {
                throw new KeyNotFoundException($"Model with id '{scenario.ModelDataReaderId}' was not found.");
            }

            var modelDataReader = _modelDataReaderService.Get(scenario.ModelDataReaderId);
            foreach (var parameterValue in scenario.ParameterValues.ToArray())
            {
                if (!modelDataReader.GetParameterList().ContainsKey(parameterValue.Key))
                {
                    throw new ArgumentException($"The parameter '{parameterValue.Key}' is not a valid parameter for model data reader '{scenario.ModelDataReaderId}'", nameof(scenario));
                }

                var type = modelDataReader.GetParameterList()[parameterValue.Key];
                var value = parameterValue.Value;
                if (value is JToken token)
                {
                    value = token.ToObject(type);
                    scenario.ParameterValues[parameterValue.Key] = value;
                }
                else if (value.GetType() != type)
                {
                    throw new ArgumentException($"The type '{parameterValue.Value.GetType()}' of the value for parameter '{parameterValue.Key}' is not valid. Value must be of type '{type}'.", nameof(scenario));
                }
            }

            foreach (var inputTimeSeriesValue in scenario.InputTimeSeriesValues)
            {
                if (!modelDataReader.GetInputTimeSeriesList().ContainsKey(inputTimeSeriesValue.Key))
                {
                    throw new ArgumentException($"The time series '{inputTimeSeriesValue.Key}' is not a valid input time series for model data reader '{scenario.ModelDataReaderId}'.", nameof(scenario));
                }

                if (_timeSeriesService != null && _timeSeriesService.Exists(inputTimeSeriesValue.Value))
                {
                    throw new ArgumentException($"A time series with ID '{inputTimeSeriesValue.Value}' was not found.", nameof(scenario));
                }
            }
        }
    }
}