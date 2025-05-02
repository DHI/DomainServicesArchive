namespace DHI.Services.Jobs.Automations.Test;

using System;
using System.Collections.Generic;
using Automations;
using Triggers;
using Logging;
using Scalars;
using Xunit;
using Xunit.Abstractions;

public class TriggerConditionTest
{
    private readonly AutomationExecutor _executor;

    public TriggerConditionTest(ITestOutputHelper testOutputHelper)
    {
        var fakeScalarService = new ScalarService<int>(new FakeScalarRepository<int>());
        _executor = new AutomationExecutor(new TestLogger(testOutputHelper), fakeScalarService);
    }

    [Fact]
    public void ToStringShouldReturnEmptyStringIfEmpty()
    {
        var condition = new TriggerCondition(new List<ITrigger>());
        Assert.Empty(condition.ToString());
    }

    [Fact]
    public void ToStringIsOk()
    {
        var triggers = new List<ITrigger>() { new TrueTrigger(true), new FalseTrigger(false) };
        var condition = new TriggerCondition(triggers);
        Assert.Equal("TrueTrigger-enabled AND FalseTrigger-disabled", condition.ToString());
    }

    [Fact]
    public void IsMetShouldReturnFalseIfEmpty()
    {
        var triggers = new List<ITrigger>();
        var condition = new TriggerCondition(triggers);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.False(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnFalseIfOnePredicateIsFalse()
    {
        var triggers = new List<ITrigger>() { new TrueTrigger(true), new FalseTrigger(true) };
        var condition = new TriggerCondition(triggers);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.False(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueIfAllTriggersAreTrue()
    {
        var triggers = new List<ITrigger>() { new TrueTrigger(true), new TrueTrigger(true) };
        var condition = new TriggerCondition(triggers);
        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueIfAllEnabledTriggersAreTrue()
    {
        var triggers = new List<ITrigger>() { new TrueTrigger(true), new FalseTrigger(false) };
        var condition = new TriggerCondition(triggers);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueIfAllTriggersAreDisabled()
    {
        var triggers = new List<ITrigger>() { new TrueTrigger(false), new FalseTrigger(false) };
        var condition = new TriggerCondition(triggers);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueIfScheduledTriggerIsMet()
    {
        var startTime = new DateTime(2023, 1, 1);
        var interval = TimeSpan.FromHours(12);
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger", startTime, interval);
        var triggers = new List<ITrigger>() { scheduledTrigger, new TrueTrigger(true) };
        var condition = new TriggerCondition(triggers);
        var parameters = new Parameters { { "utcNow", "2023-02-03 12:00:00" } };

        var automation = new Automation("automation1", "group1", "task1")
        {
            Parameters = parameters,
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Theory]
    [InlineData(true, true, "AND", true)]
    [InlineData(true, false, "AND", false)]
    [InlineData(false, false, "AND", false)]
    [InlineData(false, true, "AND", false)]
    [InlineData(true, true, "OR", true)]
    [InlineData(true, false, "OR", true)]
    [InlineData(false, false, "OR", false)]
    [InlineData(false, true, "OR", true)]
    public void IsMetShouldExecuteConditionalOrderForTwoTriggers(bool isTrigger1Met, bool isTrigger2Met, string logicalOperator, bool expected)
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, isTrigger1Met);
        var fakeTrigger2 = new TestTrigger("trigger2", "Test trigger 2", true, isTrigger2Met);
        var triggers = new List<ITrigger>() { fakeTrigger1, fakeTrigger2 };
        var conditional = $"trigger1 {logicalOperator} trigger2";
        var condition = new TriggerCondition(triggers, conditional);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.Equal(expected, _executor.Execute(automation).IsMet);
    }

    [Theory]
    [InlineData(true, true, true, "AND", "AND", true)]
    [InlineData(true, true, true, "AND", "OR", true)]
    [InlineData(true, false, true, "AND", "OR", true)]
    [InlineData(false, true, true, "AND", "OR", true)]
    [InlineData(true, true, false, "AND", "OR", true)]
    [InlineData(true, false, false, "AND", "AND", false)]
    [InlineData(true, false, false, "AND", "OR", false)]
    [InlineData(true, false, false, "OR", "AND", false)]
    [InlineData(false, false, false, "OR", "AND", false)]
    [InlineData(false, true, false, "OR", "AND", false)]
    public void IsMetShouldExecuteConditionalOrderForThreeTriggers(bool isTrigger1Met, bool isTrigger2Met, bool isTrigger3Met, string logicalOperator1, string logicalOperator2, bool expected)
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, isTrigger1Met);
        var fakeTrigger2 = new TestTrigger("trigger2", "Test trigger 2", true, isTrigger2Met);
        var fakeTrigger3 = new TestTrigger("trigger3", "Test trigger 3", true, isTrigger3Met);
        var triggers = new List<ITrigger>() { fakeTrigger1, fakeTrigger2, fakeTrigger3 };
        var conditional = $"(trigger1 {logicalOperator1} trigger2) {logicalOperator2} trigger3";
        var condition = new TriggerCondition(triggers, conditional);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.Equal(expected, _executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueWithEmptyConditional()
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, true);
        var triggers = new List<ITrigger>() { fakeTrigger1 };
        var conditional = "";
        var condition = new TriggerCondition(triggers, conditional);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueWithNullConditional()
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, true);
        var triggers = new List<ITrigger>() { fakeTrigger1 };
        var condition = new TriggerCondition(triggers);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnTrueWithNoConditional()
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, true);
        var triggers = new List<ITrigger>() { fakeTrigger1 };
        var condition = new TriggerCondition(triggers);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void IsMetShouldThrowExpectedIfMismatchedConditional()
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, true);
        var triggers = new List<ITrigger>() { fakeTrigger1 };
        var conditional = "trigger1 AND trigger2";
        var condition = new TriggerCondition(triggers, conditional);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };

        Assert.Throws<KeyNotFoundException>(() => _executor.Execute(automation));
    }

    [Fact]
    public void IsMetShouldThrowExpectedIfWrongConditional()
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, true);
        var fakeTrigger2 = new TestTrigger("trigger2", "Test trigger 2", true, false);
        var triggers = new List<ITrigger>() { fakeTrigger1, fakeTrigger2 };
        var conditional = "trigger1";
        var condition = new TriggerCondition(triggers, conditional);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition
        };
        Assert.True(_executor.Execute(automation).IsMet);
    }

    [Fact]
    public void TrimTaskParametersIsOk()
    {
        var fakeTrigger1 = new TestTrigger("trigger1", "Test trigger 1", true, true);
        var fakeTrigger2 = new TestTrigger("trigger2", "Test trigger 2", true, false);
        var triggers = new List<ITrigger>() { fakeTrigger1, fakeTrigger2 };
        var conditional = "trigger1";
        var condition = new TriggerCondition(triggers, conditional);

        var automation = new Automation("automation1", "group1", "task1")
        {
            TriggerCondition = condition,
            TaskParameters = new Dictionary<string, string>()
            {
                ["1"] = "1",
                ["value"] = "100"
            }
        };
        var result = _executor.Execute(automation);
        Assert.True(result.IsMet);
        Assert.Equal(2, result.TaskParameters.Count);
        Assert.Equal("True", result.TaskParameters["value"]);
    }
}