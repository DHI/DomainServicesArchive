namespace DHI.Services.Jobs.Automations.Test;

using System;
using System.Globalization;
using AutoFixture.Xunit2;
using DHI.Services.Jobs.Automations.Triggers;
using Logging;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

public class ScheduledTriggerTest
{
    private readonly ILogger _logger;

    public ScheduledTriggerTest(ITestOutputHelper testOutputHelper)
    {
        _logger = new TestLogger(testOutputHelper);
    }

    [Theory]
    [InlineAutoData("00:00:01")]
    [InlineAutoData("00:00:10")]
    [InlineAutoData("00:00:59")]
    [InlineAutoData("-00:02:00")]
    public void CreateWithIntervalLessThanOneMinuteShouldThrow(string interval, DateTime startTime)
    {
        var e = Assert.Throws<ArgumentException>(() => new ScheduledTrigger("trigger1", "myScheduledTrigger", startTime, TimeSpan.Parse(interval, CultureInfo.InvariantCulture)));
        Assert.Contains("cannot be less than one minute.", e.Message);
    }

    [Theory, AutoData]
    public void IsMetWithoutParametersShouldThrow(DateTime startTime)
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger", startTime, TimeSpan.FromDays(1));
        Assert.Throws<ArgumentNullException>(() => scheduledTrigger.Execute(_logger));
    }

    [Theory, AutoData]
    public void IsMetWithMissingUtcNowParameterShouldThrow(DateTime startTime)
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger", startTime, TimeSpan.FromDays(1));
        var parameters = new Parameters { { "now", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) } };
        var e = Assert.Throws<ArgumentException>(() => scheduledTrigger.Execute(_logger, parameters));
        Assert.Contains("A parameter 'utcNow' with a DateTime literal representing the current UTC time must be given.", e.Message);
    }

    [Theory, AutoData]
    public void IsMetWithIllegalUtcNowParameterShouldThrow(DateTime startTime)
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger", startTime, TimeSpan.FromDays(1));
        var parameters = new Parameters { { "utcNow", "IllegalDateTimeLiteral" } };
        var e = Assert.Throws<FormatException>(() => scheduledTrigger.Execute(_logger, parameters));
        Assert.Contains("not recognized as a valid DateTime.", e.Message);
    }

    [Fact]
    public void IsMetShouldReturnFalseIfUtcNowIsLessThanStartTime()
    {
        var utcNow = DateTime.UtcNow;
        var startTime = utcNow.AddDays(1);
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger", startTime, TimeSpan.FromDays(1));
        var parameters = new Parameters { { "utcNow", utcNow.ToString(CultureInfo.InvariantCulture) } };
        Assert.False(scheduledTrigger.Execute(_logger, parameters).IsMet);
    }

    [Theory]
    [InlineData("13:00:00", "12:00:00", "00:30:00")]
    [InlineData("13:00:01", "12:00:00", "00:30:00")]
    [InlineData("13:00:29", "12:00:00", "00:30:00")]
    [InlineData("14:30:01", "12:00:00", "00:30:00")]
    [InlineData("14:10:01", "12:00:00", "00:10:00")]
    [InlineData("2022-01-03 00:00:00", "2022-01-01 00:00:00", "2.00:00:00")]
    [InlineData("2022-01-03 00:00:29", "2022-01-01 00:00:00", "2.00:00:00")]
    public void IsMetShouldReturnTrue(string utcNow, string startTime, string interval)
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger",
            DateTime.Parse(startTime, CultureInfo.InvariantCulture),
            TimeSpan.Parse(interval));
        var parameters = new Parameters { { "utcNow", $"{utcNow}" } };
        Assert.True(scheduledTrigger.Execute(_logger, parameters).IsMet);
    }

    [Theory]
    [InlineData("12:59:59", "12:00:00", "00:30:00")]
    [InlineData("13:00:31", "12:00:00", "00:30:00")]
    [InlineData("13:16:17", "12:00:00", "00:30:00")]
    [InlineData("14:29:59", "12:00:00", "00:30:00")]
    [InlineData("14:10:31", "12:00:00", "00:10:00")]
    [InlineData("2022-01-02 00:00:00", "2022-01-01 00:00:00", "2.00:00:00")]
    [InlineData("2022-01-03 00:00:31", "2022-01-01 00:00:00", "2.00:00:00")]
    public void IsMetShouldReturnFalse(string utcNow, string startTime, string interval)
    {
        var scheduledTrigger = new ScheduledTrigger("trigger1", "myScheduledTrigger",
            DateTime.Parse(startTime, CultureInfo.InvariantCulture),
            TimeSpan.Parse(interval));
        var parameters = new Parameters { { "utcNow", $"{utcNow}" } };
        Assert.False(scheduledTrigger.Execute(_logger, parameters).IsMet);
    }
}