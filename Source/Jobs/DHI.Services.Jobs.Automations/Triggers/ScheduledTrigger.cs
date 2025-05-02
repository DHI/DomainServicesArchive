namespace DHI.Services.Jobs.Automations.Triggers;

using System;
using System.Collections.Generic;
using System.Globalization;
using Logging;
using Microsoft.Extensions.Logging;
using TriggerParametersExport;

[Serializable]
public class ScheduledTrigger : BaseTrigger, IScheduledTriggerParameters
{
    public DateTime StartTimeUtc { get; }

    public TimeSpan Interval { get; }

    public ScheduledTrigger(string id, string description, DateTime startTimeUtc, TimeSpan interval) : base(id, description)
    {
        StartTimeUtc = Guard.Against.Null(startTimeUtc, nameof(startTimeUtc));
        Interval = Guard.Against.Null(interval, nameof(interval));
        if (interval < TimeSpan.FromMinutes(1))
        {
            throw new ArgumentException($"The given interval '{interval}' cannot be less than one minute.", nameof(interval));
        }
    }

    public override AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null)
    {
        Guard.Against.Null(parameters, nameof(parameters));

        if (!parameters!.TryGetValue("utcNow", out var utcNowString))
        {
            throw new ArgumentException("A parameter 'utcNow' with a DateTime literal representing the current UTC time must be given.", nameof(parameters));
        }
        if (!DateTime.TryParse(utcNowString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var utcNow))
        {
            throw new FormatException($"The given parameter 'utcNow' with value '{utcNowString}' could not be parsed as a DateTime. It is not recognized as a valid DateTime.");
        }
        return utcNow > StartTimeUtc && IsWithinFirstHalfMinuteOfInterval(utcNow, StartTimeUtc, Interval)
            ? AutomationResult.Met()
            : AutomationResult.NotMet();
    }

    /// <summary>
    ///     Determines if the current UTC time is within the first half-minute of the current interval since the start time.
    /// </summary>
    /// <param name="utcNow">the time now in UTC</param>
    /// <param name="startTimeUtc">The start time in UTC.</param>
    /// <param name="interval">The interval time span.</param>
    /// <returns>True if within the first half-minute of the current interval, otherwise false.</returns>
    private static bool IsWithinFirstHalfMinuteOfInterval(DateTime utcNow, DateTime startTimeUtc, TimeSpan interval)
    {
        var minutesSinceStart = utcNow.Subtract(startTimeUtc).TotalMinutes;
        var remainder = minutesSinceStart % interval.TotalMinutes;

        return remainder < 0.5;
    }
}