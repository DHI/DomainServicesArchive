namespace DHI.Services;

using System;

public class LoginAttemptPolicy
{
    /// <summary>
    ///     Gets or sets the maximum login tries. Defaults to 3.
    /// </summary>
    public int MaxNumberOfLoginAttempts { get; set; } = 3;

    /// <summary>
    ///     Gets or sets the Time Span of the reset interval. Defaults to 1 Minutes.
    /// </summary>
    public TimeSpan ResetInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Gets or sets the Days of the Locked Account Time. Defaults to 30 Days.
    /// </summary>
    public TimeSpan LockedPeriod { get; set; } = TimeSpan.FromDays(30);


}

