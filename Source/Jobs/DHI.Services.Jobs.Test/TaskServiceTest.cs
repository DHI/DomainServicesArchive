namespace DHI.Services.Jobs.Test
{
    using AutoFixture;
    using Jobs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class TaskServiceTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TaskService<FakeTask<Guid>, Guid>(null));
        }

        [Theory, AutoTaskData]
        public void GetNonExistingThrows(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            Assert.False(taskService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoTaskData]
        public void GetIsOk(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            var task = taskService.GetAll().ToArray()[0];
            taskService.TryGet(task.Id, out var jb);
            Assert.Equal(task.Id, jb.Id);
        }

        [Theory, AutoTaskData]
        public void GetAllIsOk(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            Assert.Equal(_fixture.RepeatCount, taskService.GetAll().Count());
        }

        [Theory, AutoTaskData]
        public void GetIdsIsOk(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            Assert.Equal(_fixture.RepeatCount, taskService.GetIds().Count());
        }

        [Theory, AutoTaskData]
        public void CountIsOk(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            Assert.Equal(_fixture.RepeatCount, taskService.Count());
        }

        [Theory, AutoTaskData]
        public void ExistsIsOk(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            var featureCollection = taskService.GetAll().ToArray()[0];
            Assert.True(taskService.Exists(featureCollection.Id));
        }

        [Theory, AutoTaskData]
        public void DoesNotExistsIsOk(TaskService<FakeTask<Guid>, Guid> taskService)
        {
            Assert.False(taskService.Exists(Guid.NewGuid()));
        }
    }
}