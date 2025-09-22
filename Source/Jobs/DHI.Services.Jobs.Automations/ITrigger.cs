namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

public interface ITrigger
{
    string Id { get; }
    string Description { get; }
    bool IsEnabled { get; }

    [JsonInclude]
    public Type Type { get; }

    AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null);
}