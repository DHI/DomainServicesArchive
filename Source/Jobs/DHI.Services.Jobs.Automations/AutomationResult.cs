namespace DHI.Services.Jobs.Automations;

using System.Collections.Generic;

public struct AutomationResult
{
    public static AutomationResult Met()
    {
        return new AutomationResult(true, new Parameters());
    }

    public static AutomationResult Met(IDictionary<string, string> result)
    {
        return new AutomationResult(true, result);
    }

    public static AutomationResult NotMet()
    {
        return new AutomationResult(false, new Parameters());
    }

    private AutomationResult(bool isMet, IDictionary<string, string> taskParameters)
    {
        IsMet = isMet;
        TaskParameters = taskParameters;
    }

    public IDictionary<string, string> TaskParameters { get; }

    public bool IsMet { get; }
}