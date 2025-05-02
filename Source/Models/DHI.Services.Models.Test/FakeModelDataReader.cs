namespace DHI.Services.Models.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TimeSeries;

    [Serializable]
    public class FakeModelDataReader : IModelInputReader, IModelOutputReader
    {
        private readonly IDictionary<string, object> _parameters;
        private readonly IDictionary<string, ITimeSeriesData<double>> _inputTimeSeriesList;
        private readonly IDictionary<string, ITimeSeriesData<double>> _outputTimeSeriesList;
        private readonly List<Simulation> _simulations;

        public FakeModelDataReader()
        {
            _parameters = new Dictionary<string, object> { {"foo", 1}, {"bar", false}};
            _inputTimeSeriesList = new Dictionary<string, ITimeSeriesData<double>> {{"ts1-in", new TimeSeriesData<double>(new List<DateTime>
                {
                    new DateTime(2000, 1, 1, 0, 0, 0),
                    new DateTime(2000, 1, 1, 0, 30, 0),
                    new DateTime(2000, 1, 1, 1, 0, 0),
                    new DateTime(2000, 1, 1, 1, 30, 0),
                    new DateTime(2000, 1, 1, 2, 0, 0),
                    new DateTime(2000, 1, 1, 2, 30, 0),
                    new DateTime(2000, 1, 1, 3, 0, 0),
                    new DateTime(2000, 1, 1, 3, 30, 0),
                    new DateTime(2000, 1, 1, 4, 0, 0),
                    new DateTime(2000, 1, 1, 4, 30, 0),
                    new DateTime(2000, 1, 1, 5, 0, 0)
                },
                new List<double?>
                {
                    5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
                })}};

            _outputTimeSeriesList = new Dictionary<string, ITimeSeriesData<double>> {{"ts1-out", new TimeSeriesData<double>(new List<DateTime>
                {
                    new DateTime(2000, 1, 2, 0, 0, 0),
                    new DateTime(2000, 1, 2, 0, 30, 0),
                    new DateTime(2000, 1, 2, 1, 0, 0),
                    new DateTime(2000, 1, 2, 1, 30, 0),
                    new DateTime(2000, 1, 2, 2, 0, 0),
                    new DateTime(2000, 1, 2, 2, 30, 0),
                    new DateTime(2000, 1, 2, 3, 0, 0),
                    new DateTime(2000, 1, 2, 3, 30, 0),
                    new DateTime(2000, 1, 2, 4, 0, 0),
                    new DateTime(2000, 1, 2, 4, 30, 0),
                    new DateTime(2000, 1, 2, 5, 0, 0)
                },
                new List<double?>
                {
                    3.2, -1.6, null, 10.1, 94, 123, null, 0.8, 6.34, 8, 10
                })}};

            _simulations = new List<Simulation>
            {
                new Simulation("myScenario") { Status = "Finished"},
                new Simulation("myScenario") { Status = "Failed"}
            }; 
        }

        public IDictionary<string, Type> GetParameterList()
        {
            return _parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetType());
        }

        public TParameter GetParameterValue<TParameter>(string parameterId)
        {
            return (TParameter)_parameters[parameterId];
        }

        public IDictionary<string, string> GetInputTimeSeriesList()
        {
            return _inputTimeSeriesList.ToDictionary(kvp => kvp.Key, kvp => kvp.Key);
        }

        public Task<ITimeSeriesData<double>> GetInputTimeSeriesValues(string timeSeriesKey)
        {
            return Task.FromResult(_inputTimeSeriesList[timeSeriesKey]);
        }

        public IDictionary<string, string> GetOutputTimeSeriesList()
        {
            return _outputTimeSeriesList.ToDictionary(kvp => kvp.Key, kvp => kvp.Key);
        }

        public IEnumerable<Simulation> GetSimulations(string scenarioId)
        {
            return _simulations.Where(s => s.ScenarioId == scenarioId);
        }

        public Task<Maybe<ITimeSeriesData<double>>> GetOutputTimeSeriesValues(Guid simulationId, string timeSeriesKey)
        {
            return Task.FromResult(_outputTimeSeriesList[timeSeriesKey].ToMaybe());
        }
    }
}