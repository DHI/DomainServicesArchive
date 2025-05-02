namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class TriggerCondition
{
    private readonly List<ITrigger> _triggers;
    private readonly string _conditional;

    public TriggerCondition(List<ITrigger> triggers, string conditional = null)
    {
        _triggers = triggers;
        _conditional = conditional;
    }

    public List<ITrigger> Triggers => _triggers;

    public string Conditional => _conditional;

    public override string ToString()
    {
        if (string.IsNullOrEmpty(_conditional))
        {
            var sb = new StringBuilder();
            foreach (var predicate in _triggers)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append(predicate.Description);
            }

            return sb.ToString();
        }

        return _conditional;
    }
}