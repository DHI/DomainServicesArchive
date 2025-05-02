namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Workflows;
    using Xunit;

    public class LoadBalancerTest
    {
        [Theory]
        [AutoJobWorkerData]
        public void GetHostWithNoHostGroupFromGroupedHostServiceThrows(JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
        {
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            Assert.Throws<Exception>(() => loadBalancer.GetHost(Guid.NewGuid()));
        }

        [Theory]
        [AutoJobWorkerData]
        public void GetHostWithHostGroupFromNonGroupedHostServiceThrows(JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
        {
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            Assert.Throws<ArgumentException>(() => loadBalancer.GetHost(Guid.NewGuid(), "MyGroup"));
        }

        [Theory]
        [AutoJobWorkerData]
        public void GetHostByNonExistingGroupThrows(JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
        {
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            Assert.Throws<KeyNotFoundException>(() => loadBalancer.GetHost(Guid.NewGuid(), "NonExistingGroup"));
        }

        [Theory]
        [AutoJobWorkerData]
        public void GetHostIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, HostService hostService)
        {
            var host1 = new Host("194.123.123.123", "Host 1") {Priority = 1, RunningJobsLimit = 1};
            var host2 = new Host("194.234.234.234", "Host 2") {Priority = 2, RunningJobsLimit = 2};
            hostService.Add(host1);
            hostService.Add(host2);
            var task = taskService.GetAll().First();
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

            var jobId = Guid.NewGuid();
            Assert.Equal(host1.Id, loadBalancer.GetHost(jobId).Value.Id);

            jobId = Guid.NewGuid();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.InProgress, HostId = host1.Id});
            Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);

            jobId = Guid.NewGuid();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.InProgress, HostId = host2.Id});
            Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);

            jobId = Guid.NewGuid();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.InProgress, HostId = host2.Id});
            Assert.False(loadBalancer.GetHost(jobId).HasValue);

            host2.RunningJobsLimit = 3;
            hostService.Update(host2);
            Assert.Equal(host2.Id, loadBalancer.GetHost(jobId).Value.Id);
        }

        [Theory]
        [AutoJobWorkerData]
        public void GetHostByGroupIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker, GroupedHostService hostService)
        {
            var host1 = new Host("194.123.123.123", "Host 1", "group1") {Priority = 1, RunningJobsLimit = 1};
            var host2 = new Host("194.234.234.234", "Host 2", "group1") {Priority = 2, RunningJobsLimit = 2};
            var host3 = new Host("194.345.345.345", "Host 3", "group2") {Priority = 1, RunningJobsLimit = 1};
            hostService.Add(host1);
            hostService.Add(host2);
            hostService.Add(host3);
            var task = taskService.GetAll().First();
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);

            var jobId = Guid.NewGuid();
            Assert.Equal(host1.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

            jobId = Guid.NewGuid();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.InProgress, HostId = host1.Id});
            Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

            jobId = Guid.NewGuid();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.InProgress, HostId = host2.Id});
            Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);

            jobId = Guid.NewGuid();
            jobService.Add(new Job(jobId, task.Id) {Status = JobStatus.InProgress, HostId = host2.Id});
            Assert.False(loadBalancer.GetHost(jobId, "group1").HasValue);
            Assert.Equal(host3.Id, loadBalancer.GetHost(jobId, "group2").Value.Id);

            host2.RunningJobsLimit = 3;
            hostService.Update(host2);
            Assert.Equal(host2.Id, loadBalancer.GetHost(jobId, "group1").Value.Id);
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
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
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
            var loadBalancer = new LoadBalancer(id, worker, jobService, hostService);
            Assert.True(loadBalancer.GetHost(Guid.NewGuid()).HasValue);
        }
    }
}