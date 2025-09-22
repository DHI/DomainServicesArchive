namespace DHI.Services.Jobs.Automations.Test;

using System;
using AutoFixture.Xunit2;
using DHI.Services.Jobs.Automations.Triggers;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

public class JobCompletedTriggerTest : IClassFixture<JobRepositoryFixture>
{
    private readonly string _jobRepositoryFilePath;

    private readonly ILogger _logger;

    public JobCompletedTriggerTest(JobRepositoryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _logger = new TestLogger(testOutputHelper);
        _jobRepositoryFilePath = fixture.FilePath;
    }

    [Fact]
    public void CreateWithIllegalJobRepositoryTypeThrows()
    {
        var ex = Assert.Throws<ArgumentException>(() => new JobCompletedTrigger("trigger1", "myTask", typeof(int), "myConnectionString"));
        Assert.Contains($"Repository type '{typeof(int)}' does not implement interface '{nameof(IJobRepository<Guid, string>)}'.", ex.Message);
    }

    [Fact]
    public void IsMetShouldReturnTrueWhenCompleted()
    {
        var jobHasCompleted = new JobCompletedTrigger("trigger1", "myCompletedTask", typeof(JobRepository), _jobRepositoryFilePath);
        Assert.True(jobHasCompleted.Execute(_logger).IsMet);
    }

    [Fact]
    public void IsMetShouldReturnFalseWhenNotCompleted()
    {
        var jobHasCompleted = new JobCompletedTrigger("trigger1", "myPendingTask", typeof(JobRepository), _jobRepositoryFilePath);
        Assert.False(jobHasCompleted.Execute(_logger).IsMet);
    }

    [Theory, AutoData]
    public void IsMetShouldReturnFalseWhenTaskDoesNotExist(string task)
    {
        var jobHasCompleted = new JobCompletedTrigger("trigger1", task, typeof(JobRepository), _jobRepositoryFilePath);
        Assert.False(jobHasCompleted.Execute(_logger).IsMet);
    }
}