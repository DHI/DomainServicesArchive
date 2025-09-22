namespace DHI.Services.Jobs.Automations.Test;

using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

public class BaseTriggerTest
{
    private readonly ILogger _logger;

    public BaseTriggerTest(ITestOutputHelper testOutputHelper)
    {
        _logger = new TestLogger(testOutputHelper);
    }

    [Fact]
    public void EnableIsOk()
    {
        var trigger = new TrueTrigger(false);
        Assert.False(trigger.IsEnabled);
        trigger.Enable();
        Assert.True(trigger.IsEnabled);
    }

    [Fact]
    public void DisableIsOk()
    {
        var trigger = new TrueTrigger(true);
        Assert.True(trigger.IsEnabled);
        trigger.Disable();
        Assert.False(trigger.IsEnabled);
    }

    [Fact]
    public void ReturnTaskParametersIsOk()
    {
        var trigger = new TrueTrigger(true);
        var result = trigger.Execute(_logger);
        Assert.True(result.IsMet);
        Assert.True(result.TaskParameters.ContainsKey("trigger"));
    }
}