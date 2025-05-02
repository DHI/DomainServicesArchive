namespace DHI.Services.Jobs.Automations.Test;

using System;
using System.Collections.Generic;
using System.IO;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Automations.Triggers;
using Xunit;

public class AutomationRepositoryTest : IDisposable
{
    private readonly string _filePath = Path.Combine(Path.GetTempPath(), "__automations.json");
    private readonly AutomationRepository _repository;
    private readonly string _jobRepositoryFilePath;

    public AutomationRepositoryTest()
    {
        var fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "__jobs.json"));
        fileInfo.Directory!.Create();
        _jobRepositoryFilePath = fileInfo.FullName;

        _repository = new AutomationRepository(_filePath);
        File.Copy(@"../../../Data/jobs.json", _jobRepositoryFilePath, true);
    }

    public void Dispose()
    {
        File.Delete(_filePath);
        File.Delete(_jobRepositoryFilePath);
    }

    [Fact]
    public void AddAndGetIsOk()
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "Trigger once every hour", new DateTime(2023, 01, 01), TimeSpan.FromHours(1));
        var jobCompletedTrigger = new JobCompletedTrigger("trigger2", "someTask", typeof(JobRepository), _jobRepositoryFilePath);
        var triggers = new List<ITrigger>() { scheduledTrigger, jobCompletedTrigger };
        var condition = new TriggerCondition(triggers);
        var automation = new Automation("myAutomation1", "myGroup", "myTask")
        {
            HostGroup = "hostGroup",
            Tag = "tag",
            TaskParameters = new Dictionary<string, string> { { "key", "value" }, { "number", "1" }, { "BoundariesJSonString", "[{\"ID\":\"TH\",\"west\":88.0,\"east\":110.0,\"south\":0.0,\"north\":18.0}]" } },
            Parameters = new Dictionary<string, string> { { "key", "value" } },
            TriggerCondition = condition,
        };
        automation.Disable();
        _repository.Add(automation);
        var maybe = _repository.Get(automation.Id);

        Assert.True(maybe.HasValue);
        var actual = maybe.Value;
        Assert.Equal(automation.Id, actual.Id);
        Assert.Equal(automation.Group, actual.Group);
        Assert.Equal(automation.Name, actual.Name);
        Assert.Equal(automation.FullName, actual.FullName);
        Assert.Equal(automation.HostGroup, actual.HostGroup);
        Assert.Equal(automation.Tag, actual.Tag);
        Assert.Equal("value", actual.TaskParameters["key"]);
        Assert.Equal("1", actual.TaskParameters["number"]);
        Assert.Equal("[{\"ID\":\"TH\",\"west\":88.0,\"east\":110.0,\"south\":0.0,\"north\":18.0}]", actual.TaskParameters["BoundariesJSonString"]);
        Assert.Equal("value", actual.Parameters["key"]);
        Assert.Equal(2, actual.TriggerCondition.Triggers.Count);
        Assert.IsType<ScheduledTrigger>(actual.TriggerCondition.Triggers[0]);
        Assert.IsType<JobCompletedTrigger>(actual.TriggerCondition.Triggers[1]);
        var trigger = actual.TriggerCondition.Triggers[0];
        Assert.Equal(scheduledTrigger.StartTimeUtc, ((ScheduledTrigger)trigger).StartTimeUtc);
        Assert.Equal(scheduledTrigger.Interval, ((ScheduledTrigger)trigger).Interval);
        Assert.False(actual.IsEnabled);
    }

    [Fact]
    public void DisableUpdateAndGetIsOk()
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "Trigger once every hour", new DateTime(2023, 01, 01), TimeSpan.FromHours(1));
        var jobCompletedTrigger = new JobCompletedTrigger("trigger2", "someTask", typeof(JobRepository), _jobRepositoryFilePath);
        var triggers = new List<ITrigger>() { scheduledTrigger, jobCompletedTrigger };
        var condition = new TriggerCondition(triggers);
        var automation = new Automation("myAutomation2", "myGroup1", "myTask")
        {
            HostGroup = "hostGroup",
            Tag = "tag",
            TaskParameters = new Dictionary<string, string> { { "key", "value" }, { "number", "1" }, { "BoundariesJSonString", "[{\"ID\":\"TH\",\"west\":88.0,\"east\":110.0,\"south\":0.0,\"north\":18.0}]" } },
            Parameters = new Dictionary<string, string> { { "key", "value" } },
            TriggerCondition = condition,
        };
        _repository.Add(automation);

        automation.Priority = 2;
        automation.Disable();
        _repository.Update(automation);
        var maybe = _repository.Get(automation.Id);
        var actual = maybe.Value;
        Assert.False(actual.IsEnabled);
    }
}