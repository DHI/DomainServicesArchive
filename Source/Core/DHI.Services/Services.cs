namespace DHI.Services
{
    using System;

    /// <summary>
    ///     Services Factory.
    /// </summary>
    public static class Services
    {
        /// <summary>
        ///     Gets the connections.
        /// </summary>
        /// <value>The connections.</value>
        public static ConnectionService Connections { get; private set; }

        /// <summary>
        ///     Configures the services policy.
        /// </summary>
        /// <param name="connectionRepository">The connection repository.</param>
        /// <param name="lazyCreation">if set to <c>true</c> lazy creation.</param>
        public static void Configure(IConnectionRepository connectionRepository, bool lazyCreation = true)
        {
            Connections = new ConnectionService(connectionRepository);
            Connections.Added += Connections_Added;
            Connections.Deleted += Connections_Deleted;
            Connections.Updated += Connections_Updated;

            if (lazyCreation)
            {
                return;
            }

            foreach (var connection in Connections.GetAll())
            {
                var service = connection.Create();
                ServiceLocator.Register(service, connection.Id);
            }
        }

        /// <summary>
        ///     Gets the service instance with the specified connection identifier.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="connectionId">The connection identifier.</param>
        /// <returns>TService.</returns>
        public static TService Get<TService>(string connectionId)
        {
            try
            {
                return ServiceLocator.Get<TService>(connectionId);
            }
            catch (Exception)
            {
                if (Connections is null)
                {
                    throw;
                }

                if (!Connections.TryGet(connectionId, out var connection))
                {
                    throw;
                }

                var service = connection.Create();
                ServiceLocator.Register(service, connection.Id);
                return (TService)service;
            }
        }

        private static void Connections_Added(object sender, EventArgs<IConnection> e)
        {
            var connection = e.Item;
            var service = connection.Create();
            ServiceLocator.Register(service, connection.Id);
        }

        private static void Connections_Deleted(object sender, EventArgs<string> e)
        {
            var connectionId = e.Item;
            if (ServiceLocator.Contains(connectionId))
            {
                ServiceLocator.Remove(connectionId);
            }
        }

        private static void Connections_Updated(object sender, EventArgs<IConnection> e)
        {
            var connection = e.Item;
            if (ServiceLocator.Contains(connection.Id))
            {
                ServiceLocator.Remove(connection.Id);
            }

            var service = connection.Create();
            ServiceLocator.Register(service, connection.Id);
        }
    }
}