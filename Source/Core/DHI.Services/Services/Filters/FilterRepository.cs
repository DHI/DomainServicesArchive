namespace DHI.Services.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    ///     JSON Repository for storing filters.
    /// </summary>
    public class FilterRepository : JsonRepository<Filter, string>, IFilterRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public FilterRepository(string filePath) : base(filePath)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableJsonRepository{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public FilterRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public FilterRepository(string filePath,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Filter>> GetListAsync(string dataType, string dataConnectionId = null, ClaimsPrincipal user = null)
        {
            if (dataConnectionId is null)
            {
                return await Task.Run(() => GetAll().Where(f => f.DataType == dataType));
            }

            return await Task.Run(() => GetAll().Where(f => f.DataType == dataType && f.DataConnectionId == dataConnectionId));
        }

        /// <inheritdoc />
        public async Task<int> TransportConnectionsCountAsync(string filterId, ClaimsPrincipal user = null)
        {
            return await Task.Run(() => Get(filterId).Value.TransportConnections.Count);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetIdsAsync(string transportConnectionId, ClaimsPrincipal user = null)
        {
            return await Task.Run(() => GetAll().Where(f => f.TransportConnections.Contains(transportConnectionId)).Select(f => f.Id));
        }

        /// <inheritdoc />
        public async Task AddTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null)
        {
            var filter = await Task.Run(() => Get(filterId).Value);
            filter.TransportConnections.Add(transportConnectionId);
            Update(filter);
        }

        /// <inheritdoc />
        public async Task DeleteTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null)
        {
            var filter = await Task.Run(() => Get(filterId).Value);
            filter.TransportConnections.Remove(transportConnectionId);
            Update(filter);
        }
    }
}