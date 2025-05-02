using System;
using System.Collections.Concurrent;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Cache of SignalR client availability
    /// </summary>
    public class AvailableCache : ConcurrentDictionary<string, (bool, DateTime)>
    {
    }
}
