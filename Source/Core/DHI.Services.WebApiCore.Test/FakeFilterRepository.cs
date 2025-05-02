namespace DHI.Services.WebApiCore.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Filters;

    public class FakeFilterRepository : FakeRepository<Filter, string>, IFilterRepository
    {
        public async Task<IEnumerable<Filter>> GetListAsync(string dataType, string dataConnectionId = null, ClaimsPrincipal user = null)
        {
            if (dataConnectionId is null)
            {
                return await Task.Run(() => GetAll().Where(f => f.DataType == dataType));
            }

            return await Task.Run(() => GetAll().Where(f => f.DataType == dataType && f.DataConnectionId == dataConnectionId));
        }

        public async Task<int> TransportConnectionsCountAsync(string filterId, ClaimsPrincipal user = null)
        {
            return await Task.Run(() => Get(filterId).Value.TransportConnections.Count);
        }

        public async Task<IEnumerable<string>> GetIdsAsync(string transportConnectionId, ClaimsPrincipal user = null)
        {
            return await Task.Run(() => GetAll().Where(f => f.TransportConnections.Contains(transportConnectionId)).Select(f => f.Id));
        }

        public async Task AddTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null)
        {
            var filter = await Task.Run(() => Get(filterId).Value);
            filter.TransportConnections.Add(transportConnectionId);
            Update(filter);
        }

        public async Task DeleteTransportConnectionAsync(string transportConnectionId, string filterId, ClaimsPrincipal user = null)
        {
            var filter = await Task.Run(() => Get(filterId).Value);
            filter.TransportConnections.Remove(transportConnectionId);
            Update(filter);
        }
    }
}