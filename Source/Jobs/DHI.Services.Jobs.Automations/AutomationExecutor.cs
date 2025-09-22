namespace DHI.Services.Jobs.Automations;

using DHI.Services.Jobs.Automations.Triggers;
using Expressions;
using Microsoft.Extensions.Logging;
using Scalars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class AutomationExecutor
{
    private readonly ScalarService<int> _scalarService;
    private readonly string _rootGroup;
    private readonly ILogger _logger;

    public AutomationExecutor(ILogger logger, ScalarService<int> scalarService, string rootGroup = "")
    {
        Guard.Against.Null(logger, nameof(logger));
        Guard.Against.Null(scalarService, nameof(scalarService));
        _scalarService = scalarService;
        _rootGroup = rootGroup;
        _logger = logger;
    }

    public AutomationResult Execute<T>(Automation<T> automation, IDictionary<string, string> runParams)
    {
        Guard.Against.Null(automation, nameof(automation));
        _logger.LogDebug("Executing automation {AutomationId}", automation.Id);

        if (!automation.IsEnabled)
        {
            _logger.LogInformation("Automation {AutomationId} is disabled", automation.Id);
            return AutomationResult.NotMet();
        }

        var conditional = automation.TriggerCondition?.Conditional ?? string.Empty;
        var triggers = automation.TriggerCondition?.Triggers ?? new List<ITrigger>();
        var isImplicitAnd = string.IsNullOrEmpty(conditional);

        if (isImplicitAnd && triggers.Count == 0)
        {
            WriteTriggerResultScalar(automation.Id, false, automation.HostGroup);
            _logger.LogInformation("Conditional Failed: No triggers defined; exiting early");
            return AutomationResult.NotMet();
        }

        var mergedParameters = new Dictionary<string, string>(automation.TaskParameters);

        var (evaluated, updatedParameters) = EvaluateTriggers(
                automationId: automation.Id,
                hostGroup: automation.HostGroup,
                triggers: triggers,
                baseParameters: mergedParameters,
                shortCircuitOnFirstFailure: isImplicitAnd,
                runParams: runParams
            );

        mergedParameters = updatedParameters;

        bool isMet;
        if (isImplicitAnd)
        {
            isMet = triggers
                .Where(t => t.IsEnabled)
                .All(t => evaluated.TryGetValue(t.Id, out var ok) && ok);

            _logger.LogInformation("Result: {Result}", isMet);
        }
        else
        {
            isMet = ExpressionFactories.EvaluateExpressionNotation(conditional, ref evaluated);
            _logger.LogInformation("Result: {Result}", isMet);
        }

        if (!isMet)
        {
            return AutomationResult.NotMet();
        }

        var trimmed = TrimTaskParameters(automation.TaskParameters.Keys, mergedParameters);
        return AutomationResult.Met(trimmed);
    }

    public AutomationResult Execute<T>(Automation<T> automation)
            => Execute(automation, runParams: null);

    /// <summary>
    /// Evaluates triggers, writes per-trigger scalars, and merges parameters from passing triggers.
    /// When shortCircuitOnFirstFailure is true, stops at the first failing trigger.
    /// Returns the pass/fail map and the merged parameters at the time evaluation ended.
    /// </summary>
    private (Dictionary<string, bool> evaluated, Dictionary<string, string> mergedParameters) EvaluateTriggers(string automationId, string hostGroup, IList<ITrigger> triggers, Dictionary<string, string> baseParameters, bool shortCircuitOnFirstFailure, IDictionary<string, string> runParams)
    {
        var evaluated = new Dictionary<string, bool>();
        var parameters = new Dictionary<string, string>(baseParameters);

        var scheduledOverlayKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "utcNow", "utcPrev", "toleranceSeconds"
        };

        foreach (var trigger in triggers)
        {
            _logger.LogDebug("Evaluating trigger {TriggerId}", trigger.Id);

            if (!trigger.IsEnabled)
            {
                _logger.LogInformation("Skipped: Trigger {TriggerId} is disabled", trigger.Id);
                continue;
            }

            var executionParameters = BuildExecutionParameters(parameters, trigger);

            if (runParams is not null && (trigger is ScheduledTrigger ||
                    trigger.Type == typeof(ScheduledTrigger)))
            {
                foreach (var k in scheduledOverlayKeys)
                {
                    if (runParams.TryGetValue(k, out var v) && v is not null)
                    {
                        executionParameters[k] = v;
                    }
                }
            }

            var triggerResult = trigger.Execute(_logger, executionParameters);

            WriteTriggerResultScalar(automationId, triggerResult.IsMet, hostGroup, trigger.Id);

            if (!triggerResult.IsMet)
            {
                _logger.LogInformation("Failed: Trigger {TriggerId} is not met", trigger.Id);
                evaluated[trigger.Id] = false;

                if (shortCircuitOnFirstFailure)
                {
                    return (evaluated, parameters);
                }

                continue;
            }

            _logger.LogInformation("Passed: Trigger {TriggerId} is met", trigger.Id);
            evaluated[trigger.Id] = true;

            foreach (var kv in triggerResult.TaskParameters)
            {
                parameters[kv.Key] = kv.Value;
            }
        }

        return (evaluated, parameters);
    }

    /// <summary>
    /// Builds the effective parameter set used to execute a trigger by layering `Extra` (if present) on top of the current parameters.
    /// Properly converts JsonElement values to strings.
    /// </summary>
    private Dictionary<string, string> BuildExecutionParameters(
        Dictionary<string, string> currentParameters,
        ITrigger trigger)
    {
        var executionParameters = new Dictionary<string, string>(currentParameters);

        if (trigger is BaseTrigger baseTrigger && baseTrigger.Extra is not null)
        {
            foreach (var kvp in baseTrigger.Extra)
            {
                try
                {
                    string valueString = kvp.Value.ValueKind switch
                    {
                        JsonValueKind.String => kvp.Value.GetString(),
                        JsonValueKind.Number => kvp.Value.ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => null,
                        _ => kvp.Value.ToString()
                    };

                    if (valueString != null)
                    {
                        executionParameters[kvp.Key] = valueString;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Extra field {FieldKey} on trigger {TriggerId}", kvp.Key, trigger.Id);
                }
            }
        }

        return executionParameters;
    }

    /// <summary>
    /// Safely trims to only the keys present in neededKeys (skips missing keys rather than throwing).
    /// </summary>
    private Dictionary<string, string> TrimTaskParameters(
        Dictionary<string, string>.KeyCollection neededKeys,
        Dictionary<string, string> taskParameters)
    {
        var trimmed = new Dictionary<string, string>();
        foreach (var key in neededKeys)
        {
            if (taskParameters.TryGetValue(key, out var value))
            {
                trimmed[key] = value;
            }
            else
            {
                _logger.LogDebug("TrimTaskParameters: missing key '{Key}' in merged parameters; skipping.", key);
            }
        }
        return trimmed;
    }

    private void WriteTriggerResultScalar(string automationId, bool isMet, string hostGroup, string triggerId = null)
    {
        string hostGroupOrEmpty = string.IsNullOrEmpty(hostGroup) ? "Empty" : hostGroup;
        var scalarGroup = $"{_rootGroup}/{automationId}/{hostGroupOrEmpty}";
        if (!string.IsNullOrEmpty(triggerId))
        {
            scalarGroup += $"/{triggerId}";
        }

        var scalarName = "Is Met";
        var scalarData = new ScalarData(isMet, DateTime.UtcNow);
        var scalar = new Scalar($"{scalarGroup}/{scalarName}", scalarName, "System.Boolean", scalarGroup, scalarData);
        _scalarService.TrySetDataOrAdd(scalar);
    }
}
