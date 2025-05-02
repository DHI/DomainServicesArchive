namespace DHI.Services.Jobs
{
    public interface ITaskService<TTask, TId> : IService<TTask, TId>, IDiscreteService<TTask, TId>, IImmutableService<TTask, TId> where TTask : ITask<TId>
    {
    }
}