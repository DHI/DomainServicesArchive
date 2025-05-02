namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.Linq;
using Expressions;
using Logging;
using Microsoft.Extensions.Logging;
using Scalars;

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

    public AutomationResult Execute<T>(Automation<T> automation)
    {
        Guard.Against.Null(automation, nameof(automation));

        _logger.LogDebug("Executing automation {AutomationId}", automation.Id);

        if (!automation.IsEnabled)
        {
            _logger.LogInformation("Automation {AutomationId} is disabled", automation.Id);
            return AutomationResult.NotMet();
        }

        if (string.IsNullOrEmpty(automation.TriggerCondition.Conditional))
        {
            var res = ExecuteConditional(automation);
            if (res.IsMet)
            {
                // trimmed the combined parameters (incl. dynamically generated in trigger) to match the original task parameters
                // as a workflow task with additional parameters will be treated as an errorous submission when submitted to
                // web api.
                var trimmed = TrimTaskParameters(automation.TaskParameters.Keys, res.TaskParameters);
                return AutomationResult.Met(trimmed);
            }

            return res;
        }

        var evaluatedItems = new Dictionary<string, bool>();
        var parameters = automation?.Parameters?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, string>(); //copy
        foreach (var parameter in automation.TaskParameters)
        {
            parameters[parameter.Key] = parameter.Value;
        }

        foreach (var trigger in automation.TriggerCondition.Triggers)
        {
            _logger.LogDebug("Evaluating trigger {TriggerId}", trigger.Id);
            if (!trigger.IsEnabled)
            {
                _logger.LogInformation("Skipped: Trigger {TriggerId} is disabled", trigger.Id);
                continue;
            }

            var triggerResult = trigger.Execute(_logger, parameters);

            if (!triggerResult.IsMet)
            {
                WriteTriggerResultScalar(automation.Id, false, trigger.Id);
                _logger.LogInformation("Failed: Trigger {TriggerId} is not met", trigger.Id);
                evaluatedItems[trigger.Id] = false;
                continue;
            }

            _logger.LogInformation("Passed: Trigger {TriggerId} is met", trigger.Id);
            _logger.LogDebug("Writing Scalar for {TriggerId}", trigger.Id);

            WriteTriggerResultScalar(automation.Id, true, trigger.Id);
            evaluatedItems[trigger.Id] = true;

            foreach (var parameter in triggerResult.TaskParameters)
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }

        var result = ExpressionFactories.EvaluateExpressionNotation(automation.TriggerCondition.Conditional, ref evaluatedItems);
        _logger.LogInformation("Result: {Result}", result);
        if (result)
        {
            // trimmed the combined parameters (incl. dynamically generated in trigger) to match the original task parameters
            // as a workflow task with additional parameters will be treated as an errorous submission when submitted to
            // web api.
            var trimmed = TrimTaskParameters(automation.TaskParameters.Keys, parameters);
            return AutomationResult.Met(trimmed);
        }

        return AutomationResult.NotMet();
    }

    private IDictionary<string, string> TrimTaskParameters(Dictionary<string, string>.KeyCollection neededKeys, IDictionary<string, string> taskParameters)
    {
        return neededKeys.ToDictionary(k => k, k => taskParameters[k]);
    }

    private AutomationResult ExecuteConditional<T>(Automation<T> automation)
    {
        _logger.LogDebug("Executing conditional for automation {AutomationId}", automation.Id);
        if (automation.TriggerCondition.Triggers.Count == 0)
        {
            WriteTriggerResultScalar(automation.Id, false);
            _logger.LogInformation("Conditional Failed: No triggers defined; exiting early");
            return AutomationResult.NotMet();
        }

        var parameters = automation?.Parameters?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, string>(); //copy
        foreach (var parameter in automation.TaskParameters)
        {
            parameters[parameter.Key] = parameter.Value;
        }

        foreach (var trigger in automation.TriggerCondition.Triggers)
        {
            _logger.LogDebug("Evaluating trigger {TriggerId}", trigger.Id);
            if (!trigger.IsEnabled)
            {
                _logger.LogInformation("Conditional Skipped: Trigger {TriggerId} is disabled", trigger.Id);
                continue;
            }

            var triggerResult = trigger.Execute(_logger, parameters);
            if (!triggerResult.IsMet)
            {
                WriteTriggerResultScalar(automation.Id, false, trigger.Id);
                _logger.LogInformation("Conditional Failed: Trigger {TriggerId} is not met", trigger.Id);
                return AutomationResult.NotMet();
            }

            foreach (var parameter in triggerResult.TaskParameters)
            {
                parameters[parameter.Key] = parameter.Value;
            }

            _logger.LogInformation("Conditional Passed: Trigger {TriggerId} is met", trigger.Id);
            WriteTriggerResultScalar(automation.Id, true, trigger.Id);
        }

        _logger.LogInformation("Conditional Passed: All triggers are met");
        return AutomationResult.Met(parameters);
    }

    private void WriteTriggerResultScalar(string automationId, bool isMet, string triggerId = null)
    {
        var scalarGroup = $"{_rootGroup}/{automationId}";
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