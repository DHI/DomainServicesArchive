namespace DHI.Services.Jobs.Orchestrator.Test
{
    using System.Security.Claims;
    using Scalars;

    public class FakeScalarRepository : FakeGroupedRepository<Scalar<string, int>, string>, IGroupedScalarRepository<string, int>
    {
        public void SetData(string id, ScalarData<int> data, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.SetData(data);
            Update(scalar);
        }

        public void SetLocked(string id, bool locked, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.Locked = locked;
            Update(scalar);
        }
    }
}