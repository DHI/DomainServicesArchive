using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Cache for responses to the Report message.
    /// </summary>
    public class ReportCache : ConcurrentDictionary<string, (Dictionary<string, object>, DateTime)>
    {
    }
}
