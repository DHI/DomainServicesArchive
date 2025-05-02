namespace DHI.Services.Jobs
{
    using System;

    public class TaskService<TTask, TId> : BaseUpdatableDiscreteService<TTask, TId>, ITaskService<TTask, TId> where TTask : ITask<TId>
    {
        public TaskService(ITaskRepository<TTask, TId> repository)
            : base(repository)
        {
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<ITaskRepository<TTask, TId>>(path);
        }
    }
}