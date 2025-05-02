namespace DHI.Services.Models.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public class ScenarioServiceTest
    {
        private readonly ModelDataReaderService _modelDataReaderService;

        public ScenarioServiceTest()
        {
            var modelRepository = new FakeModelDataReaderRepository();
            modelRepository.Add(new ModelDataReader<FakeModelDataReader>("MyModelDataReader", "My model data reader"));
            _modelDataReaderService = new ModelDataReaderService(modelRepository);
        }

        [Fact]
        public void AddWithNonExistingModelThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "NonExistingModelId");
            
            var e = Assert.Throws<KeyNotFoundException>(() => scenarioService.Add(scenario));
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public void AddWithNonExistingParameterThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var parameterValues = new Dictionary<string, object> {{"NonExistingParameter", 99}};
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader", parameterValues, null);

            var e = Assert.Throws<ArgumentException>(() => scenarioService.Add(scenario));
            Assert.Contains("not a valid parameter", e.Message);
        }

        [Fact]
        public void AddWithIllegalParameterValueThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var parameterValues = new Dictionary<string, object> { { "foo", false } };
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader", parameterValues, null);

            var e = Assert.Throws<ArgumentException>(() => scenarioService.Add(scenario));
            Assert.Contains("Value must be of type", e.Message);
        }

        [Fact]
        public void AddWithNonExistingInputTimeSeriesThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var inputTimeSeriesValues = new Dictionary<string, string> { { "NonExistingTimeSeries", "MyTimeSeriesId" } };
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader", null, inputTimeSeriesValues);

            var e = Assert.Throws<ArgumentException>(() => scenarioService.Add(scenario));
            Assert.Contains("not a valid input time series", e.Message);
        }

        [Fact]
        public void CreateAndAddThrowsIfNoFactory()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());

            var e = Assert.Throws<Exception>(() => scenarioService.CreateAndAdd("myDerivedScenario", "scenarioId", Guid.NewGuid()));
            Assert.Contains("Cannot create scenario", e.Message);
        }

        [Fact]
        public void CreateAndAddFromNonExistingScenarioThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepositoryWithFactory(), _modelDataReaderService, new FakeWorker());

            var e = Assert.Throws<KeyNotFoundException>(() => scenarioService.CreateAndAdd("myDerivedScenario", "NonExistingScenarioId", Guid.NewGuid()));
            Assert.Contains("Scenario", e.Message);
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public void CreateAndAddFromNonExistingSimulationThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepositoryWithFactory(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);

            var e = Assert.Throws<KeyNotFoundException>(() => scenarioService.CreateAndAdd("myDerivedScenario", scenario.Id, Guid.NewGuid()));
            Assert.Contains("Simulation", e.Message);
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public async Task GetInputTimeSeriesDataForNonExistingScenarioThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());

            var e = await Assert.ThrowsAsync<KeyNotFoundException>(() => scenarioService.GetInputTimeSeriesData("NonExistingScenario", "ts1-in"));
            Assert.Contains("Scenario", e.Message);
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public async Task GetInputTimeSeriesDataForNonExistingTimeSeriesThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);

            var e = await Assert.ThrowsAsync<ArgumentException>(() => scenarioService.GetInputTimeSeriesData(scenario.Id, "NonExistingTimeSeriesKey"));
            Assert.Contains("not a valid input time series", e.Message);
        }

        [Fact]
        public void ExecuteNonExistingScenarioThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());

            var e = Assert.Throws<KeyNotFoundException>(() => scenarioService.Execute("NonExistingScenario"));
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public void GetSimulationsForNonExistingScenarioThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());

            var e =  Assert.Throws<KeyNotFoundException>(() => scenarioService.GetSimulations("NonExistingScenario"));
            Assert.Contains("Scenario", e.Message);
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingScenarioThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());

            var e = await Assert.ThrowsAsync<KeyNotFoundException>(() => scenarioService.GetSimulationData("NonExistingScenario", Guid.NewGuid(), "ts1-out"));
            Assert.Contains("Scenario", e.Message);
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingSimulationThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);

            var e = await Assert.ThrowsAsync<KeyNotFoundException>(() => scenarioService.GetSimulationData(scenario.Id, Guid.NewGuid(), "ts1-out"));
            Assert.Contains("Simulation", e.Message);
            Assert.Contains("not found", e.Message);
        }

        [Fact]
        public async Task GetSimulationDataForNonExistingTimeSeriesThrows()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            var simulations = scenarioService.GetSimulations(scenario.Id);

            var e = await Assert.ThrowsAsync<ArgumentException>(() => scenarioService.GetSimulationData(scenario.Id, simulations.First().Id, "NonExistingTimeSeriesKey"));
            Assert.Contains("is not a valid output time series for model", e.Message);
        }

        [Fact]
        public void AddScenarioIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);

            Assert.Equal(scenario.Id, scenarioService.Get(scenario.Id).Id);
        }

        [Fact]
        public void UpdateIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            scenario.ParameterValues.Add("foo", 123);
            scenarioService.Update(scenario);

            Assert.Contains("foo", scenarioService.Get(scenario.Id).ParameterValues);
        }

        [Fact]
        public void AddOrUpdateIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var raisedEvents = new List<string>();
            scenarioService.Added += (s, e) => { raisedEvents.Add("Added"); };
            scenarioService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.AddOrUpdate(scenario);
            scenario.ParameterValues.Add("foo", 123);
            scenarioService.AddOrUpdate(scenario);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Contains("foo", scenarioService.Get(scenario.Id).ParameterValues);
        }

        [Fact]
        public void TryAddIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");

            Assert.True(scenarioService.TryAdd(scenario));
            Assert.Equal(scenario.Id, scenarioService.Get(scenario.Id).Id);
        }

        [Fact]
        public void TryUpdateIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            scenario.ParameterValues.Add("foo", 123);

            Assert.True(scenarioService.TryUpdate(scenario));
            Assert.Contains("foo", scenarioService.Get(scenario.Id).ParameterValues);
        }

        [Fact]
        public void CreateAndAddIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepositoryWithFactory(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            var simulations = scenarioService.GetSimulations(scenario.Id);
            var derivedScenario = scenarioService.CreateAndAdd("myDerivedScenarioName", scenario.Id, simulations[0].Id);

            Assert.True(derivedScenario.Name == "myDerivedScenarioName");
            Assert.True(scenarioService.Exists(derivedScenario.Id));
        }

        [Fact]
        public void GetSimulationsIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            var simulations = scenarioService.GetSimulations(scenario.Id);

            Assert.True(simulations.Any());
        }
        [Fact]
        public async Task GetInputTimeSeriesDataIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            var data = await scenarioService.GetInputTimeSeriesData(scenario.Id, "ts1-in");

            Assert.Equal(11, data.DateTimes.Count);
            Assert.Equal(12, data.Values.Max());
        }


        [Fact]
        public async Task GetSimulationDataIsOk()
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), _modelDataReaderService, new FakeWorker());
            var scenario = new Scenario("myScenario", "My Scenario", "MyModelDataReader");
            scenarioService.Add(scenario);
            var simulations = scenarioService.GetSimulations(scenario.Id);
            var data = await scenarioService.GetSimulationData(scenario.Id, simulations.First().Id, "ts1-out");

            Assert.Equal(11, data.DateTimes.Count);
            Assert.Equal(123, data.Values.Max());
        }
    }

    internal class FakeModelDataReaderRepository : FakeRepository<IModelDataReader, string>, IModelDataReaderRepository
    {
        public IEnumerable<Scenario> GetScenarios(string id, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Simulation> GetSimulations(string id, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }

    internal class FakeScenarioRepository : FakeRepository<Scenario, string>, IScenarioRepository
    {
        public IEnumerable<string> GetInputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Simulation GetLatestSimulation(string scenarioId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetOutputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<Guid, Guid> GetSimulationModelObjectAssociations(string simulationId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetSimulationModelObjectProperties(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Simulation[] GetSimulations(string scenarioId, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSimulationSchematics(string simulationId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }

    internal class FakeScenarioRepositoryWithFactory : FakeRepository<Scenario, string>, IScenarioRepository, IDerivedScenarioFactory
    {
        public Scenario Create(string derivedScenarioName, Guid simulationId, Parameters parameters = null)
        {
            return new Scenario($"ScenarioFromSimulation{simulationId}", derivedScenarioName, "MyModelDataReader", null, null, null);
        }

        public IEnumerable<string> GetInputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Simulation GetLatestSimulation(string scenarioId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetOutputTimeSeriesForSimulationModelObject(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<Guid, Guid> GetSimulationModelObjectAssociations(string simulationId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetSimulationModelObjectProperties(string simulationId, Guid modelObjectId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public Simulation[] GetSimulations(string scenarioId, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSimulationSchematics(string simulationId, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }

    internal class FakeWorker : IScenarioWorker
    {
        public Guid Execute(Scenario scenario)
        {
            throw new NotImplementedException();
        }

        public void Cancel(Guid scenarioId)
        {
            throw new NotImplementedException();
        }
    }
}