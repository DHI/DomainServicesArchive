namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Xunit;
    using Scenarios;

    public class ScenarioRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly ScenarioRepository _repository;

        public ScenarioRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "scenarios.json");
            File.Copy(@"../../../Data/scenarios.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            _repository = new ScenarioRepository(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ScenarioRepository(null));
        }

        [Fact]
        public void CreateWithNonExistingFilePathThrows()
        {
            Assert.Throws<FileNotFoundException>(() => new ScenarioRepository("C:\\NonExistingFile.json"));
        }

        [Theory, AutoData]
        public void AddAndGetIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            var actual = _repository.Get(scenario.Id).Value;
            Assert.Equal(scenario.Id, actual.Id);
        }

        [Theory, AutoData]
        public void ContainsIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            Assert.True(_repository.Contains(scenario.Id));
        }

        [Theory, AutoData]
        public void DoesNotContainIsOk(string id)
        {
            Assert.False(_repository.Contains(id));
        }

        [Theory, AutoData]
        public void CountIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoData]
        public void GetAllIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            Assert.Single(_repository.GetAll());
        }

        [Theory, AutoData]
        public void GetWithinTimeIntervalIsOk(List<Scenario> scenarios)
        {
            var minDate = scenarios.Select(s => s.DateTime).Min().Value;
            var maxDate = scenarios.Select(s => s.DateTime).Max().Value;
            foreach (var scenario in scenarios)
            {
                _repository.Add(scenario);
            }
            
            Assert.Equal(scenarios.Count-2, _repository.Get(minDate, maxDate).Count());
        }

        [Theory, AutoData]
        public void GetByQueryIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            var query = new Query<Scenario>
            {
                new QueryCondition("Id", QueryOperator.Equal, scenario.Id)
            };

            Assert.Single(_repository.Get(query));
        }

        [Theory, AutoData]
        public void GetIdsIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            Assert.Equal(scenario.Id, _repository.GetIds().First());
        }

        [Theory, AutoData]
        public void RemoveIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            _repository.Remove(scenario.Id);
            Assert.False(_repository.Contains(scenario.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoData]
        public void UpdateIsOk(Scenario scenario)
        {
            _repository.Add(scenario);
            scenario.DateTime = DateTime.Now;
            _repository.Update(scenario);
            Assert.Equal(scenario.DateTime, _repository.Get(scenario.Id).Value.DateTime);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }
    }
}