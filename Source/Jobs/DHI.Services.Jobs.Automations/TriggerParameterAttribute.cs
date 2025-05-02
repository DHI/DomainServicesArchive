namespace DHI.Services.Jobs.Automations;

using System;

/// <summary>
///     Used to ignore a property when creating trigger parameter metadata for webapi
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TriggerParameterIgnoreAttribute : Attribute
{
}

/// <summary>
///     Used to specify metadata for a trigger parameter when creating trigger parameter metadata for webapi
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TriggerParameterAttribute : Attribute
{
    public TriggerParameterAttribute(bool required, string title = null, string description = null, string format = null)
    {
        Required = required;
        Title = title;
        Description = description;
        Format = format;
    }

    public string Title { get; set; }
    public bool Required { get; set; }
    public string Description { get; set; }
    public string Format { get; }
}