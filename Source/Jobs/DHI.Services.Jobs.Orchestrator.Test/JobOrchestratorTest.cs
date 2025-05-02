namespace DHI.Services.Jobs.Orchestrator.Test
{
    using Logging;
    using Scalars;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Workflows;
    using Xunit;

    public class JobOrchestratorTest : IClassFixture<JobWorkersFixture>
    {
        private readonly JobWorkersFixture _fixture;

        public JobOrchestratorTest(JobWorkersFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void NullOrEmptyJobWorkersThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new JobOrchestrator<string>(null!, _fixture.Logger, 1000, 5000, 5000));
            Assert.Contains("Value cannot be null. (Parameter 'jobWorkers')", e.Message);

            var jobWorkers = new List<JobWorker<Workflow, string>>();
            var e2 = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(jobWorkers, _fixture.Logger, 1000, 5000, 5000));
            Assert.Contains("Required input `jobWorkers` is empty. (Parameter 'jobWorkers')", e2.Message);
        }

        [Fact]
        public void NullLoggerThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, null!, 1000, 5000, 5000));
            Assert.Contains("Value cannot be null. (Parameter 'logger')", e.Message);
        }

        [Fact]
        public void NegativeOrZeroExecutionTimerIntervalThrows()
        {
            var e = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, -999, 5000, 5000));
            Assert.Contains("Required input executionTimerInterval cannot be zero or negative. (Parameter 'executionTimerInterval')", e.Message);

            var e2 = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 0, 5000, 5000));
            Assert.Contains("Required input executionTimerInterval cannot be zero or negative. (Parameter 'executionTimerInterval')", e2.Message);
        }


        [Fact]
        public void NegativeOrZeroHeartbeatTimerIntervalThrows()
        {
            var e = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, -999, 5000));
            Assert.Contains("Required input heartbeatTimerInterval cannot be zero or negative. (Parameter 'heartbeatTimerInterval')", e.Message);

            var e2 = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 0, 5000));
            Assert.Contains("Required input heartbeatTimerInterval cannot be zero or negative. (Parameter 'heartbeatTimerInterval')", e2.Message);
        }

        [Fact]
        public void NegativeOrZeroTimeoutTimerIntervalThrows()
        {
            var e = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, -999));
            Assert.Contains("Required input timeoutTimerInterval cannot be zero or negative. (Parameter 'timeoutTimerInterval')", e.Message);

            var e2 = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 0));
            Assert.Contains("Required input timeoutTimerInterval cannot be zero or negative. (Parameter 'timeoutTimerInterval')", e2.Message);
        }

        [Fact]
        public void NullScalarServiceThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000, null!, null!));
            Assert.Contains("Value cannot be null. (Parameter 'scalarService')", e.Message);
        }

        [Theory, AutoMoqData]
        public void NullOrEmptyJobServiceDictionaryThrows(IGroupedScalarRepository<string, int> scalarRepository)
        {
            var scalarService = new GroupedScalarService(scalarRepository);
            var e = Assert.Throws<ArgumentNullException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000, scalarService, null!));
            Assert.Contains("Value cannot be null. (Parameter 'jobServices')", e.Message);

            var jobServices = new Dictionary<string, IJobService<string>>();
            var e2 = Assert.Throws<ArgumentException>(() => new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000, scalarService, jobServices));
            Assert.Contains("Required input `jobServices` is empty. (Parameter 'jobServices')", e2.Message);
        }

        [Fact]
        public void CreationIsOk()
        {
            var jobOrchestrator = new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000);

            Assert.False(jobOrchestrator.IsRunning());
            Assert.False(jobOrchestrator.ScalarsEnabled());
        }

        [Fact]
        public void StartAndStopIsOk()
        {
            var jobOrchestrator = new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000);
            jobOrchestrator.Start();
            Assert.True(jobOrchestrator.IsRunning());

            jobOrchestrator.Stop();
            Assert.False(jobOrchestrator.IsRunning());
        }

        [Theory, AutoMoqData]
        public void CreationWithScalarServiceIsOk(IGroupedScalarRepository<string, int> scalarRepository)
        {
            var scalarService = new GroupedScalarService(scalarRepository);
            var jobServices = new Dictionary<string, IJobService<string>>
            {
                { _fixture.JobWorker.Id, _fixture.JobService }
            };

            var jobOrchestrator = new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000, scalarService, jobServices);

            Assert.False(jobOrchestrator.IsRunning());
            Assert.True(jobOrchestrator.ScalarsEnabled());
        }

        [Fact]
        public void ScalarsAreWrittenAsExpected()
        {
            var scalarService = new GroupedScalarService(new FakeScalarRepository());
            var jobServices = new Dictionary<string, IJobService<string>>
            {
                { _fixture.JobWorker.Id, _fixture.JobService }
            };

            var jobOrchestrator = new JobOrchestrator<string>(_fixture.JobWorkers, _fixture.Logger, 1000, 5000, 5000, scalarService, jobServices);
            Assert.True(jobOrchestrator.ScalarsEnabled());
            Assert.Empty(scalarService.GetAll());

            jobOrchestrator.Start();
            Thread.Sleep(2000);
            var scalarFullNames = scalarService.GetFullNames().ToList();
            Assert.NotEmpty(scalarFullNames);
            Assert.Contains(scalarFullNames, s => FullName.Parse(s).Name == "Jobs In Progress");
            var id = scalarFullNames.Single(s => FullName.Parse(s).Name == "Jobs In Progress");
            Assert.True(scalarService.TryGet(id, out var scalar));
            var maybe = scalar.GetData();
            Assert.True(maybe.HasValue);
            var jobsInProgress = maybe.Value.Value;
            Assert.Equal(0, jobsInProgress);
        }
    }
}