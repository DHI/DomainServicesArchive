namespace DHI.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Class ServiceLocator.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly ConcurrentDictionary<string, object> _services = new();

        /// <summary>
        ///     Gets the number of services.
        /// </summary>
        /// <value>The count.</value>
        public static int Count => _services.Count;

        /// <summary>
        ///     Gets all service IDs.
        /// </summary>
        /// <returns>IEnumerable&lt;string&gt;.</returns>
        public static IEnumerable<string> Ids => _services.Keys;

        /// <summary>
        ///     Determines whether the service locator contains a service with the specified service identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>
        ///     <c>true</c> if the service locator contains a service with the specified service identifier; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string serviceId)
        {
            return _services.ContainsKey(serviceId);
        }

        /// <summary>
        ///     Gets the service with the specified identifier.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>TService.</returns>
        /// <exception cref="ArgumentNullException">serviceId</exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static TService Get<TService>(string serviceId)
        {
            Guard.Against.NullOrEmpty(serviceId, nameof(serviceId));
            if (!_services.ContainsKey(serviceId))
            {
                throw new KeyNotFoundException(
                    $"A service with ID '{serviceId}' is not registered in the service locator.");
            }

            return (TService)_services[serviceId];
        }

        /// <summary>
        ///     Gets all services of the given type.
        /// </summary>
        /// <typeparam name="TService">The type of the t service.</typeparam>
        /// <returns>IEnumerable&lt;TService&gt;.</returns>
        public static IEnumerable<TService> GetAll<TService>()
        {
            return _services.Values.OfType<TService>().ToArray();
        }

        /// <summary>
        ///     Gets all service types.
        /// </summary>
        /// <returns>IDictionary&lt;string, string&gt;.</returns>
        public static IDictionary<string, Type> GetTypes()
        {
            return _services.ToDictionary(s => s.Key, s => s.Value.GetType());
        }

        /// <summary>
        ///     Registers the specified service with the given identifier.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="serviceId">The service identifier.</param>
        public static void Register(object service, string serviceId)
        {
            Guard.Against.Null(service, nameof(service));
            Guard.Against.NullOrEmpty(serviceId, nameof(serviceId));
            _services.TryAdd(serviceId, service);
        }

        /// <summary>
        ///     Removes the service with the specified service identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <exception cref="ArgumentNullException">serviceId</exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static void Remove(string serviceId)
        {
            Guard.Against.NullOrEmpty(serviceId, nameof(serviceId));
            _services.TryRemove(serviceId, out var service);

            if (service == null)
            {
                throw new KeyNotFoundException(
                    $"A service with ID '{serviceId}' is not registered in the service locator.");
            }
        }
    }
}