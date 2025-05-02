namespace DHI.Services.Jobs.Automations;

using System;

internal interface IScheduledTriggerParameters
{
    public DateTime StartTimeUtc { get; }

    public TimeSpan Interval { get; }
}