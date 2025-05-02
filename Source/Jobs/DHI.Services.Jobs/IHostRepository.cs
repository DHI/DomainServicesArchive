namespace DHI.Services.Jobs
{
    using System.Security.Claims;

    public interface IHostRepository : IRepository<Host, string>, IDiscreteRepository<Host, string>, IUpdatableRepository<Host, string>
    {
        void CreateHost(ClaimsPrincipal user = null);

        void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null);
    }
}