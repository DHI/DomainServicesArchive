namespace DHI.Services.Jobs
{
    public interface ITaskRepository<TTask, TId> : IRepository<TTask, TId>, IDiscreteRepository<TTask, TId>, IUpdatableRepository<TTask, TId> where TTask : ITask<TId>
    {
    }
}