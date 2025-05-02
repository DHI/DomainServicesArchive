namespace DHI.Services.Filters
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IFilterRepository :
        IRepository<Filter, string>,
        IDiscreteRepository<Filter, string>,
        IImmutableRepository<Filter, string>
    {
        /// <summary>
        ///     Gets a collection of filters for the specified type of data.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="dataConnectionId">Id of the data connection.</param>
        /// <param name="user">The user.</param>
        Task<IEnumerable<Filter>> GetListAsync(string dataType, string dataConnectionId = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Get the number of real-time transport connections to the specified filter.
        /// </summary>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="user">The user</param>
        Task<int> TransportConnectionsCountAsync(string filterId, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets all filter identifiers for the specified transport connection.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection identifier.</param>
        /// <param name="user">The user.</param>
        Task<IEnumerable<string>> GetIdsAsync(string transportConnectionId, ClaimsPrincipal user = null);

        /// <summary>
        ///     Adds a transport connection to the specified filter.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection identifier.</param>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="user">The user.</param>
        Task AddTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null);

        /// <summary>
        ///     Deletes a transport connection from the specified filter.
        /// </summary>
        /// <param name="transportConnectionId">The transport connection identifier.</param>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="user">The user.</param>
        Task DeleteTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null);
    }
}