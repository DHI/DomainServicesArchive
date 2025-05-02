namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounts;
    using Jobs;
    using Workflows;
    using Xunit;

    public class JobServiceTest
    {
        private const int RepeatCount = 10;

        [Theory, AutoJobData]
        public void CreateWithNullRepositoryThrows(ITaskService<FakeTask<string>, string> taskService, AccountService accountService)
        {
            Assert.Throws<ArgumentNullException>(() => new JobService<FakeTask<string>, string>(null, taskService, accountService));
        }

        [Theory, AutoJobData]
        public void GetNonExistingThrows(JobService<FakeTask<string>, string> jobService)
        {
            Assert.False(jobService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoJobData]
        public void UpdateNonExistingThrows(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id);
            Assert.Throws<KeyNotFoundException>(() => jobService.Update(job));
        }

        [Theory, AutoJobData]
        public void UpdateStatusForNonExistingThrows(JobService<FakeTask<string>, string> jobService)
        {
            Assert.Throws<KeyNotFoundException>(() => jobService.UpdateStatus(Guid.NewGuid(), JobStatus.Error));
        }

        [Theory, AutoJobData]
        public void AddWithNonExistingTaskThrows(JobService<FakeTask<string>, string> jobService)
        {
            var job = new Job(Guid.NewGuid(), "NonExistingTaskId");
            Assert.Throws<KeyNotFoundException>(() => jobService.Add(job));
        }

        [Theory, AutoJobData]
        public void AddWithNonExistingAccountThrows(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id, "NonExistingAccountId");
            Assert.Throws<KeyNotFoundException>(() => jobService.Add(job));
        }

        [Theory, AutoJobData]
        public void AddWithNonExistingParametersThrows(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id);
            job.Parameters.Add("NonExistingParameter", "SomeValue");
            Assert.Throws<KeyNotFoundException>(() => jobService.Add(job));
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateWithNonExistingTaskThrows(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            var updatedJob = new Job(job.Id, "NonExistingTask");
            Assert.Throws<KeyNotFoundException>(() => jobService.Update(updatedJob));
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateWithNonExistingAccountThrows(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            var updatedJob = new Job(job.Id, job.TaskId, "NonExistingAccountId");
            Assert.Throws<KeyNotFoundException>(() => jobService.Update(updatedJob));
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateWithNonExistingParametersThrows(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            var updatedJob = new Job(job.Id, job.TaskId, job.AccountId);
            job.Parameters.Add("NonExistingParameter", "SomeValue");
            Assert.Throws<KeyNotFoundException>(() => jobService.Update(updatedJob));
        }

        [Theory, AutoJobData]
        public void UpdateToStatusCancelIfNotInProgressOrPendingThrows(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.Completed };
            jobService.Add(job);
            var updatedJob = new Job(job.Id, task.Id) { Status = JobStatus.Cancel };

            Assert.Throws<ArgumentException>(() => jobService.Update(updatedJob));
        }

        [Theory, AutoJobData]
        public void UpdateStatusToCancelIfNotInProgressOrPendingThrows(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id) { Status = JobStatus.Completed };
            jobService.Add(job);

            Assert.Throws<ArgumentException>(() => jobService.UpdateStatus(job.Id, JobStatus.Cancel));
        }

        [Theory, AutoJobData(RepeatCount)]
        public void GetAllIsOk(JobService<FakeTask<string>, string> jobService)
        {
            Assert.Equal(RepeatCount, jobService.GetAll().Count());
        }

        [Theory, AutoJobData(RepeatCount)]
        public void GetIdsIsOk(JobService<FakeTask<string>, string> jobService)
        {
            Assert.Equal(RepeatCount, jobService.GetIds().Count());
        }

        [Theory, AutoJobData]
        public void AddAndGetIsOk(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var account = accountService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id, account.Id);
            jobService.Add(job);

            jobService.TryGet(job.Id, out var jb);
            Assert.Equal(job.Id, jb.Id);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void CountIsOk(JobService<FakeTask<string>, string> jobService)
        {
            Assert.Equal(RepeatCount, jobService.Count());
        }

        [Theory, AutoJobData(RepeatCount)]
        public void ExistsIsOk(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            Assert.True(jobService.Exists(job.Id));
        }

        [Theory, AutoJobData(RepeatCount)]
        public void DoesNotExistIsOk(JobService<FakeTask<string>, string> jobService)
        {
            Assert.False(jobService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoJobData]
        public void EventsAreRaisedOnAdd(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var raisedEvents = new List<string>();
            jobService.Adding += (_, _) => { raisedEvents.Add("Adding"); };
            jobService.Added += (_, _) => { raisedEvents.Add("Added"); };

            var task = taskService.GetAll().First();
            var account = accountService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id, account.Id);
            jobService.Add(job);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void RemoveIsOk(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            jobService.Remove(job.Id);

            Assert.False(jobService.Exists(job.Id));
            Assert.Equal(RepeatCount - 1, jobService.Count());
        }

        [Theory, AutoJobData]
        public void RemoveByFilterIsOk(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var task1 = taskService.GetAll().First();
            var task2 = taskService.GetAll().Last();
            var account1 = accountService.GetAll().First();
            var account2 = accountService.GetAll().Last();

            var job1 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task2.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            jobService.Add(job4);

            var job5 = new Job(Guid.NewGuid(), task1.Id, account2.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job5);

            jobService.Remove(before: new DateTime(2015, 01, 03));
            Assert.False(jobService.Exists(job1.Id));
            Assert.Equal(3, jobService.Count());
        }

        [Theory, AutoJobData(RepeatCount)]
        public void RemoveWithNoFilterRemovesNothing(JobService<FakeTask<string>, string> jobService)
        {
            jobService.Remove();

            Assert.Equal(RepeatCount, jobService.Count());
        }

        [Theory, AutoJobData(RepeatCount)]
        public void EventsAreRaisedOnRemove(JobService<FakeTask<string>, string> jobService)
        {
            var raisedEvents = new List<string>();
            jobService.Deleting += (_, _) => { raisedEvents.Add("Deleting"); };
            jobService.Deleted += (_, _) => { raisedEvents.Add("Deleted"); };

            var job = jobService.GetAll().First();
            jobService.Remove(job.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void EventsAreRaisedOnRemoveByFilter(JobService<FakeTask<string>, string> jobService)
        {
            var raisedEvents = new List<string>();
            jobService.DeletingMultiple += (_, _) => { raisedEvents.Add("DeletingMultiple"); };
            jobService.DeletedMultiple += (_, _) => { raisedEvents.Add("DeletedMultiple"); };

            var job = jobService.GetAll().First();
            jobService.Remove(accountId: job.AccountId);

            Assert.Equal("DeletingMultiple", raisedEvents[0]);
            Assert.Equal("DeletedMultiple", raisedEvents[1]);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateIsOk(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var task = taskService.GetAll().First();
            var job = jobService.GetAll().First();
            var updatedJob = new Job(job.Id, task.Id) { Tag = "SomeTag" };
            jobService.Update(updatedJob);

            jobService.TryGet(job.Id, out var jb);
            Assert.Equal("SomeTag", jb.Tag);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateStatusIsOk(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            jobService.UpdateStatus(job.Id, JobStatus.Error);

            jobService.TryGet(job.Id, out var jb);
            Assert.Equal(JobStatus.Error, jb.Status);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateStatusToInProgressSetsStartedDate(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            var utcBeforeUpdate = DateTime.UtcNow;
            jobService.UpdateStatus(job.Id, JobStatus.InProgress);

            jobService.TryGet(job.Id, out var jobAfterUpdate);
            Assert.Equal(JobStatus.InProgress, jobAfterUpdate.Status);
            Assert.True(utcBeforeUpdate < jobAfterUpdate.Started);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateStatusToCompletedSetsFinishedDate(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            var utcBeforeUpdate = DateTime.UtcNow;
            jobService.UpdateStatus(job.Id, JobStatus.Completed);

            jobService.TryGet(job.Id, out var jobAfterUpdate);
            Assert.Equal(JobStatus.Completed, jobAfterUpdate.Status);
            Assert.True(utcBeforeUpdate < jobAfterUpdate.Finished);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void UpdateStatusToErrorSetsFinishedDate(JobService<FakeTask<string>, string> jobService)
        {
            var job = jobService.GetAll().First();
            var utcBeforeUpdate = DateTime.UtcNow;
            jobService.UpdateStatus(job.Id, JobStatus.Error);

            jobService.TryGet(job.Id, out var jobAfterUpdate);
            Assert.Equal(JobStatus.Error, jobAfterUpdate.Status);
            Assert.True(utcBeforeUpdate < jobAfterUpdate.Finished);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void AddOrUpdateIsOk(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var raisedEvents = new List<string>();
            jobService.Added += (_, _) => { raisedEvents.Add("Added"); };
            jobService.Updated += (_, _) => { raisedEvents.Add("Updated"); };
            var task = taskService.GetAll().First();
            var account = accountService.GetAll().First();
            var job = new Job(Guid.NewGuid(), task.Id, account.Id);
            jobService.AddOrUpdate(job);
            var updated = new Job(job.Id, job.TaskId, job.AccountId) { Tag = "SomeTag" };
            jobService.AddOrUpdate(updated);

            jobService.TryGet(job.Id, out var jb);
            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal("SomeTag", jb.Tag);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void EventsAreRaisedOnUpdate(TaskService<FakeTask<string>, string> taskService, JobService<FakeTask<string>, string> jobService)
        {
            var raisedEvents = new List<string>();
            jobService.Updating += (_, _) => { raisedEvents.Add("Updating"); };
            jobService.Updated += (_, _) => { raisedEvents.Add("Updated"); };

            var task = taskService.GetAll().First();
            var job = jobService.GetAll().First();
            var updatedJob = new Job(job.Id, task.Id) { Tag = "SomeTag" };
            jobService.Update(updatedJob);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoJobData(RepeatCount)]
        public void EventsAreRaisedOnUpdateStatus(JobService<FakeTask<string>, string> jobService)
        {
            var raisedEvents = new List<string>();
            jobService.Updating += (_, _) => { raisedEvents.Add("Updating"); };
            jobService.Updated += (_, _) => { raisedEvents.Add("Updated"); };

            var job = jobService.GetAll().First();
            jobService.UpdateStatus(job.Id, JobStatus.Error);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoJobData]
        public void GetOverloadIsOk(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var task1 = taskService.GetAll().First();
            var task2 = taskService.GetAll().Last();
            var account1 = accountService.GetAll().First();
            var account2 = accountService.GetAll().Last();

            var job1 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task2.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            jobService.Add(job4);

            var job5 = new Job(Guid.NewGuid(), task1.Id, account2.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job5);

            Assert.Equal(4, jobService.Get(accountId: account1.Id).Count());
            Assert.Equal(3, jobService.Get(accountId: account1.Id, since: new DateTime(2015, 01, 03)).Count());
            Assert.Equal(2, jobService.Get(accountId: account1.Id, since: new DateTime(2015, 01, 03), tag: "Tag1").Count());
            Assert.Equal(3, jobService.Get(accountId: account1.Id, taskId: task1.Id).Count());
            Assert.Single(jobService.Get(status: JobStatus.Error));
            Assert.Empty(jobService.Get(status: JobStatus.Pending));
            Assert.Single(jobService.Get(accountId: account2.Id));
        }

        [Theory, AutoJobData]
        public void GetByQueryIsOk(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var task1 = taskService.GetAll().First();
            var task2 = taskService.GetAll().Last();
            var account1 = accountService.GetAll().First();
            var account2 = accountService.GetAll().Last();

            var job1 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task2.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            jobService.Add(job4);

            var job5 = new Job(Guid.NewGuid(), task1.Id, account2.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job5);

            var query = new Query<Job<Guid, string>>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, account1.Id)
            };
            Assert.Equal(4, jobService.Get(query).Count());

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, account1.Id),
                new QueryCondition("Requested", QueryOperator.GreaterThanOrEqual, new DateTime(2015, 01, 03))
            };
            Assert.Equal(3, jobService.Get(query).Count());

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, account1.Id),
                new QueryCondition("Requested", QueryOperator.GreaterThanOrEqual, new DateTime(2015, 01, 03)),
                new QueryCondition("Tag", QueryOperator.Equal, "Tag1")
            };
            Assert.Equal(2, jobService.Get(query).Count());

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, account1.Id),
                new QueryCondition("TaskId", QueryOperator.Equal, task1.Id)
            };
            Assert.Equal(3, jobService.Get(query).Count());

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("Status", QueryOperator.Equal, JobStatus.Error)
            };
            Assert.Single(jobService.Get(query));

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("Status", QueryOperator.Equal, JobStatus.Pending)
            };
            Assert.Empty(jobService.Get(query));

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("AccountId", QueryOperator.Equal, account2.Id)
            };
            Assert.Single(jobService.Get(query));
        }

        [Theory, AutoJobData]
        public void GetLastIsOk(TaskService<FakeTask<string>, string> taskService, AccountService accountService, JobService<FakeTask<string>, string> jobService)
        {
            var task1 = taskService.GetAll().First();
            var task2 = taskService.GetAll().Last();
            var account1 = accountService.GetAll().First();
            var account2 = accountService.GetAll().Last();

            var job1 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job1);

            var job2 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            jobService.Add(job2);

            var job3 = new Job(Guid.NewGuid(), task2.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            jobService.Add(job3);

            var job4 = new Job(Guid.NewGuid(), task1.Id, account1.Id)
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            jobService.Add(job4);

            var job5 = new Job(Guid.NewGuid(), task1.Id, account2.Id)
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            jobService.Add(job5);

            Assert.Equal(job4.Id, jobService.GetLast(accountId: account1.Id).Id);
            Assert.Equal(job2.Id, jobService.GetLast(accountId: account1.Id, status: JobStatus.Completed, tag: "Tag1").Id);
            Assert.Equal(job5.Id, jobService.GetLast(accountId: account2.Id).Id);
            Assert.Null(jobService.GetLast(taskId: "NonExistingTask"));
        }

        [Theory, AutoJobData(RepeatCount)]
        public void GetWithNoFilterReturnsAll(JobService<FakeTask<string>, string> jobService)
        {
            Assert.Equal(RepeatCount, jobService.Get().Count());
        }

        [Theory, AutoJobData]
        public void CreateNonGenericIsOk(IJobRepository<Guid, string> jobRepository)
        {
            var service = new JobService(jobRepository, new CodeWorkflowService(new FakeCodeWorkflowRepository()));
            Assert.Equal(0, service.Count());
        }
    }
}