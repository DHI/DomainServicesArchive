namespace DHI.Services.Scalars.Test
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public class FakeScalarRepository : FakeGroupedRepository<Scalar<Guid, int>, Guid>, IGroupedScalarRepository<Guid, int>
    {
        public FakeScalarRepository()
        {
        }

        public FakeScalarRepository(IEnumerable<Scalar<Guid, int>> scalars)
            : base(scalars)
        {
        }

        public void SetData(Guid id, ScalarData<int> data, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.SetData(data);
            Update(scalar);
        }

        public void SetLocked(Guid id, bool locked, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.Locked = locked;
            Update(scalar);
        }
    }
}