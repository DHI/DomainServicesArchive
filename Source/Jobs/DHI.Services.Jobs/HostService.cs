namespace DHI.Services.Jobs
{
    using System;
    using System.Security.Claims;

    public class HostService : BaseUpdatableDiscreteService<Host, string>, IHostService
    {
        private readonly IHostRepository _repository;

        public HostService(IHostRepository repository)
            : base(repository)
        {
            _repository = repository;
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IHostRepository>(path);
        }

        public void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null)
        {
            try
            {
                _repository.AdjustJobCapacity(desiredJobCapacity, user);
            }
            catch (NotImplementedException)
            {
                throw new NotSupportedException("This repository cannot adjust job capacity dynamically.");
            }
        }

        public void CreateHost(ClaimsPrincipal user = null)
        {
            try
            {
                _repository.CreateHost(user);
            }
            catch (NotImplementedException)
            {
                throw new NotSupportedException("This repository cannot create hosts dynamically.");
            }
        }
    }
}