namespace DHI.Services.Jobs.Test
{
    using Jobs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Workflows;
    using Xunit;

    public class JobWorkerTest
    {
        [Theory, AutoJobWorkerData]
        public void CreateWithNullWorkerThrows(string id, JobService<FakeTask<string>, string> jobService, TaskService<FakeTask<string>, string> taskService)
        {
            Assert.Throws<ArgumentNullException>(() => new JobWorker<FakeTask<string>, string>(id, null, taskService, jobService));
        }

        [Theory, AutoJobWorkerData]
        public void CreateWithNullTaskServiceThrows(string id, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            Assert.Throws<ArgumentNullException>(() => new JobWorker<FakeTask<string>, string>(id, worker, null, jobService));
        }

        [Theory, AutoJobWorkerData]
        public void CreateWithNullJobServiceThrows(string id, IWorker<Guid, string> worker, TaskService<FakeTask<string>, string> taskService)
        {
            Assert.Throws<ArgumentNullException>(() => new JobWorker<FakeTask<string>, string>(id, worker, taskService, null));
        }

        [Theory, AutoJobWorkerData]
        public void ExecutePendingForNonExistingTaskSetsStatusToError(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Pending
            };
            jobService.Add(job);
            taskService.Remove(task.Id);
            jobWorker.ExecutePending();

            jobService.TryGet(job.Id, out var jb);
            Assert.Equal(JobStatus.Error, jb.Status);
        }

