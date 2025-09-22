namespace DHI.Services.Jobs.Test
{
	using DHI.Services;
	using System;
    using System.Collections.Generic;
    using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
    using Workflows;
    using Xunit;

    public class RoundRobinLoadBalancerTest
    {
        [Theory]
        [AutoJobWorkerData]
        public void GetHostWithNoHostGroupFromGroupedHostServiceThrows(JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
        {
            var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            Assert.Throws<Exception>(() => loadBalancer.GetHost(Guid.NewGuid()));
        }

        [Theory]
        [AutoJobWorkerData]
        public void GetHostWithHostGroupFromNonGroupedHostServiceThrows(JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
        {
            var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            Assert.Throws<ArgumentException>(() => loadBalancer.GetHost(Guid.NewGuid(), "MyGroup"));
        }

        [Theory]
        [AutoJobWorkerData]
        public void GetHostByNonExistingGroupThrows(JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
        {
            var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            Assert.Throws<KeyNotFoundException>(() => loadBalancer.GetHost(Guid.NewGuid(), "NonExistingGroup"));
        }

		[Theory]
		[AutoJobWorkerData]
		public void GetHostIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
		{
			var host1 = new Host("194.123.123.123", "Host 1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2") { Priority = 2, RunningJobsLimit = 2 };
			hostService.Add(host1);
			hostService.Add(host2);
			var task = taskService.GetAll().First();
			var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

			var jobId = Guid.NewGuid();
			Assert.Equal(host1.Id, loadBalancer.GetHost(jobId).Value.Id);

			jobId = Guid.NewGuid();
			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host1.Id });
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);

			jobId = Guid.NewGuid();
			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host2.Id });
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);

			jobId = Guid.NewGuid();
			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host2.Id });
			Assert.False(loadBalancer.GetHost(jobId).HasValue);

			host2.RunningJobsLimit = 3;
			hostService.Update(host2);
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);
		}

		[Theory]
		[AutoJobWorkerData]
		public void GetHostByGroupIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
		{
			var host1 = new Host("194.123.123.123", "Host 1", "group1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2", "group1") { Priority = 2, RunningJobsLimit = 2 };
			var host3 = new Host("194.345.345.345", "Host 3", "group2") { Priority = 1, RunningJobsLimit = 1 };
			hostService.Add(host1);
			hostService.Add(host2);
			hostService.Add(host3);
			var task = taskService.GetAll().First();
			var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

			var jobId = Guid.NewGuid();
			Assert.Equal(host1.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

			jobId = Guid.NewGuid();
			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host1.Id });
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

			jobId = Guid.NewGuid();
			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host2.Id });
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

			jobId = Guid.NewGuid();
			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host2.Id });
			Assert.False(loadBalancer.GetHost(jobId, "group1").HasValue);
			Assert.Equal(host3.Id, loadBalancer.GetHost(jobId, "group2").Value.Id);

			host2.RunningJobsLimit = 3;
			hostService.Update(host2);
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);
		}

		[Theory]
        [AutoJobWorkerData]
        public void GetHostAlternatesBetweenAvailableHostsIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
        {
			var host1 = new Host("194.123.123.123", "Host 1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2") { Priority = 1, RunningJobsLimit = 1 };
			hostService.Add(host1);
			hostService.Add(host2);
			var task = taskService.GetAll().First();
            var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

			// assignment alternates between host1 and host2
			var jobId = Guid.NewGuid();
			Assert.Equal(host1.Id, loadBalancer.GetHost(jobId).Value.Id);

			jobId = Guid.NewGuid();
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);

			jobId = Guid.NewGuid();
			Assert.Equal(host1.Id, loadBalancer.GetHost(jobId).Value.Id);

			jobId = Guid.NewGuid();
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);
		}

        [Theory]
        [AutoJobWorkerData]
        public void GetHostByGroupAlternatesBetweenHostsIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
        {
            var host1 = new Host("194.123.123.123", "Host 1", "group1") {Priority = 1, RunningJobsLimit = 1};
            var host2 = new Host("194.234.234.234", "Host 2", "group1") {Priority = 2, RunningJobsLimit = 2};
            var host3 = new Host("194.345.345.345", "Host 3", "group2") {Priority = 1, RunningJobsLimit = 1};
            var host4 = new Host("194.345.345.346", "Host 4", "group2") {Priority = 1, RunningJobsLimit = 1};

			hostService.Add(host1);
            hostService.Add(host2);
            hostService.Add(host3);
            hostService.Add(host4);

			var task = taskService.GetAll().First();
            var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

            var jobId = Guid.NewGuid();                        
			Assert.Equal(host1.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

			jobId = Guid.NewGuid();
			Assert.Equal(host1.Id, loadBalancer.GetHost(jobId, "group1").Value.Id); // still 1 due to priority

			jobService.Add(new Job(jobId, task.Id) { Status = JobStatus.InProgress, HostId = host1.Id });

			jobId = Guid.NewGuid();
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id); // now 2 due to host1 being busy

			//group2 hosts should alternate
			jobId = Guid.NewGuid();
			Assert.Equal(host3.Id, loadBalancer.GetHost(jobId, "group2").Value.Id); 

			jobId = Guid.NewGuid();
			Assert.Equal(host4.Id, loadBalancer.GetHost(jobId, "group2").Value.Id);

			jobId = Guid.NewGuid();
			Assert.Equal(host3.Id, loadBalancer.GetHost(jobId, "group2").Value.Id);
		}

        [Theory]
        [AutoJobWorkerData]
        public async Task GetHostStartsCloudInstance(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, HostService hostService)
        {
            var host = new Host("host", "Host")
            {
                Priority = 1,
                RunningJobsLimit = 2,
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = {{"HostId", Guid.NewGuid().ToString()}}
            };

            hostService.Add(host);
            var worker = new FakeWorker(false);
            var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            var jobId = Guid.NewGuid();

            var hostId = loadBalancer.GetHost(jobId).Value.Id;

            hostService.TryGet(hostId, out var j);
            Assert.Equal(CloudInstanceStatus.Starting, j.CloudInstanceHandler.GetStatus());

            hostService.TryGet(hostId, out var jb);
            var task = taskService.GetAll().First();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.Pending, HostId = host.Id});
            Assert.Equal(host.Id, loadBalancer.GetHost(jobId).Value.Id);
            await Task.Delay(1500);
            Assert.Equal(CloudInstanceStatus.Running, jb.CloudInstanceHandler.GetStatus());
        }

        [Theory, AutoJobWorkerData]
        public void CreateNonGenericIsOk(string id, IWorker<Guid, string> worker, IJobRepository<Guid, string> jobRepository, HostService hostService)
        {
            hostService.Add(new Host("MyHost", "My host"));
            var jobService = new JobService(jobRepository, new CodeWorkflowService(new FakeCodeWorkflowRepository()));
            var loadBalancer = new RoundRobinLoadBalancer(id, worker, jobService, hostService);
            Assert.True(loadBalancer.GetHost(Guid.NewGuid()).HasValue);
        }

		[Theory]
		[AutoJobWorkerData]
		public void RoundRobinReturnsFalseIfAllHostsAreBusy(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
		{
			var host1 = new Host("194.123.123.123", "Host 1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2") { Priority = 1, RunningJobsLimit = 1 };
			hostService.Add(host1);
			hostService.Add(host2);
			var task = taskService.GetAll().First();
			var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

			//A
			jobService.Add(new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.Pending, HostId = host1.Id });
			jobService.Add(new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.Pending, HostId = host2.Id });

			var jobId = Guid.NewGuid();
			Assert.False(loadBalancer.GetHost(jobId).HasValue);
		}


		[Theory]
		[AutoJobWorkerData]
		public void RoundRobinReturnsFalseIfAllHostsDoNotRespondInTime(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
		{
			var host1 = new Host("194.123.123.123", "Host 1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2") { Priority = 1, RunningJobsLimit = 1 };
			hostService.Add(host1);
			hostService.Add(host2);
			var task = taskService.GetAll().First();
			var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService, hostResponseThreshold: TimeSpan.Zero);

			var jobId = Guid.NewGuid();
			Assert.False(loadBalancer.GetHost(jobId).HasValue);
		}

		[Theory]
		[AutoJobWorkerData]
		public void RoundRobinReturnsFalseIfAllHostsDoNotRespondInTime2(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, HostService hostService)
		{
			var host1 = new Host("194.123.123.123", "Host 1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2") { Priority = 1, RunningJobsLimit = 1 };
			hostService.Add(host1);
			hostService.Add(host2);
			var task = taskService.GetAll().First();

			var worker = new DelayingWorker(new Dictionary<string, TimeSpan>
			{
				{ host1.Id, TimeSpan.FromSeconds(3) },
				{ host2.Id, TimeSpan.FromSeconds(1) }
			});

			var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

			var jobId = Guid.NewGuid();
			var selectedHostId = loadBalancer.GetHost(jobId);

			Assert.True(selectedHostId.HasValue);
			Assert.Equal(host2.Id, selectedHostId.Value.Id);
		}

		private class DelayingWorker : IRemoteWorker<Guid, string>
		{
			private readonly Dictionary<string, TimeSpan> _delays;
			private int _index = 0;

			public event EventHandler<EventArgs<Tuple<Guid, JobStatus, string>>> Executed;
			public event EventHandler<EventArgs<Tuple<Guid, string>>> Executing;
			public event EventHandler<EventArgs<Tuple<Guid, string>>> Cancelled;
			public event EventHandler<EventArgs<Guid>> Cancelling;
			public event EventHandler<EventArgs<Tuple<Guid, Progress>>> ProgressChanged;
			public event EventHandler<EventArgs<Guid>> HostNotAvailable;

			public DelayingWorker(Dictionary<string, TimeSpan> delays)
			{
				_delays = delays;
			}

			public void Cancel(Guid jobId, string hostId = null)
			{			
			}

			public void Execute(Guid jobId, ITask<string> taskId, Dictionary<string, object> parameters, string hostId = null)
			{				
			}

			public bool IsHostAvailable(string hostId)
			{
				Thread.Sleep(_delays[hostId]);
				return true;
			}

			public void Timeout(Guid jobId, string hostId = null)
			{				
			}
		}

		[Theory]
		[AutoJobWorkerData]
		public void RoundRobinReturnsOkIfOneHostIsBusy(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
		{
			var host1 = new Host("194.123.123.123", "Host 1") { Priority = 1, RunningJobsLimit = 1 };
			var host2 = new Host("194.234.234.234", "Host 2") { Priority = 1, RunningJobsLimit = 1 };
			hostService.Add(host1);
			hostService.Add(host2);
			var task = taskService.GetAll().First();
			var loadBalancer = new RoundRobinLoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

			//A
			jobService.Add(new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.Pending, HostId = host1.Id });

			var jobId = Guid.NewGuid();
			Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);
		}
	}
}