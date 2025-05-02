namespace DHI.Services.Jobs.Automations.Test;

using System;
using DHI.Services.Jobs.Automations;
using Xunit;

public class AutomationTest
{
    [Fact]
    public void CreateWithoutTaskIdThrows()
    {
        Assert.Throws<ArgumentNullException>( () => new Automation("my-automation", "my-group", null));
    }

    [Fact]
    public void CreateIsOk()
    {
        var automation = new Automation("my-automation", "my-group", "my-task");
        Assert.Equal($"{automation.Group}/{automation.Name}", automation.Id);
        Assert.Equal("my-task", automation.TaskId);
        Assert.Equal(1, automation.Priority);
        Assert.True(automation.IsEnabled);
    }

    [Fact]
    public void CreateWithoutGroupIsOk()
    {
        var automation = new Automation("my-automation", null, "my-task");
        Assert.Equal($"{automation.Name}", automation.Id);
        Assert.Equal("my-task", automation.TaskId);
        Assert.Equal(1, automation.Priority);
        Assert.True(automation.IsEnabled);
    }

    [Fact]
    public void DisableAndEnableIsOk()
    {
        var automation = new Automation("my-automation", "my-group", "my-task");
        Assert.True(automation.IsEnabled);
        automation.Disable();
        Assert.False(automation.IsEnabled);
        automation.Enable();
        Assert.True(automation.IsEnabled);
    }
}