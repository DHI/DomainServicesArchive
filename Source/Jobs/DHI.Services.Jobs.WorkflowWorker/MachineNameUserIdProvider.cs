using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Assigns SignalR clients a user id based on connected machine name.
    /// </summary>
    public class MachineNameUserIdProvider : IUserIdProvider
    {
        /// <summary>
        /// Get the user id in {hostgroup}/{machinename} format
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public string GetUserId(HubConnectionContext connection)
        {
            var group = connection.User?.FindFirst("HostGroup")?.Value! ?? "none";
            var machineName = connection.User?.FindFirst(nameof(Environment.MachineName))?.Value;

            if (string.IsNullOrEmpty(machineName))
            {
                return Path.GetRandomFileName();
            }

            return $"{group}/{machineName}";
        }
    }
}
