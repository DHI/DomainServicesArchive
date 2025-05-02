using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Implementation of IGroupedHostService accepting SignalR clients as hosts.
    /// </summary>
    /// <remarks>
    /// This implementation is readonly, as hosts are managed in the IHostCollection through connection to SignalR
    /// </remarks>
    public class SignalRHostService : IGroupedHostService
    {
        public event EventHandler<EventArgs<Host>> Added;
        public event EventHandler<CancelEventArgs<Host>> Adding;
        public event EventHandler<EventArgs<string>> Deleted;
        public event EventHandler<CancelEventArgs<string>> Deleting;
        public event EventHandler<EventArgs<Host>> Updated;
        public event EventHandler<CancelEventArgs<Host>> Updating;

        private readonly ISignalRHostCollection _hostCollection;
        private readonly string[] _possibleHostGroups;

        public SignalRHostService(ISignalRHostCollection hostCollection, string[] possibleHostGroups)
        {
            _hostCollection = hostCollection;
            _possibleHostGroups = possibleHostGroups;
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Add(Host _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddOrUpdate(Host _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AdjustJobCapacity(int _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Job capacity may not be modified in this implementation");
        }

        /// <inheritdoc />
        public int Count(ClaimsPrincipal _ = null)
        {
            return _hostCollection.Count();
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService
        /// </summary>
        /// <param name="_"></param>        
        /// <exception cref="NotImplementedException"></exception>
        public void CreateHost(ClaimsPrincipal _ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <inheritdoc />
        public bool Exists(string id, ClaimsPrincipal _ = null)
        {
            return _hostCollection.Contains(id);
        }

        /// <inheritdoc />
        public Host Get(string id, ClaimsPrincipal _ = null)
        {
            var host = _hostCollection.Get(id);
            return host.HasValue ? host.Value : null;
        }

        public bool TryGet(string id, out Host entity, ClaimsPrincipal user = null)
        {
            try
            {
                var host = _hostCollection.Get(id);
                if (host.HasValue)
                {
                    entity = host.Value;
                    return true;
                }

                entity = default;
                return false;
            }
            catch
            {
                entity = default;
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerable<Host> Get(IEnumerable<string> ids, ClaimsPrincipal _ = null)
        {
            return _hostCollection.GetAll().Where(m => ids.Contains(m.Id));
        }

        public bool TryGet(IEnumerable<string> ids, out IEnumerable<Host> entities, ClaimsPrincipal user = null)
        {
            try
            {
                var host = _hostCollection.GetAll().Where(m => ids.Contains(m.Id)).ToArray();
                if (ids.All(i => host.Select(h => h.Id).Contains(i)))
                {
                    entities = host;
                    return true;
                }

                entities = host;
                return false;
            }
            catch
            {
                entities = Array.Empty<Host>();
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerable<Host> GetAll(ClaimsPrincipal _ = null)
        {
            return _hostCollection.GetAll();
        }

        /// <inheritdoc />
        public IEnumerable<Host> GetByGroup(string group, ClaimsPrincipal _ = null)
        {
            return _hostCollection.GetGroupMembers(group);
        }

        /// <inheritdoc />
        public IEnumerable<Host> GetByGroups(IEnumerable<string> groups, ClaimsPrincipal _ = null)
        {
            return groups.SelectMany(g => _hostCollection.GetGroupMembers(g));
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFullNames(string group, ClaimsPrincipal _ = null)
        {
            return _hostCollection.GetGroupMembers(group).Select(h => h.FullName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFullNames(ClaimsPrincipal _ = null)
        {
            return _hostCollection.GetAll().Select(h => h.FullName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetIds(ClaimsPrincipal _ = null)
        {
            return _hostCollection.GetAll().Select(h => h.Id);
        }

        /// <inheritdoc />
        public bool GroupExists(string group, ClaimsPrincipal _ = null)
        {
            return _possibleHostGroups.Contains(group);
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Remove(string _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryAdd(Host _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryUpdate(Host _, ClaimsPrincipal __ = null)
        {
            throw new NotImplementedException("Host collection may not be modified in this implementation");
        }

        /// <summary>
        /// Method not implemented in this readonly IGroupedHostService 
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