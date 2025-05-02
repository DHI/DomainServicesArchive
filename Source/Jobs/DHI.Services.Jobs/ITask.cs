namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;

    public interface ITask<out TId> : INamedEntity<TId>
    {
        IDictionary<string, object> Parameters { get; }

        TimeSpan? Timeout { get; }

        string HostGroup { get; }
    }
}