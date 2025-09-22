namespace DHI.Services.Jobs.Automations;

using System.Collections.Generic;
using System.Linq;

public sealed class TriggerParameters : TriggerParameters<string>
{
    public TriggerParameters(string id, Dictionary<string, TriggerParameter> properties, IEnumerable<string> required) : base(id, properties, required)
    {
    }
}

/// <summary>
///     Entity for exposing trigger parameters through a repository.
/// </summary>
public class TriggerParameters<TTaskId> : BaseEntity<TTaskId>
{
    public TriggerParameters(TTaskId id, Dictionary<string, TriggerParameter> properties, IEnumerable<string> required)
        : base(id)
    {
        Properties = properties;
        Required = required.ToArray();
    }

    public Dictionary<string, TriggerParameter> Properties { get; set; }
    public string[] Required { get; set; }
    public string Type { get; set; } = "object";
    public string FullTypeName { get; set; }
    public string AssemblyName { get; set; }
    public string AssemblyQualifiedTypeName { get; set; }
}

/// <summary>
///     Details on a trigger parameter to expose through a repository.
/// </summary>
public class TriggerParameter
{
    public static TriggerParameter FromAttribute(TriggerParameterAttribute attribute)
    {
        return new TriggerParameter
        {
            Description = attribute.Description,
            Format = attribute.Format,
            Title = attribute.Title
        };
    }

    public string Description { get; set; }
    public string Format { get; set; }
    public string Title { get; set; }
    public string Type { get; set; } = "string";
    public string ItemsType { get; set; }
    public string[] EnumValues { get; set; }
}