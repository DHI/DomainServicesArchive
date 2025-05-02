namespace DHI.Services.Jobs.Automations.Test;

using System.ComponentModel.Composition;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Automations.TriggerParametersExport;

[Export(typeof(ITriggerParameters))]
[ExportMetadata("Id", nameof(FalseTrigger))]
public class FalseTriggerParameters : ITriggerParameters
{
    [TriggerParameterIgnore]
    public string Id { get; set; } = "falsetrigger";

    [TriggerParameter(false, title: "Description")]
    public string Description
    {
        get => $"{nameof(FalseTrigger)}-{(IsEnabled ? "enabled" : "disabled")}";
        set
        {
            // do nothing
        }
    }

    [TriggerParameter(true, title: "Is Enabled")]
    public bool IsEnabled { get; set; }
}

[Export(typeof(ITriggerParameters))]
[ExportMetadata("Id", nameof(TrueTrigger))]
public class TrueTriggerParameters : ITriggerParameters
{
    [TriggerParameterIgnore]
    public string Id { get; set; } = "truetrigger";

    [TriggerParameter(true, title: "Description")]
    public string Description
    {
        get => $"{nameof(TrueTrigger)}-{(IsEnabled ? "enabled" : "disabled")}";
        set
        {
            // do nothing
        }
    }

    [TriggerParameter(true, title: "Is Enabled")]
    public bool IsEnabled { get; set; }
}

[Export(typeof(ITriggerParameters))]
[ExportMetadata("Id", nameof(TestTrigger))]
public class TestTriggerParameters : ITriggerParameters
{
    [TriggerParameterIgnore]
    public string Id { get; set; } = "testtrigger";

    [TriggerParameter(true, title: "Description")]
    public string Description
    {
        get => $"{nameof(TestTrigger)}-{(IsEnabled ? "enabled" : "disabled")}";
        set
        {
            // do nothing
        }
    }

    [TriggerParameter(true, title: "Is Enabled")]
    public bool IsEnabled { get; set; }
}