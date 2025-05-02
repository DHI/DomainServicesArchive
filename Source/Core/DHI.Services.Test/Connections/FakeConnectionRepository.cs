namespace DHI.Services.Test
{
    using System.Collections.Generic;
    using System.Security.Claims;

    internal class FakeConnectionRepository : FakeRepository<BaseConnection, string>, IConnectionRepository
    {
        public FakeConnectionRepository()
        {
        }

        public FakeConnectionRepository(IEnumerable<BaseConnection> connectionList)
            : base(connectionList)
        {
        }

        public new Maybe<IConnection> Get(string id, ClaimsPrincipal user = null)
        {
            var maybe = base.Get(id, user);
            return maybe.HasValue ? ((IConnection)maybe.Value).ToMaybe() : Maybe.Empty<IConnection>();
        }

        public new IEnumerable<IConnection> GetAll(ClaimsPrincipal user = null)
        {
            return base.GetAll(user);
        }

        public void Add(IConnection entity, ClaimsPrincipal user = null)
        {
            base.Add((BaseConnection)entity, user);
        }

        public void Update(IConnection entity, ClaimsPrincipal user = null)
        {
            base.Update((BaseConnection)entity, user);
        }
    }
}