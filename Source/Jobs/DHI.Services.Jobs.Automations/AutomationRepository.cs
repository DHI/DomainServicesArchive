namespace DHI.Services.Jobs.Automations;

using System.Globalization;
using System;
using System.IO;
using System.Security.Claims;

public class AutomationRepository<TTaskId> : GroupedJsonRepository<Automation<TTaskId>>, IAutomationRepository<TTaskId>
{
    private readonly string _versionFilePath;
    private static readonly object _versionLock = new();
    public AutomationRepository(string filePath) : base(filePath, AutomationRepositoryConverters.Required)
    {
        var resolvedPath = filePath.TryResolveFullPath();
        var dir = Path.GetDirectoryName(resolvedPath) ?? ".";

        Directory.CreateDirectory(dir);
        _versionFilePath = Path.Combine(dir, "version.txt");
    }

    public DateTime GetVersionTimestamp()
    {
        lock (_versionLock)
        {
            if (!File.Exists(_versionFilePath))
            {
                var now = DateTime.UtcNow;
                File.WriteAllText(_versionFilePath, now.ToString("O"));
                return now;
            }

            var text = File.ReadAllText(_versionFilePath);
            return DateTime.TryParseExact(text, "O", CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeUniversal, out var ts)
                   ? ts.ToUniversalTime()
                   : DateTime.MinValue;
        }
    }

    public DateTime TouchVersion()
    {
        lock (_versionLock)
        {
            var now = DateTime.UtcNow;
            File.WriteAllText(_versionFilePath, now.ToString("O"));
            return now;
        }
    }

    public new void Add(Automation<TTaskId> entity, ClaimsPrincipal user = null)
    {
        base.Add(entity, user);
        TouchVersion();
    }

    public new void Update(Automation<TTaskId> entity, ClaimsPrincipal user = null)
    {
        base.Update(entity, user);
        TouchVersion();
    }

    public new void Remove(string id, ClaimsPrincipal user = null)
    {
        base.Remove(id, user);
        TouchVersion();
    }
}

public class AutomationRepository : AutomationRepository<string>, IAutomationRepository
{
    public AutomationRepository(string filePath) : base(filePath)
    {
    }
}