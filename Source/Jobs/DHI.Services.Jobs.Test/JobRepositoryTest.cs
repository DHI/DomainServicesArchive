namespace DHI.Services.Jobs.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Jobs;
    using Xunit;

    public class JobRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly JobRepository<Guid, string> _repository;

        public JobRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "jobs.json");
            File.Copy(@"../../../Data/jobs.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            _repository = new JobRepository<Guid, string>(_filePath);
        }

        [Theory]
        [AutoData]
        public void AddAndGetIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            var actual = _repository.Get(job.Id).Value;
            Assert.Equal(job.Id, actual.Id);
        }

        [Theory]
        [AutoData]
        public void ContainsIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            Assert.True(_repository.Contains(job.Id));
        }

        [Theory]
        [AutoData]
        public void CountIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            Assert.Equal(1, _repository.Count());
        }

        [Theory]
        [AutoData]
        public void GetAllIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            Assert.Single(_repository.GetAll());
        }

        [Theory]
        [AutoData]
        public void GetIdsIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            Assert.Equal(job.Id, _repository.GetIds().First());
        }

        [Theory]
        [AutoData]
        public void RemoveIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            _repository.Remove(job.Id);
            Assert.False(_repository.Contains(job.Id));
            Assert.Equal(0, _repository.Count());
        }

        [Theory]
        [AutoData]
        public void UpdateIsOk(Job<Guid, string> job)
        {
            _repository.Add(job);
            job.Status = JobStatus.Error;
            _repository.Update(job);
            Assert.Equal(JobStatus.Error, _repository.Get(job.Id).Value.Status);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new JobRepository<Guid, Guid>(null));
        }

        [Fact]
        public void CreateWithNonExistingFilePathThrows()
        {
            Assert.Throws<FileNotFoundException>(() => new JobRepository<Guid, Guid>("C:\\NonExistingFile.json"));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(_repository.Contains(Guid.NewGuid()));
        }

        [Fact]
        public void GetByExpressionIsOk()
        {
            var job1 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job1);

            var job2 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            _repository.Add(job2);

            var job3 = new Job<Guid, string>(Guid.NewGuid(), "Task2", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            _repository.Add(job3);

            var job4 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            _repository.Add(job4);

            var job5 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User2")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job5);

            Assert.Equal(4, _repository.Get(j => j.AccountId == "User1").Count());
            Assert.Equal(3, _repository.Get(j => j.AccountId == "User1" && j.Requested >= new DateTime(2015, 01, 03)).Count());
            Assert.Equal(2, _repository.Get(j => j.AccountId == "User1" && j.Requested >= new DateTime(2015, 01, 03) && j.Tag == "Tag1").Count());
            Assert.Equal(3, _repository.Get(j => j.AccountId == "User1" && j.TaskId == "Task1").Count());
            Assert.Single(_repository.Get(j => j.Status == JobStatus.Error));
            Assert.Empty(_repository.Get(j => j.Status == JobStatus.Pending));
            Assert.Single(_repository.Get(j => j.AccountId == "User2"));
        }

        [Fact]
        public void GetByQueryIsOk()
        {
            var job1 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job1);

            var job2 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            _repository.Add(job2);

            var job3 = new Job<Guid, string>(Guid.NewGuid(), "Task2", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            _repository.Add(job3);

            var job4 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            _repository.Add(job4);

            var job5 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User2")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job5);

            var query = new Query<Job<Guid, string>> { new QueryCondition("AccountId", "User1") };
            Assert.Equal(4, _repository.Get(query).Count());

            query.Add(new QueryCondition("Requested", QueryOperator.GreaterThanOrEqual, new DateTime(2015, 01, 03)));
            Assert.Equal(3, _repository.Get(query).Count());

            query.Add(new QueryCondition("Tag", "Tag1"));
            Assert.Equal(2, _repository.Get(query).Count());

            query = new Query<Job<Guid, string>> { new QueryCondition("AccountId", "User1"), new QueryCondition("TaskId", "Task1") };
            Assert.Equal(3, _repository.Get(query).Count());

            query = new Query<Job<Guid, string>> { new QueryCondition("Status", JobStatus.Error) };
            Assert.Single(_repository.Get(query));

            query = new Query<Job<Guid, string>> { new QueryCondition("Status", JobStatus.Pending) };
            Assert.Empty(_repository.Get(query));

            query = new Query<Job<Guid, string>> { new QueryCondition("AccountId", "User2") };
            Assert.Single(_repository.Get(query));
        }

        [Fact]
        public void GetLastIsOk()
        {
            var job1 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job1);

            var job2 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            _repository.Add(job2);

            var job3 = new Job<Guid, string>(Guid.NewGuid(), "Task2", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            _repository.Add(job3);

            var job4 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            _repository.Add(job4);

            var job5 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User2")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job5);

            var query = new Query<Job<Guid, string>> { new QueryCondition("AccountId", "User1") };
            Assert.Equal(job4.Id, _repository.GetLast(query).Id);

            query = new Query<Job<Guid, string>> { new QueryCondition("AccountId", "User2") };
            Assert.Equal(job5.Id, _repository.GetLast(query).Id);

            query = new Query<Job<Guid, string>>
            {
                new QueryCondition("Requested", QueryOperator.LessThan,
                new DateTime(2015, 01, 04)), new QueryCondition("TaskId", "Task1")
            };
            Assert.Equal(job2.Id, _repository.GetLast(query).Id);

            query = new Query<Job<Guid, string>> { new QueryCondition("TaskId", "NonExistingTask") };
            Assert.Null(_repository.GetLast(query));
        }

        [Fact]
        public void GetOverridesReturnsOrderedByRequested()
        {
            var job1 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1") { Requested = new DateTime(2015, 01, 01) };
            _repository.Add(job1);

            var job2 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1") { Requested = new DateTime(2013, 01, 01) };
            _repository.Add(job2);

            var job3 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1") { Requested = new DateTime(2014, 01, 01) };
            _repository.Add(job3);

            var job4 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User2") { Requested = new DateTime(2012, 01, 01) };
            _repository.Add(job4);

            var list = _repository.Get(j => j.AccountId == "User1").ToList();
            Assert.Equal(list[0].Id, job1.Id);
            Assert.Equal(list[1].Id, job3.Id);
            Assert.Equal(list[2].Id, job2.Id);

            var all = _repository.GetAll().ToList();
            Assert.Equal(all[0].Id, job1.Id);
            Assert.Equal(all[1].Id, job3.Id);
            Assert.Equal(all[2].Id, job2.Id);
            Assert.Equal(all[3].Id, job4.Id);
        }

        [Fact]
        public void RemoveByExpressionIsOk()
        {
            var job1 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job1);

            var job2 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            _repository.Add(job2);

            var job3 = new Job<Guid, string>(Guid.NewGuid(), "Task2", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            _repository.Add(job3);

            var job4 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            _repository.Add(job4);

            var job5 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User2")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job5);

            _repository.Remove(j => j.Requested < new DateTime(2015, 01, 03));
            Assert.False(_repository.Contains(job1.Id));
            Assert.Equal(3, _repository.Count());
        }

        [Fact]
        public void RemoveByQueryIsOk()
        {
            var job1 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job1);

            var job2 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed,
                Tag = "Tag1"
            };
            _repository.Add(job2);

            var job3 = new Job<Guid, string>(Guid.NewGuid(), "Task2", "User1")
            {
                Requested = new DateTime(2015, 01, 03),
                Status = JobStatus.Completed
            };
            _repository.Add(job3);

            var job4 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User1")
            {
                Requested = new DateTime(2015, 01, 04),
                Status = JobStatus.Error,
                Tag = "Tag1"
            };
            _repository.Add(job4);

            var job5 = new Job<Guid, string>(Guid.NewGuid(), "Task1", "User2")
            {
                Requested = new DateTime(2015, 01, 02),
                Status = JobStatus.Completed
            };
            _repository.Add(job5);

            var query = new Query<Job<Guid, string>> { new QueryCondition("Requested", QueryOperator.LessThan, new DateTime(2015, 01, 03)) };
            _repository.Remove(query);
            Assert.False(_repository.Contains(job1.Id));
            Assert.Equal(3, _repository.Count());
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }
    }
}