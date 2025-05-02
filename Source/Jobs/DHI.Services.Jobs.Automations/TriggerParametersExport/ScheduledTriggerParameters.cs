namespace DHI.Services.Jobs.Automations.TriggerParametersExport;

using System;
using System.ComponentModel.Composition;
using Triggers;

/// <summary>
///     The exported trigger parameters for the ScheduledTriggerParameters. 
/// </summary>
[Export(typeof(ITriggerParameters))]
[ExportMetadata("Id", nameof(ScheduledTrigger))]
public class ScheduledTriggerParameters : IScheduledTriggerParameters, ITriggerParameters
{
    [TriggerParameter(true, title: "Start Date Time in UTC", format: "date-time")]
    public DateTime StartTimeUtc { get; set; }

    [TriggerParameter(true, title: "Interval", format: "Format: d.hh:mm:ss.ff")]
    public TimeSpan Interval { get; set; }

    [TriggerParameter(true, title: "Description")]
    public string Description { get; set; }
}