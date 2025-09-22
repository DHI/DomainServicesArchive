namespace DHI.Services.Jobs.Automations.Triggers;

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;

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
        var (nowOpt, prevOpt) = TryGetCycleTimes(parameters);
        var now = nowOpt ?? DateTime.UtcNow;

        if (Interval <= TimeSpan.Zero || now < StartTimeUtc)
            return AutomationResult.NotMet();

        var tolerance = TryGetTolerance(parameters) ?? TimeSpan.FromSeconds(30);

        if (prevOpt is null || now <= prevOpt.Value)
        {
            var met = IsWithinToleranceWindow(now, StartTimeUtc, Interval, tolerance);
            logger?.LogDebug("ScheduledTrigger {Id}: fallback tolerance (prev missing/invalid) → {Met}", Id, met);
            return met ? AutomationResult.Met() : AutomationResult.NotMet();
        }

        var result = HasBoundarySince(prevOpt.Value, now, StartTimeUtc, Interval);
        return result ? AutomationResult.Met() : AutomationResult.NotMet();
    }

    /// <summary>Reads parameters["utcNow"] and ["utcPrev"] as UTC datetimes.</summary>
    private static (DateTime? now, DateTime? prev) TryGetCycleTimes(IDictionary<string, string> parameters)
    {
        DateTime? Parse(string key)
        {
            if (parameters is null || !parameters.TryGetValue(key, out var s) || string.IsNullOrWhiteSpace(s))
                return null;

            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                return dto.UtcDateTime;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return dt.ToUniversalTime();

            throw new FormatException(
                $"Parameter '{key}' value '{s}' could not be parsed as a DateTime. It is not recognized as a valid DateTime.");
        }

        return (Parse("utcNow"), Parse("utcPrev"));
    }

    /// <summary>Reads parameters["toleranceSeconds"] if present and valid.</summary>
    private static TimeSpan? TryGetTolerance(IDictionary<string, string> parameters)
    {
        if (parameters is null || !parameters.TryGetValue("toleranceSeconds", out var s) || string.IsNullOrWhiteSpace(s))
            return null;

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds >= 0)
            return TimeSpan.FromSeconds(seconds);

        return null;
    }

    private static bool HasBoundarySince(DateTime prevUtc, DateTime nowUtc, DateTime startUtc, TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero || nowUtc < startUtc)
            return false;

        if (prevUtc < startUtc)
            return true;

        var kPrev = (long)((prevUtc - startUtc).Ticks / interval.Ticks);
        var kNow = (long)((nowUtc - startUtc).Ticks / interval.Ticks);

        return kNow > kPrev;
    }

    private static bool IsWithinToleranceWindow(DateTime utcNow, DateTime startUtc, TimeSpan interval, TimeSpan tolerance)
    {
        if (utcNow < startUtc || interval <= TimeSpan.Zero || tolerance <= TimeSpan.Zero)
            return false;

        var cappedTolTicks = Math.Min(tolerance.Ticks, Math.Max(1, interval.Ticks - 1));
        var cappedTolerance = TimeSpan.FromTicks(cappedTolTicks);

        var elapsed = utcNow - startUtc;
        var remainderTicks = elapsed.Ticks % interval.Ticks;
        var remainder = TimeSpan.FromTicks(remainderTicks);

        return remainder <= cappedTolerance;
    }
}
