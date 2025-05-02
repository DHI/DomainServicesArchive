namespace DHI.Services.Jobs
{
    using System.Security.Claims;

    public abstract class BaseTaskRepository<TTask, TId> : BaseDiscreteRepository<TTask, TId>, ITaskRepository<TTask, TId> where TTask : ITask<TId>
    {
        public abstract void Add(TTask entity, ClaimsPrincipal user = null);

        public abstract void Remove(TId id, ClaimsPrincipal user = null);

        public abstract void Update(TTask entity, ClaimsPrincipal user = null);
    }
}