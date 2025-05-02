namespace DHI.Services.Jobs
{
    using System.Security.Claims;

    public interface IHostService : IService<Host, string>, IDiscreteService<Host, string>, IUpdatableService<Host, string>
    {
        void CreateHost(ClaimsPrincipal user = null);

        void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null);
    }
}