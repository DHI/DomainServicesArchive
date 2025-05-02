using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Class implements Host collection for SignalR clients.  
    /// </summary>
    /// <remarks>
    /// IHostCollection implementation is readonly. ISignalRHostCollection is writeable.
    /// </remarks>
    public class SignalRHostRepository : ISignalRHostCollection
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<Host>> _hosts;
        private readonly object _memberOperationLock = new object();
        private readonly ILogger _logger;

        public SignalRHostRepository()
        {
            _hosts = new();
            _logger = null;
        }

        public SignalRHostRepository(ILogger logger)
        {
            _hosts = new();
            _logger = logger;
        }


        public SignalRHostRepository(ConcurrentDictionary<string, ConcurrentBag<Host>> hosts)
        {
            _hosts = hosts;
            _logger = null;
        }

        public SignalRHostRepository(ConcurrentDictionary<string, ConcurrentBag<Host>> hosts, ILogger logger)
        {
            _hosts = hosts;
            _logger = logger;
        }

        /// <inheritdoc />
        public void AddMember(string connectionId, Dictionary<string, string> claims)
        {
            Host host = null;
            var group = claims.TryGetValue("HostGroup", out var claimsGroup) ? claimsGroup : "none";

            host = new Host(connectionId, connectionId, group)
            {
                Priority = claims.TryGetValue("Priority", out var priorityString) && int.TryParse(priorityString, out var priority) ? priority : 1,
                RunningJobsLimit = claims.TryGetValue("RunningJobsLimit", out var runningJobsLimitString) && int.TryParse(runningJobsLimitString, out var runningJobsLimit) ? runningJobsLimit : 1,
            };

            if (_hosts.TryGetValue(group, out var members))
            {
                /*
                 * There can exist a state where the host disconnects and reconnects before the disconnect event is handled. This leaves the host connected to the signalr hub, but not in the host collection.
                 * We allow a duplicate to be added such that the subsequent disconnect event can remove the duplicate.
                 */
                if (members.Count(m => m.Id == connectionId) > 1)
                {
                    _logger?.Log(LogLevel.Warning, "AddMember: {connectionId} already has 2 members in {group}. Will be NOT be added again.", connectionId, group);
                    return;
                }

                if (members.Any(m => m.Id == connectionId))
                {
                    _logger?.LogWarning("AddMember: {connectionId} already a member of {group}. Will be added as duplicate.", connectionId, group);
                }

                members.Add(host);
                _logger?.LogInformation("AddMember: Added {connectionId} to {group}", connectionId, group);
            }
            else
            {
                _logger?.LogInformation("AddMember: Creating new group {group} with member {connectionId}", connectionId, group);
                lock (_memberOperationLock)
                {
                    members = new ConcurrentBag<Host>
                    {
                        host
                    };

                    if (_hosts.TryAdd(group, members))
                    {
                        _logger?.LogInformation("AddMember: Added new group {group}", group);
                    }
                    else
                    {
                        _logger?.LogWarning("AddMember: New group {group} not added", group);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void RemoveMember(string connectionId, string group)
        {
            if (_hosts.TryRemove(group, out var members))
            {
                _logger?.LogInformation("RemoveMember: Removed group {group}", group);

                var removedMembers = members.Where(m => m.Id == connectionId);
                var newMembers = members.Where(m => m.Id != connectionId).ToList();

                /*
                 * There can exist a state where the host disconnects and reconnects before the disconnect event is handled. This leaves the host connected to the signalr hub, but not in the host collection.
                 * We allow the duplicate to remain after this operation to represent the reconnected host.
                 */

                if (removedMembers.Count() > 1)
                {
                    newMembers.Add(removedMembers.First());

                    _logger?.LogInformation("RemoveMember: group {group} had {count} duplicates of {connectionId}, 1 duplicate will be readded to the group.", group, removedMembers.Count() - 1, connectionId);
                }

                if (_hosts.TryAdd(group, new ConcurrentBag<Host>(newMembers)))
                {
                    _logger?.LogInformation("RemoveMember: Readded group {group} with members {newMembers}", group, newMembers.Select(h => h.Name).DefaultIfEmpty().Aggregate((p, n) => $"{p}, {n}"));
                }
                else
                {
                    _logger?.LogWarning("RemoveMember: Could NOT readd group {group} with members {newMembers}", group, newMembers.Select(h => h.Name).DefaultIfEmpty().Aggregate((p, n) => $"{p}, {n}"));
                }
            }
            else
            {
                _logger?.LogWarning("RemoveMember: Group {group} not found", group);
            }
        }

        /// <inheritdoc />
        public IEnumerable<Host> GetGroupMembers(string group = "none")
        {
            if (_hosts.TryGetValue(group, out var value))
            {
                return value.GroupBy(v => v.Id).Select(g => g.First()).ToArray();
            }

            return Array.Empty<Host>();
        }

        /// <inheritdoc />
        public void Add(Host entity, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <inheritdoc />
        public void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <inheritdoc />
        public bool Contains(string id, ClaimsPrincipal user = null)
        {
            return _hosts.Any(kvp => kvp.Value.Any(v => v.Id == id));
        }

        /// <inheritdoc />
        public int Count(ClaimsPrincipal user = null)
        {
            return _hosts.Sum(s => s.Value.GroupBy(v => v.Id).Select(g => g.First()).Count());
        }

        /// <summary>
        /// Method not implemented in this readonly IHostRepository 
        /// </summary>
        /// <param name="_"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateHost(ClaimsPrincipal _ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <inheritdoc />
        public Maybe<Host> Get(string id, ClaimsPrincipal _ = null)
        {
            var group = _hosts.FirstOrDefault(kvp => kvp.Value.Any(v => v.Id == id));

            if (group.Equals(default(KeyValuePair<string, ConcurrentBag<Host>>)))
            {
                return Maybe.Empty<Host>();
            }

            return group.Value.First(h => h.Id == id).ToMaybe();
        }

        /// <inheritdoc />
        public IEnumerable<Host> GetAll(ClaimsPrincipal user = null)
        {
            return _hosts.SelectMany(kvp => kvp.Value.GroupBy(v => v.Id).Select(g => g.First()));
        }

        /// <inheritdoc />
        public IEnumerable<string> GetIds(ClaimsPrincipal user = null)
        {
            return _hosts.SelectMany(kvp => kvp.Value.GroupBy(v => v.Id).Select(g => g.First()).Select(v => v.Id));
        }

        /// <summary>
        /// Method not implemented in this readonly IHostRepository 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Remove(string _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <summary>
        /// Method not implemented in this readonly IHostRepository 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Update(Host _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }
    }
}