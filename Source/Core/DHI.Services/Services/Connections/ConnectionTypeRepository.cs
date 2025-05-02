namespace DHI.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     Connection type repository.
    /// </summary>
    public class ConnectionTypeRepository : BaseDiscreteRepository<ConnectionType, string>, IUpdatableRepository<ConnectionType, string>
    {
        private readonly Dictionary<string, ConnectionType> _connectionTypes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionTypeRepository" /> class.
        /// </summary>
        public ConnectionTypeRepository()
        {
            _connectionTypes = new Dictionary<string, ConnectionType>();
        }

        /// <summary>
        ///     Adds the specified connection type.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="user">The user.</param>
        public void Add(ConnectionType connectionType, ClaimsPrincipal user = null)
        {
            _connectionTypes[connectionType.Id] = connectionType;
        }

        /// <summary>
        ///     Removes the connection type with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        public void Remove(string id, ClaimsPrincipal user = null)
        {
            _connectionTypes.Remove(id);
        }

        /// <summary>
        ///     Updates the specified connection type.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="user">The user.</param>
        public void Update(ConnectionType connectionType, ClaimsPrincipal user = null)
        {
            _connectionTypes[connectionType.Id] = connectionType;
        }

        /// <summary>
        ///     Gets all connection types.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public override IEnumerable<ConnectionType> GetAll(ClaimsPrincipal user = null)
        {
            return _connectionTypes.Values ?? Enumerable.Empty<ConnectionType>();
        }
    }
}