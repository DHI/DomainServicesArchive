namespace DHI.Services.Jobs.Test
{
    using System.Collections.Generic;
    using System.Security.Claims;

    internal class FakeTaskRepository<TId> : FakeRepository<FakeTask<TId>, TId>, ITaskRepository<FakeTask<TId>, TId>
    {
        public FakeTaskRepository(IEnumerable<FakeTask<TId>> taskList)
            : base(taskList)
        {
        }

        public new Maybe<ITask<TId>> Get(TId id, ClaimsPrincipal user = null)
        {
            var maybe = base.Get(id, user);
            return maybe.HasValue ? ((ITask<TId>)maybe.Value).ToMaybe() : Maybe.Empty<ITask<TId>>();
        }

        public new IEnumerable<ITask<TId>> GetAll(ClaimsPrincipal user = null)
        {
            return base.GetAll(user);
        }
    }
}