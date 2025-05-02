namespace DHI.Services.Jobs.Automations.TriggerParametersExport;

using System;
using System.ComponentModel.Composition;
using Triggers;

/// <summary>
///     The exported trigger parameters for the JobCompletedTrigger. 
/// </summary>
/// <remarks>
///     This is the definition read by the webapi and used to generate the UI.
/// </remarks>
[Export(typeof(ITriggerParameters))]
[ExportMetadata("Id", nameof(JobCompletedTrigger))]
public class JobCompletedTriggerParameters : IJobCompletedTriggerParameters, ITriggerParameters
{
    [TriggerParameter(true, title: "Repository Type")]
    public Type RepositoryType { get; set; }

    [TriggerParameter(true, title: "Connection String")]
    public string ConnectionString { get; set; }

    [TriggerParameter(true, title: "TaskId")]
    public string TaskId { get; set; }

    [TriggerParameter(true, title: "Description")]
    public string Description { get; set; }
}