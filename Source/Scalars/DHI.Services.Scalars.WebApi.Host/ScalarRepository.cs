namespace DHI.Services.Scalars.WebApi.Host
{
    using System.Security.Claims;

    public class ScalarRepository : FakeGroupedRepository<Scalar<string, int>, string>, IGroupedScalarRepository<string, int>
    {
        public ScalarRepository(string connectionString)
        {
        }

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