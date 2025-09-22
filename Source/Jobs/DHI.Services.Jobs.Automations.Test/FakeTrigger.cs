namespace DHI.Services.Jobs.Automations.Test;

using System.Collections.Generic;
using Automations;
using Microsoft.Extensions.Logging;

public abstract class FakeTrigger : BaseTrigger
{
    private readonly bool _value;

    protected FakeTrigger(string id, string description, bool isEnabled, bool value) : base(id, description)
    {
        IsEnabled = isEnabled;
        _value = value;
    }

    public override AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null)
    {
        var outputParameters = new Dictionary<string, string>
        {
            ["trigger"] = Id,
            ["value"] = _value.ToString()
        };
        return _value
            ? AutomationResult.Met(outputParameters)
            : AutomationResult.NotMet();
    }
}

public class FalseTrigger : FakeTrigger
{
    public FalseTrigger(bool isEnabled) : base("falsetrigger", $"{nameof(FalseTrigger)}-{(isEnabled ? "enabled" : "disabled")}", isEnabled, false)
    {
    }
}

public class TrueTrigger : FakeTrigger
{
    public TrueTrigger(bool isEnabled) : base("truetrigger", $"{nameof(TrueTrigger)}-{(isEnabled ? "enabled" : "disabled")}", isEnabled, true)
    {
    }
}

public class TestTrigger : FakeTrigger
{
    public TestTrigger(string id, string description, bool isEnabled, bool value) : base(id, description, isEnabled, value)
    {
        Id = id;
    }
}