        [Theory, AutoJobWorkerData]
        public void ExecutePendingIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job1 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Pending,
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.InProgress
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Pending
            };
            jobService.Add(job4);

            var pendingJobs = new[] { job2.Id, job4.Id };
            jobWorker.Executed += (sender, args) => { Assert.Contains(args.Item.Item1, pendingJobs); };

            jobWorker.ExecutePending();
        }

        [Theory, AutoJobWorkerData]
        public void CancelIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var raisedEvents = new List<string>();
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.Pending };
            jobService.Add(job);

            jobWorker.Executing += (sender, args) =>
            {
                raisedEvents.Add("Executing");
                Assert.Equal(job.Id, args.Item.Id);

                // Force cancellation
                job.Status = JobStatus.Cancel;
                jobService.Update(job);
                Thread.Sleep(1000);
                jobWorker.Cancel();
            };

            jobWorker.Executed += (sender, args) => { raisedEvents.Add("Executed"); };

            jobWorker.Cancelling += (sender, args) =>
            {
                raisedEvents.Add("Cancelling");
                Assert.Equal(job.Id, args.Item.Id);
                Assert.Equal(JobStatus.Cancelling, args.Item.Status);
            };

            jobWorker.Cancelled += (sender, args) =>
            {
                Assert.Equal(job.Id, args.Item.Item1);
                Assert.Contains("Executing", raisedEvents);
                Assert.Contains("Cancelling", raisedEvents);
                Assert.DoesNotContain("Executed", raisedEvents);
            };

            jobWorker.ExecutePending();
        }

        [Theory, AutoJobWorkerData]
        public void ExecutePendingWithExplicitPriorityIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job1 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Priority = 2,
                Status = JobStatus.Pending
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.InProgress
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Pending
            };
            jobService.Add(job4);

            var pendingJobs = new[] { job2.Id, job4.Id };
            jobWorker.Executed += (sender, args) => { Assert.Contains(args.Item.Item1, pendingJobs); };
            jobWorker.ExecutePending();
        }

        [Theory, AutoJobWorkerData]
        public void CleanLongRunningJobsIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job1 = new Job(Guid.NewGuid(), task.Id)
            {
                Started = new DateTime(2015, 01, 02).ToUniversalTime(),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task.Id)
            {
                Started = new DateTime(2015, 01, 03).ToUniversalTime(),
                Status = JobStatus.Pending
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task.Id)
            {
                Started = DateTime.UtcNow.Subtract(task.Timeout.Value - TimeSpan.FromHours(1)),
                Status = JobStatus.InProgress
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task.Id)
            {
                Started = new DateTime(2015, 01, 04).ToUniversalTime(),
                Status = JobStatus.Error
            };
            jobService.Add(job4);

            var job5 = new Job(Guid.NewGuid(), task.Id)
            {
                Started = DateTime.UtcNow.Subtract(task.Timeout.Value + TimeSpan.FromHours(1)),
                Status = JobStatus.InProgress
            };
            jobService.Add(job5);

            jobWorker.CleanLongRunningJobs();

            jobService.TryGet(job5.Id, out var jb);
            Assert.Equal(JobStatus.Error, jb.Status);
            Assert.Equal(2, jobService.Get(status: JobStatus.Error).Count());
            Assert.Single(jobService.Get(status: JobStatus.Completed));
            Assert.Single(jobService.Get(status: JobStatus.Pending));
            Assert.Single(jobService.Get(status: JobStatus.InProgress));
        }

        [Theory, AutoJobWorkerData]
        public void CleanNotStartedJobsIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job1 = new Job(Guid.NewGuid(), task.Id)
            {
                Starting = new DateTime(2015, 01, 02).ToUniversalTime(),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task.Id)
            {
                Starting = new DateTime(2015, 01, 03).ToUniversalTime(),
                Status = JobStatus.Pending
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task.Id)
            {
                Starting = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                Status = JobStatus.Starting
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task.Id)
            {
                Starting = new DateTime(2015, 01, 04).ToUniversalTime(),
                Status = JobStatus.Error
            };
            jobService.Add(job4);

            var job5 = new Job(Guid.NewGuid(), task.Id)
            {
                Starting = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(3)),
                Status = JobStatus.Starting
            };
            jobService.Add(job5);

            jobWorker.CleanNotStartedJobs();

            jobService.TryGet(job5.Id, out var jb);
            Assert.Equal(JobStatus.Error, jb.Status);
            Assert.Equal(2, jobService.Get(status: JobStatus.Error).Count());
            Assert.Single(jobService.Get(status: JobStatus.Completed));
            Assert.Single(jobService.Get(status: JobStatus.Pending));
            Assert.Single(jobService.Get(status: JobStatus.Starting));
        }

        [Theory, AutoJobWorkerData]
        public void ExecutePendingForCloudInstancesIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, HostService hostService)
        {
            var host = new Host("myHost", "MyHost")
            {
                Priority = 1,
                RunningJobsLimit = 2,
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = { { "HostId", Guid.NewGuid().ToString() } }
            };

            hostService.Add(host);
            var task = taskService.GetAll().First();
            var worker = new FakeWorker(false);
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService, hostService, loadBalancer: loadBalancer);

            var job1 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Pending
            };
            jobService.Add(job2);

            jobWorker.Executing += (sender, args) =>
            {
                // job2 is executing
                var job = args.Item;
                jobService.TryGet(job.Id, out var j);
                Assert.Equal(JobStatus.InProgress, j.Status);
            };

            jobWorker.Executed += (sender, args) =>
            {
                // job2 finished successfully
                var jobId = args.Item.Item1;
                var status = args.Item.Item2;
                Assert.Equal(job2.Id, jobId);
                Assert.Equal(JobStatus.Completed, status);
                var completed = new Query<Job<Guid, string>>(new QueryCondition("Status", JobStatus.Completed));
                var completedJobs = jobService.Get(completed);
                Assert.Contains(completedJobs, job => job.Id == job2.Id);

                // Wait for cloud instance to close down
                Thread.Sleep(1500);

                // Cloud instance is successfully stopped
                Assert.Equal(CloudInstanceStatus.Stopped, host.CloudInstanceHandler.GetStatus());
            };

            // Try the first time
            jobWorker.ExecutePending();

            jobService.TryGet(job2.Id, out var jb);
            // job2 is allocated to host
            Assert.Equal(host.Id, jb.HostId);

            // However, job2 is still pending, because cloud instance has to start first
            Assert.Equal(JobStatus.Pending, jb.Status);

            // Cloud instance is starting up
            Assert.Equal(CloudInstanceStatus.Starting, host.CloudInstanceHandler.GetStatus());

            // Wait for cloud instance to start up
            Thread.Sleep(1500);

            // Cloud instance is now running
            Assert.Equal(CloudInstanceStatus.Running, host.CloudInstanceHandler.GetStatus());

            // Try the second time time
            jobWorker.ExecutePending();
        }

        [Theory, AutoJobWorkerData]
        public void ExecutePendingForGroupedCloudInstancesIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, GroupedHostService hostService)
        {
            var host = new Host("host", "host", "group1")
            {
                Priority = 1,
                RunningJobsLimit = 2,
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = { { "HostId", Guid.NewGuid().ToString() } }
            };

            hostService.Add(host);
            var task = taskService.GetAll().First();
            var worker = new FakeWorker(false);
            var loadBalancer = new LoadBalancer<FakeTask<string>, string>("Test Worker", worker, jobService, hostService);
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService, hostService, loadBalancer: loadBalancer);

            var job1 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed,
                HostGroup = host.Group
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Pending,
                HostGroup = host.Group
            };
            jobService.Add(job2);

            jobWorker.Executing += (sender, args) =>
            {
                // job2 is executing
                var job = args.Item;
                jobService.TryGet(job.Id, out var j);
                Assert.Equal(JobStatus.InProgress, j.Status);
            };

            jobWorker.Executed += (sender, args) =>
            {
                // job2 finished successfully
                var jobId = args.Item.Item1;
                var status = args.Item.Item2;
                Assert.Equal(job2.Id, jobId);
                Assert.Equal(JobStatus.Completed, status);
                var completed = new Query<Job<Guid, string>>(new QueryCondition("Status", JobStatus.Completed));
                var completedJobs = jobService.Get(completed);
                Assert.Contains(completedJobs, job => job.Id == job2.Id);

                // Wait for cloud instance to close down
                Thread.Sleep(1500);

                // Cloud instance is successfully stopped
                Assert.Equal(CloudInstanceStatus.Stopped, host.CloudInstanceHandler.GetStatus());
            };

            // Try the first time
            jobWorker.ExecutePending();

            jobService.TryGet(job2.Id, out var jb);
            // job2 is allocated to host
            Assert.Equal(host.Id, jb.HostId);

            // However, job2 is still pending, because cloud instance has to start first
            Assert.Equal(JobStatus.Pending, jb.Status);

            // Cloud instance is starting up
            Assert.Equal(CloudInstanceStatus.Starting, host.CloudInstanceHandler.GetStatus());

            // Wait for cloud instance to start up
            Thread.Sleep(1500);

            // Cloud instance is now running
            Assert.Equal(CloudInstanceStatus.Running, host.CloudInstanceHandler.GetStatus());

            // Try the second time time
            jobWorker.ExecutePending();
        }

        [Theory, AutoJobWorkerData]
        public void CreateNonGenericIsOk(string id, IWorker<Guid, string> worker, IJobRepository<Guid, string> jobRepository)
        {
            var taskService = new CodeWorkflowService(new FakeCodeWorkflowRepository());
            var jobService = new JobService(jobRepository, new CodeWorkflowService(new FakeCodeWorkflowRepository()));
            var jobWorker = new JobWorker(id, worker, taskService, jobService);
            Assert.Equal(id, jobWorker.Id);
        }

        [Theory, AutoJobWorkerData]
        public void HeartbeatMonitorIsOk(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService, heartbeatTimeout: TimeSpan.FromMilliseconds(500));

            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.InProgress, Heartbeat = DateTime.UtcNow };
            jobService.Add(job);

            jobWorker.MonitorInProgressHeartbeat();

            jobService.TryGet(job.Id, out var updatedJob);

            Assert.Equal<JobStatus>(JobStatus.InProgress, updatedJob.Status);

            Thread.Sleep(500);

            jobWorker.MonitorInProgressHeartbeat();

            jobService.TryGet(job.Id, out updatedJob);

            Assert.Equal<JobStatus>(JobStatus.Error, updatedJob.Status);
        }

        [Theory, AutoJobWorkerData]
        public void TimeoutMonitorRespectsElapsedDynamicTimeout(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            (task as IDynamicTimeoutTask).WorkflowTimeout = TimeSpan.FromMilliseconds(500);
            task.Parameters.Add("WorkflowTimeout", typeof(TimeSpan));
            taskService.Update(task);

            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.InProgress, Heartbeat = DateTime.UtcNow, Started = DateTime.UtcNow.AddDays(-1) };
            job.Parameters.Add("WorkflowTimeout", TimeSpan.FromDays(1));

            jobService.Add(job);

            jobWorker.MonitorTimeouts();

            jobService.TryGet(job.Id, out var updatedJob);

            Assert.Equal<JobStatus>(JobStatus.TimedOut, updatedJob.Status);
        }

        [Theory, AutoJobWorkerData]
        public void TimeoutMonitorRespectsUnElapsedDynamicTimeout(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            (task as IDynamicTimeoutTask).WorkflowTimeout = TimeSpan.FromDays(1);
            task.Parameters.Add("WorkflowTimeout", typeof(TimeSpan));

            taskService.Update(task);

            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.InProgress, Heartbeat = DateTime.UtcNow, Started = DateTime.UtcNow };
            job.Parameters.Add("WorkflowTimeout", TimeSpan.FromDays(1));

            jobService.Add(job);

            jobWorker.MonitorTimeouts();

            jobService.TryGet(job.Id, out var updatedJob);

            Assert.Equal<JobStatus>(JobStatus.InProgress, updatedJob.Status);
        }

        [Theory, AutoJobWorkerData]
        public void TimeoutMonitorRespectsElapsedStaticTimeout(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            taskService.Update(task);
            task.Timeout = TimeSpan.FromMilliseconds(500);
            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.InProgress, Heartbeat = DateTime.UtcNow, Started = DateTime.UtcNow.AddDays(-1) };

            jobService.Add(job);

            jobWorker.MonitorTimeouts();

            jobService.TryGet(job.Id, out var updatedJob);

            Assert.Equal<JobStatus>(JobStatus.TimedOut, updatedJob.Status);
        }

        [Theory, AutoJobWorkerData]
        public void TimeoutMonitorRespectsUnElapsedStaticTimeout(string id, TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService, IWorker<Guid, string> worker)
        {
            var task = taskService.GetAll().First();
            task.Timeout = TimeSpan.FromDays(1);
            taskService.Update(task);

            var jobWorker = new JobWorker<FakeTask<string>, string>(id, worker, taskService, jobService);

            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.InProgress, Heartbeat = DateTime.UtcNow, Started = DateTime.UtcNow };

            jobService.Add(job);

            jobWorker.MonitorTimeouts();

            jobService.TryGet(job.Id, out var updatedJob);

            Assert.Equal<JobStatus>(JobStatus.InProgress, updatedJob.Status);
        }
    }
}