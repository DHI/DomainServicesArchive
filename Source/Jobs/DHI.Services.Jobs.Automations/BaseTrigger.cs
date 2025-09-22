namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

[Serializable]
public abstract class BaseTrigger : ITrigger
{
    protected BaseTrigger(string id, string description)
    {
        Guard.Against.NullOrEmpty(id, nameof(id));
        Guard.Against.NullOrEmpty(description, nameof(description));
        Id = id;
        Description = description;
        Type = GetType();
    }

    public string Description { get; protected set; }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public bool IsEnabled { get; protected set; } = true;

    public Type Type { get; private set; }

    public string Id { get; protected set; }

    public abstract AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null);

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);

    public string? this[string key]
    {
        get => Extra.TryGetValue(key, out var el) ? el.GetString() : null;
        set => Extra[key] =
            JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement;
    }
}