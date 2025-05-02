using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.WebApiCore.Test")]

namespace DHI.Services.WebApiCore
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly FilterService _filterService;
        private readonly ILogger _logger;

        public NotificationHub(IFilterRepository filterRepository, ILogger logger)
        {
            _logger = logger;
            _filterService = new FilterService(filterRepository, _logger);
            _filterService.TransportConnectionDeleted += FilterService_TransportConnectionDeleted;
        }

        public async Task AddJobFilter(string dataConnectionId, QueryDTO queryDTO)
        {
            try
            {
                Guard.Against.NullOrEmpty(dataConnectionId, nameof(dataConnectionId));
                await AddFilter("Job", queryDTO, dataConnectionId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed trying to add job filter in {Class}.{Method}", nameof(NotificationHub), nameof(AddJobFilter));
                throw;
            }
        }

        public async Task AddJsonDocumentFilter(string dataConnectionId, QueryDTO queryDTO)
        {
            try
            {
                Guard.Against.NullOrEmpty(dataConnectionId, nameof(dataConnectionId));
                await AddFilter("JsonDocument", queryDTO, dataConnectionId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed trying to add json document filter in {Class}.{Method}", nameof(NotificationHub), nameof(AddJsonDocumentFilter));
                throw;
            }
        }

        public async Task AddTimeSeriesFilter(string dataConnectionId, QueryDTO queryDTO)
        {
            try
            {
                Guard.Against.NullOrEmpty(dataConnectionId, nameof(dataConnectionId));
                await AddFilter("TimeSeries", queryDTO, dataConnectionId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed trying to add time series filter in {Class}.{Method}", nameof(NotificationHub), nameof(AddTimeSeriesFilter));
                throw;
            }
        }

        public async Task AddUserGroupFilter(QueryDTO queryDTO)
        {
            try
            {
                await AddFilter("UserGroup", queryDTO);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed trying to add user group filter in {Class}.{Method}", nameof(NotificationHub), nameof(AddUserGroupFilter));
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var filterIds = (await _filterService.GetIdsAsync(Context.ConnectionId, Context.User)).ToArray();
            foreach (var filterId in filterIds)
            {
                await _filterService.TryDeleteTransportConnectionAsync(Context.ConnectionId, filterId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        internal async Task AddFilter(string dataType, QueryDTO queryDTO, string dataConnectionId = null)
        {
            Guard.Against.NullOrEmpty(dataType, nameof(dataType));
            var transportConnectionId = Context.ConnectionId;
            var user = Context.User;
            var filter = queryDTO is null ? new Filter(dataType, dataConnectionId) : new Filter(dataType, dataConnectionId, queryDTO.ToQueryConditions());
            _filterService.TryAdd(filter, user);
            await _filterService.TryAddTransportConnectionAsync(transportConnectionId, filter.Id, user);
            var groupName = filter.Id;
            await Groups.AddToGroupAsync(transportConnectionId, groupName);
        }

        private void FilterService_TransportConnectionDeleted(object sender, TransportConnectionEventArgs e)
        {
            var (_, filterId, count) = e.Item;
            if (count == 0)
            {
                _filterService.Remove(filterId);
            }
        }
    }
}