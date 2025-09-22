namespace JobAutomator.Caching;
public sealed class CacheSettings
{
    /// <summary>Absolute TTL in seconds. After this the entry is *always* re-fetched.</summary>
    public int AutomationAbsoluteTtlSeconds { get; set; } = 60;

    /// <summary>If the entry is read again inside this window the clock resets.</summary>
    public int AutomationSlidingTtlSeconds { get; set; } = 30;

    /// <summary>Absolute or relative path for the local version.txt.</summary>
    public string LocalVersionFilePath { get; set; } = "version.txt";
}
