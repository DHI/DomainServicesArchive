namespace DHI.Services.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Service for managing filters for real-time (SignalR) messages.
    /// </summary>
    public class FilterService : BaseImmutableDiscreteService<Filter, string>
    {
        private readonly ILogger _logger;
        private readonly IFilterRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="logger">The logger.</param>
        public FilterService(IFilterRepository repository, ILogger logger = null) : base(repository)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        ///     Occurs when a transport connection was added.
        /// </summary>
        public event EventHandler<TransportConnectionEventArgs> TransportConnectionAdded;

        /// <summary>
        ///     Occurs when adding a transport connection.
        /// </summary>
        public event EventHandler<TransportConnectionCancelEventArgs> AddingTransportConnection;

        /// <summary>
        ///     Occurs when a transport connection was deleted.
        /// </summary>
        public event EventHandler<TransportConnectionEventArgs> TransportConnectionDeleted;

        /// <summary>
        ///     Occurs when deleting a transport connection.
        /// </summary>
        public event EventHandler<TransportConnectionCancelEventArgs> DeletingTransportConnection;

        /// <summary>
        ///     Gets a collection of filters for the specified type of data.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="dataConnectionId">Id of the data connection.</param>
        /// <param name="user">The user.</param>
        public async Task<IEnumerable<Filter>> GetListAsync(string dataType, string dataConnectionId = null, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrEmpty(dataType, nameof(dataType));
            return await _repository.GetListAsync(dataType, dataConnectionId, user);
        }

        /// <summary>
        ///     Gets all filter identifiers for the specified transport connection.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection identifier.</param>
        /// <param name="user">The user.</param>
        public async Task<IEnumerable<string>> GetIdsAsync(string transportConnectionId, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrEmpty(transportConnectionId, nameof(transportConnectionId));
            return await _repository.GetIdsAsync(transportConnectionId, user);
        }

        /// <summary>
        ///     Try adding a transport connection to the specified filter.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection identifier.</param>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if transport connection was successfully added to the specified filter, <c>false</c> otherwise.</returns>
        public async Task<bool> TryAddTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null)
        {
            try
            {
                var cancelEventArgs = new TransportConnectionCancelEventArgs((transportConnectionId, filterId));
                OnAddingTransportConnection(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                await _repository.AddTransportConnectionAsync(transportConnectionId, filterId, user);
                var count = await _repository.TransportConnectionsCountAsync(filterId, user);
                OnTransportConnectionAdded(transportConnectionId, filterId, count);
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Failed trying to add transport connection to filter.");
                return false;
            }
        }

        /// <summary>
        ///     Try deleting a transport connection from the specified filter.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection identifier.</param>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if transport connection was successfully deleted from the specified filter, <c>false</c> otherwise.</returns>
        public async Task<bool> TryDeleteTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null)
        {
            try
            {
                var cancelEventArgs = new TransportConnectionCancelEventArgs((transportConnectionId, filterId));
                OnDeletingTransportConnection(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                await _repository.DeleteTransportConnectionAsync(transportConnectionId, filterId, user);
                var count = await _repository.TransportConnectionsCountAsync(filterId, user);
                OnConnectionDeleted(transportConnectionId, filterId, count);
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Failed trying to delete transport connection from filter.");
                return false;
            }
        }

        /// <summary>
        ///     Called when a transport connection was added.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection ID.</param>
        /// <param name="filterId">The filter ID</param>
        /// <param name="count">The total number of transport connections for given filter</param>
        protected virtual void OnTransportConnectionAdded(string transportConnectionId, string filterId, int count)
        {
            TransportConnectionAdded?.Invoke(this, new TransportConnectionEventArgs((transportConnectionId, filterId, count)));
        }

        /// <summary>
        ///     Called when adding a transport connection.
        /// </summary>
        /// <param name="e">The event argument.</param>
        protected virtual void OnAddingTransportConnection(TransportConnectionCancelEventArgs e)
        {
            AddingTransportConnection?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a transport connection was deleted.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection ID.</param>
        /// <param name="filterId">The filter ID</param>
        /// <param name="count">The total number of transport connections for the given filter</param>
        protected virtual void OnConnectionDeleted(string transportConnectionId, string filterId, int count)
        {
            TransportConnectionDeleted?.Invoke(this, new TransportConnectionEventArgs((transportConnectionId, filterId, count)));
        }

        /// <summary>
        ///     Called when deleting an transport connection.
        /// </summary>
        /// <param name="e">The event argument.</param>
        protected virtual void OnDeletingTransportConnection(TransportConnectionCancelEventArgs e)
        {
            DeletingTransportConnection?.Invoke(this, e);
        }
    }
}