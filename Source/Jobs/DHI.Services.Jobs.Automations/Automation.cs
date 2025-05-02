namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

[Serializable]
public class Automation<TTaskId> : BaseGroupedEntity<string>
{
    public Automation(string name, string group, TTaskId taskId) : base(new FullName(group, name).ToString(), name, group)
    {
        Guard.Against.Null(taskId, nameof(taskId));
        TaskId = taskId;
    }

    public TTaskId TaskId { get; set; }

    public Dictionary<string, string> TaskParameters { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, string> Parameters { get; set; }

    public string HostGroup { get; set; }

    public int Priority { get; set; } = 1;

    public string Tag { get; set; }

    // Needed for private set with System.Text.Json
    [JsonInclude]
    public bool IsEnabled { get; private set; } = true;

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public TriggerCondition TriggerCondition { get; set; }
}

public class Automation : Automation<string>
{
    public Automation(string name, string group, string taskId) : base(name, group, taskId)
    {
    }
}