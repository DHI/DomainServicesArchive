using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DHI.Services.Jobs.WorkflowWorker
{
    [Authorize()]
    public class WorkerHub : Hub<IWorkerClient>
    {
        private readonly ILogger _logger;
        private readonly ISignalRHostCollection _signalRHostsCache;
        private readonly AvailableCache _availableCache;
        private readonly ReportCache _reportCache;
        public WorkerHub(ILogger logger, AvailableCache availableCache, ISignalRHostCollection signalrHostsCache, ReportCache reportCache)
        {
            _logger = logger;
            _availableCache = availableCache;
            _signalRHostsCache = signalrHostsCache;
            _reportCache = reportCache;
        }

        public async Task AvailableResponse(bool available)
        {
            _logger.LogInformation("Availablility Updated {Id}: {result}", Context.UserIdentifier, available);

            _availableCache.TryAdd($"{Context.UserIdentifier}", (available, DateTime.UtcNow));

            await Task.CompletedTask;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {Id}", Context.UserIdentifier);
            var user = Context.User;
            var claims = user.Claims.ToDictionary(k => k.Type, v => v.Value);

            _signalRHostsCache.AddMember(Context.UserIdentifier, claims);

            var group = claims["HostGroup"];
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            _logger.LogWarning(exception, "Client disconnected: {Id}", Context.UserIdentifier);

            var user = Context.User;
            
            var claims = user.Claims.ToDictionary(k => k.Type, v => v.Value);
            var group = claims["HostGroup"];
            await Groups.RemoveFromGroupAsync(Context.UserIdentifier, group);

            _signalRHostsCache.RemoveMember(Context.UserIdentifier, group);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task ReportResponse(Dictionary<string, object> report)
        {
            _logger.LogInformation("Report received for {Id}", Context.UserIdentifier);

            _reportCache.AddOrUpdate(Context.UserIdentifier, (report, DateTime.UtcNow), (s, o) => (report, DateTime.UtcNow));

            await Task.CompletedTask;
        }
    }
}
