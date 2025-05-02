namespace DHI.Services.Jobs.WorkflowWorker.Test
{
    using Fixtures;
    using System;
    using System.Collections.Generic;
    using Xunit;

    [Collection("CodeWorkflowWorker collection")]
    public class SignalRWorkflowWorkerTest
    {
        private readonly SignalRWorkflowWorker _worker;

        public SignalRWorkflowWorkerTest(SignalRWorkflowWorkerFixture fixture)
        {
            _worker = fixture.Worker;
        }

        [Fact]
        public void ExecuteWithNoHostThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _worker.Execute(Guid.NewGuid(), SignalRWorkflowWorkerFixture.GetWorkflow(), new Dictionary<string, object>(), string.Empty));
        }

        [Fact]
        public void ExecuteWithNullTaskThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _worker.Execute(Guid.NewGuid(), null, new Dictionary<string, object>(), "localhost"));
        }

        [Fact]
        public void ExecuteWithTaskNotAWorkflowThrows()
        {
            var ex = Assert.Throws<ArgumentException>(() => _worker.Execute(Guid.NewGuid(), new FakeTask<string>("hs", "Harry", "oneD"), new Dictionary<string, object>(), "localhost"));
            Assert.Equal("Task should be a CodeWorkflow or a Workflow", ex.Message);
        }

        [Fact]
        public void ExecuteWithNoAvailableHostThrows()
        {
            var jobId = Guid.NewGuid();
            var firedEvent = Assert.Raises<EventArgs<Guid>>(
                f => _worker.HostNotAvailable += f,
                f => _worker.HostNotAvailable -= f,
                () => _worker.Execute(jobId, SignalRWorkflowWorkerFixture.GetWorkflow(), new Dictionary<string, object>(), "host"));

            Assert.NotNull(firedEvent);
            Assert.Equal(jobId, firedEvent.Arguments.Item);
        }

        [Fact]
        public void CancelWithNoHostFiresIsOk()
        {
            var jobId = Guid.NewGuid();
            var firedEvent = Assert.Raises<EventArgs<Tuple<Guid, string>>>(
                f => _worker.Cancelled += f,
                f => _worker.Cancelled -= f,
                () => _worker.Cancel(jobId, string.Empty));

            Assert.NotNull(firedEvent);
            Assert.Equal(jobId, firedEvent.Arguments.Item.Item1);
        }

        [Fact]
        public void CancelWithNoAvailableHostThrows()
        {
            var jobId = Guid.NewGuid();
            var firedEvent = Assert.Raises<EventArgs<Guid>>(
                f => _worker.HostNotAvailable += f,
                f => _worker.HostNotAvailable -= f,
                () => _worker.Cancel(jobId, "host"));

            Assert.NotNull(firedEvent);
            Assert.Equal(jobId, firedEvent.Arguments.Item);
        }
    }
}