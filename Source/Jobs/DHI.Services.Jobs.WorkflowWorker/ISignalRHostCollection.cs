using System.Collections.Generic;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Interface for a grouped collection of SignalR clients.
    /// </summary>
    public interface ISignalRHostCollection : IHostRepository
    {
        /// <summary>
        /// Add a host built from the claims collection.
        /// </summary>
        /// <param name="connectionId">The SignalR connection id.</param>
        /// <param name="claims">The collection of claims.</param>
        void AddMember(string connectionId, Dictionary<string, string> claims);

        /// <summary>
        /// Remove the SignalR client Host representation from the hosts collection.
        /// </summary>
        /// <param name="connectionId">The SignalR connection id.</param>
        /// <param name="group">The host group.</param>
        void RemoveMember(string connectionId, string group);

        /// <summary>
        /// Get Hosts in a named group.
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <returns></returns>
        IEnumerable<Host> GetGroupMembers(string groupId);
    }
}
