namespace DHI.Services.Models.WebApi.Host
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

#warning Probably a ConnectionString is needed in the ctor signature
        public FakeModelDataReader()
        {
            _parameters = new Dictionary<string, object>
            {
                {"foo", 1L},
                {"bar", false},
                {"baz", new [] {"string1", "string2"}},
                {"qux", new Dictionary<string, int> {{"james", 9001}, {"joe", 1234}}}

            };
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
                new Simulation(new Guid("f5d65c95-6859-4303-b310-183e72c04b04"), "myScenario")
                {
                    Status = "Finished",
                    Requested = new DateTime(2020, 10, 21, 10, 0, 0),
                    Start = new DateTime(2020, 10, 21, 10, 0, 0).AddSeconds(3),
                    End = new DateTime(2020, 10, 21, 10, 0, 0).AddMinutes(5),
                    SimulationRange = new DateRange(new DateTime(2020, 10, 22), new DateTime(2020, 10, 24)),

                },
                new Simulation(new Guid("cf4b7076-e392-418e-816e-21cc76695551"), "myScenario")
                {
                    Status = "Failed",
                    Requested = new DateTime(2020, 10, 21),
                    Start = new DateTime(2020, 10, 21).AddSeconds(2),
                    SimulationRange = new DateRange(new DateTime(2020, 11, 12), new DateTime(2020, 11, 14))
                },
                new Simulation(new Guid("185ba082-94ec-4b6a-bc7a-ceed263a33ed"), "scenario1")
                {
                    Status = "Failed",
                    Requested = new DateTime(2020, 10, 22),
                    Start = new DateTime(2020, 10, 22).AddSeconds(2),
                    SimulationRange = new DateRange(new DateTime(2020, 11, 12), new DateTime(2020, 11, 14))
                }
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

        public Task<ITimeSeriesData<double>> GetInputTimeSeriesValues(string timeSeriesId)
        {
            return Task.FromResult(_inputTimeSeriesList[timeSeriesId]);
        }

        public IDictionary<string, string> GetOutputTimeSeriesList()
        {
            return _outputTimeSeriesList.ToDictionary(kvp => kvp.Key, kvp => kvp.Key);
        }

        public IEnumerable<Simulation> GetSimulations(string scenarioId)
        {
            return _simulations.Where(s => s.ScenarioId == scenarioId);
        }

        public Task<Maybe<ITimeSeriesData<double>>> GetOutputTimeSeriesValues(Guid simulationId, string timeSeriesId)
        {
            return Task.FromResult(_outputTimeSeriesList[timeSeriesId].ToMaybe());
        }
    }
}