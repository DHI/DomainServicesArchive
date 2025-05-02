namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoTaskDataAttribute : AutoDataAttribute
    {
        public AutoTaskDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                var taskList = fixture.CreateMany<FakeTask<Guid>>().ToList();
                fixture.Register<ITaskRepository<FakeTask<Guid>, Guid>>(() => new FakeTaskRepository<Guid>(taskList));

                var stringTaskList = fixture.CreateMany<FakeTask<string>>().ToList();
                fixture.Register<ITaskRepository<FakeTask<string>, string>>(() => new FakeTaskRepository<string>(stringTaskList));
                return fixture;
            })
        {
        }
    }
}