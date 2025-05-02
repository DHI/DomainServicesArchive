namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Scenarios;
    using Xunit;

    public class ScenarioServiceTest
    {
        private const int RepeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ScenarioService(null));
        }

        [Theory, AutoScenarioData]
        public void GetNonExistingThrows(ScenarioService scenarioService)
        {
            Assert.Throws<KeyNotFoundException>(() => scenarioService.Get("UnknownScenario"));
        }

        [Theory, AutoScenarioData]
        public void UpdateNonExistingThrows(ScenarioService scenarioService, Scenario scenario)
        {
            Assert.Throws<KeyNotFoundException>(() => scenarioService.Update(scenario));
        }

        [Theory, AutoScenarioData]
        public void RemoveNonExistingThrows(ScenarioService scenarioService, Scenario scenario)
        {
            Assert.Throws<KeyNotFoundException>(() => scenarioService.Remove(scenario.Id));
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetAllIsOk(ScenarioService scenarioService)
        {
            Assert.Equal(RepeatCount, scenarioService.GetAll().Count());
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetInfoWithNonExistingLastJobResetsLastJobIdProperty(ScenarioService scenarioService)
        {
            var scenario = scenarioService.GetAll().First();
            var scenarioInfo = scenarioService.Get(scenario.Id);

            Assert.Null(scenarioInfo.LastJobId);
        }

        [Theory, AutoData]
        public void GetInfoWithNoJobRepositoryIsOk(Scenario scenario)
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(new List<Scenario> { scenario }));
            var scenarioInfo = scenarioService.Get(scenario.Id);
            Assert.Null(scenarioInfo.LastJobProgress);
            Assert.Equal(JobStatus.Unknown, scenarioInfo.LastJobStatus);
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetInIntervalIsOk(ScenarioService scenarioService)
        {
            var scenarios = scenarioService.GetAll().ToList();
            var from = scenarios.Select(s => s.DateTime).Min().Value.AddSeconds(1);
            var to = scenarios.Select(s => s.DateTime).Max().Value.AddSeconds(-1);
            Assert.Equal(RepeatCount - 2, scenarioService.Get(from, to).Count());
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetInIntervalWithNonExistingLastJobResetsLastJobIdProperties(ScenarioService scenarioService)
        {
            var scenarios = scenarioService.GetAll().ToList();
            var from = scenarios.Select(s => s.DateTime).Min().Value.AddSeconds(1);
            var to = scenarios.Select(s => s.DateTime).Max().Value.AddSeconds(-1);
            var scenarioInfoList = scenarioService.Get(from, to);

            foreach (var scenarioInfo in scenarioInfoList)
            {
                Assert.Null(scenarioInfo.LastJobId);
            }
        }

        [Theory, AutoData]
        public void GetInIntervalWithNoJobRepositoryIsOk(List<Scenario> scenarios)
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(scenarios));
            var scenarioInfoList = scenarioService.Get(DateTime.MinValue, DateTime.MaxValue);
            foreach (var scenarioInfo in scenarioInfoList)
            {
                Assert.Null(scenarioInfo.LastJobProgress);
                Assert.Equal(JobStatus.Unknown, scenarioInfo.LastJobStatus);
            }
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetByQueryIsOk(ScenarioService scenarioService)
        {
            var scenario = scenarioService.GetAll().First();
            var query = new Query<Scenario>
            {
                new QueryCondition("Id", QueryOperator.Equal, scenario.Id)
            };

            Assert.Single(scenarioService.Get(query));
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetByQueryWithNonExistingLastJobResetsLastJobIdProperties(ScenarioService scenarioService)
        {
            var scenario = scenarioService.GetAll().First();
            var query = new Query<Scenario>
            {
                new QueryCondition("Id", QueryOperator.Equal, scenario.Id)
            };
            
            var scenarioInfoList = scenarioService.Get(query).ToArray();
            Assert.True(scenarioInfoList.Any());

            foreach (var scenarioInfo in scenarioInfoList)
            {
                Assert.Null(scenarioInfo.LastJobId);
            }
        }

        [Theory, AutoData]
        public void GetByQueryWithNoJobRepositoryIsOk(List<Scenario> scenarios)
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(scenarios));
            var scenario = scenarioService.GetAll().First();
            var query = new Query<Scenario>
            {
                new QueryCondition("Id", QueryOperator.Equal, scenario.Id)
            };

            var scenarioInfoList = scenarioService.Get(query).ToArray();
            Assert.True(scenarioInfoList.Any());

            foreach (var scenarioInfo in scenarioInfoList)
            {
                Assert.Null(scenarioInfo.LastJobProgress);
                Assert.Equal(JobStatus.Unknown, scenarioInfo.LastJobStatus);
            }
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void GetIdsIsOk(ScenarioService scenarioService)
        {
            Assert.Equal(RepeatCount, scenarioService.GetIds().Count());
        }

        [Theory, AutoScenarioData]
        public void GetWithDataSelectorIsOk(ScenarioService scenarioService, string scenarioId)
        {
            var scenarioData =
@"{
  ""field1"":""prop1"",
  ""obj1"": {
    ""field2"":""prop2"",
    ""field2.1"":""prop2.1"",
  },
  ""obj2"": {
    ""field3"":""prop3"",
  },
  ""field4"":""prop4""
}";

            var updated = new Scenario(scenarioId) { Data = scenarioData };
            scenarioService.AddOrUpdate(updated);

            var dataSelectors = new string[] { "field1", "obj1.field2", "obj2" };
            var scenarioInfo = scenarioService.Get(scenarioId, dataSelectors);

            Assert.Equal(
                expected: "{\"field1\":\"prop1\",\"obj1\":{\"field2\":\"prop2\"},\"obj2\":{\"field3\":\"prop3\"}}",
                actual: scenarioInfo.Data);
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void AddAndGetIsOk(IJobRepository<Guid, string> jobRepository, Scenario scenario)
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), jobRepository);
            var lastJob = jobRepository.GetAll().Last();
            scenario.LastJobId = lastJob.Id;
            scenarioService.Add(scenario);

            Assert.Equal(scenario.Id, scenarioService.Get(scenario.Id).Id);
            Assert.Equal(lastJob.Progress, scenarioService.Get(scenario.Id).LastJobProgress);
            Assert.Equal(lastJob.Status, scenarioService.Get(scenario.Id).LastJobStatus);
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void CountIsOk(ScenarioService scenarioService)
        {
            Assert.Equal(RepeatCount, scenarioService.Count());
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void ExistsIsOk(ScenarioService scenarioService)
        {
            var scenario = scenarioService.GetAll().ToArray()[0];
            Assert.True(scenarioService.Exists(scenario.Id));
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void DoesNotExistIsOk(ScenarioService scenarioService)
        {
            Assert.False(scenarioService.Exists("NonExistingHost"));
        }

        [Theory, AutoScenarioData]
        public void EventsAreRaisedOnAdd(ScenarioService scenarioService, Scenario scenario)
        {
            var raisedEvents = new List<string>();
            scenarioService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            scenarioService.Added += (s, e) => { raisedEvents.Add("Added"); };

            scenarioService.Add(scenario);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoScenarioData]
        public void RemoveIsOk(ScenarioService scenarioService, Scenario scenario)
        {
            scenarioService.Add(scenario);
            scenarioService.Remove(scenario.Id);

            Assert.False(scenarioService.Exists(scenario.Id));
            Assert.Equal(0, scenarioService.Count());
        }

        [Theory, AutoScenarioData]
        public void EventsAreRaisedOnRemove(ScenarioService scenarioService, Scenario scenario)
        {
            var raisedEvents = new List<string>();
            scenarioService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            scenarioService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            scenarioService.Add(scenario);

            scenarioService.Remove(scenario.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void UpdateIsOk(IJobRepository<Guid, string> jobRepository, Scenario scenario)
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), jobRepository);
            var lastJob = jobRepository.GetAll().Last();
            scenario.LastJobId = lastJob.Id;
            scenarioService.Add(scenario);
            var updated = new Scenario(scenario.Id) { LastJobId = scenario.LastJobId, Version = new Guid()};
            scenarioService.Update(updated);

            Assert.Equal(updated.Version, scenarioService.Get(scenario.Id).Version);
        }

        [Theory, AutoScenarioData(RepeatCount)]
        public void AddOrUpdateIsOk(IJobRepository<Guid, string> jobRepository, Scenario scenario)
        {
            var scenarioService = new ScenarioService(new FakeScenarioRepository(), jobRepository);
            var lastJob = jobRepository.GetAll().Last();
            scenario.LastJobId = lastJob.Id;
            var raisedEvents = new List<string>();
            scenarioService.Added += (s, e) => { raisedEvents.Add("Added"); };
            scenarioService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scenarioService.AddOrUpdate(scenario);
            var updated = new Scenario(scenario.Id) { LastJobId = scenario.LastJobId, Version = new Guid() };
            scenarioService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Version, scenarioService.Get(scenario.Id).Version);
        }

        [Theory, AutoScenarioData]
        public void EventsAreRaisedOnUpdate(ScenarioService scenarioService, Scenario scenario)
        {
            var raisedEvents = new List<string>();
            scenarioService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            scenarioService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            scenarioService.Add(scenario);

            var updated = new Scenario(scenario.Id) { LastJobId = scenario.LastJobId, Version = new Guid() };
            scenarioService.Update(updated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoScenarioData]
        public void TrySoftRemoveIsOk(ScenarioService scenarioService, Scenario scenario)
        {
            scenario.Deleted = null;
            scenarioService.Add(scenario);

            Assert.True(scenarioService.TrySoftRemove(scenario.Id));
            var removedScenario = scenarioService.Get(scenario.Id);
            Assert.NotNull(removedScenario.Deleted);
            Assert.InRange(removedScenario.Deleted.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory, AutoScenarioData]
        public void EventsAreRaisedOnTrySoftRemove(ScenarioService scenarioService, Scenario scenario)
        {
            var raisedEvents = new List<string>();
            scenarioService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            scenarioService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            scenario.Deleted = null;
            scenarioService.Add(scenario);

            scenarioService.TrySoftRemove(scenario.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoScenarioData]
        public void TrySoftRemoveIsIdempotent(ScenarioService scenarioService, Scenario scenario)
        {
            scenario.Deleted = null;
            scenarioService.Add(scenario);

            Assert.True(scenarioService.TrySoftRemove(scenario.Id));
            var removedScenario = scenarioService.Get(scenario.Id);
            Assert.NotNull(removedScenario.Deleted);
            var deletedTime = removedScenario.Deleted.Value;
            Assert.InRange(deletedTime, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));

            Assert.True(scenarioService.TrySoftRemove(scenario.Id));
            Assert.Equal(deletedTime, scenarioService.Get(scenario.Id).Deleted);
        }

        [Theory, AutoScenarioData]
        public void TrySoftRemoveNonExistingReturnsFalse(ScenarioService scenarioService, string scenarioId)
        {
            Assert.False(scenarioService.TrySoftRemove(scenarioId));
        }
    }
}