namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class GroupedHostService : BaseGroupedUpdatableDiscreteService<Host, string>, IGroupedHostService
    {
        private readonly IGroupedHostRepository _repository;

        public GroupedHostService(IGroupedHostRepository repository)
            : base(repository)
        {
            _repository = repository;
        }

        public override void Add(Host host, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(host.FullName, user))
            {
                throw new ArgumentException($"Host with fullname '{host.FullName}' already exists.", nameof(host));
            }

            if (_repository.GetIds(user).Contains(host.Id))
            {
                throw new ArgumentException($"A host with ID '{host.Id}' already exists.", nameof(host));
            }

            var cancelEventArgs = new CancelEventArgs<Host>(host);
            OnAdding(cancelEventArgs);
            if (!cancelEventArgs.Cancel)
            {
                _repository.Add(host, user);
                OnAdded(host);
            }
        }

        public override void Update(Host updatedHost, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(updatedHost.FullName, user))
            {
                throw new KeyNotFoundException($"Host with fullname '{updatedHost.FullName}' was not found.");
            }

            var currentId = _repository.Get(updatedHost.FullName).Value.Id;
            if (updatedHost.Id != currentId && _repository.GetIds(user).Contains(updatedHost.Id))
            {
                throw new ArgumentException($"A host with ID '{updatedHost.Id}' already exists.", nameof(updatedHost));
            }

            var cancelEventArgs = new CancelEventArgs<Host>(updatedHost);
            OnUpdating(cancelEventArgs);
            if (!cancelEventArgs.Cancel)
            {
                _repository.Update(updatedHost, user);
                OnUpdated(updatedHost);
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
                throw new NotImplementedException("This repository cannot create hosts dynamically.");
            }
        }

        public void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null)
        {
            try
            {
                _repository.AdjustJobCapacity(desiredJobCapacity, user);
            }
            catch (NotImplementedException)
            {
                throw new NotImplementedException("This repository cannot ajust job capacity dynamically.");
            }
        }

        public override void AddOrUpdate(Host host, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(host.FullName, user))
            {
                if (_repository.GetIds(user).Contains(host.Id))
                {
                    throw new ArgumentException($"A host with ID '{host.Id}' already exists.", nameof(host));
                }

                var cancelEventArgs = new CancelEventArgs<Host>(host);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Add(host, user);
                    OnAdded(host);
                }
            }
            else
            {
                var currentId = _repository.Get(host.FullName).Value.Id;
                if (host.Id != currentId && _repository.GetIds(user).Contains(host.Id))
                {
                    throw new ArgumentException($"A host with ID '{host.Id}' already exists.", nameof(host));
                }

                var cancelEventArgs = new CancelEventArgs<Host>(host);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Update(host, user);
                    OnUpdated(host);
                }
            }
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IGroupedHostRepository>(path);
        }
    }
}