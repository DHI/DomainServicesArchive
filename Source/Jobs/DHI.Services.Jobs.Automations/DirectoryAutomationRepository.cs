namespace DHI.Services.Jobs.Automations;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

/// <summary>
/// A repository for automations that stores them in a directory tree. Each automation is stored in a separate file.
/// </summary>
/// <typeparam name="TTaskId"></typeparam>
public class DirectoryAutomationRepository<TTaskId> : IAutomationRepository<TTaskId>
{
    private readonly DirectoryInfo _directoryPath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly string _versionFilePath;
    private static readonly object _versionLock = new();

    /// <summary>
    /// Creates a new instance of <see cref="DirectoryAutomationRepository{TTaskId}"/>.
    /// </summary>
    /// <param name="directoryPath">The path to the root of the directory tree</param>
    /// <exception cref="ArgumentException">When directory path is not a valid path</exception>
    public DirectoryAutomationRepository(string directoryPath)
    {
        Guard.Against.NullOrWhiteSpace(directoryPath, nameof(directoryPath));
        var resolvedDirectoryPath = directoryPath.TryResolveFullPath()
                                    ?? throw new ArgumentException($"The directory path '{directoryPath}' does not exist.");


        _directoryPath = new DirectoryInfo(resolvedDirectoryPath);
        if (!_directoryPath.Exists)
        {
            _directoryPath.Create();
        }

        var attributes = File.GetAttributes(_directoryPath.FullName);
        if (!attributes.HasFlag(FileAttributes.Directory))
        {
            throw new ArgumentException($"The path '{_directoryPath.FullName}' is not a directory.");
        }

        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        Array.ForEach(AutomationRepositoryConverters.All.ToArray(), rc => _jsonSerializerOptions.Converters.Add(rc));

        _versionFilePath = Path.Combine(_directoryPath.FullName, "version.txt");
    }

    /// <inheritdoc cref="IRepository{TEntity,TEntityId}.Get"/>>
    /// <typeparam name="TEntity"><see cref="Automation{TaskId}"/></typeparam>
    public Maybe<Automation<TTaskId>> Get(string id, ClaimsPrincipal user = null)
    {
        var parsedId = ParseId(id);
        var filePath = Path.Combine(_directoryPath.FullName, parsedId.RelativeDirectory, parsedId.FileName);

        if (!filePath.StartsWith(_directoryPath.FullName))
        {
            return Maybe.Empty<Automation<TTaskId>>();
        }

        var file = new FileInfo(filePath);
        if (!file.Exists)
        {
            return Maybe.Empty<Automation<TTaskId>>();
        }

        if (!TryReadJson(filePath, out var automation))
        {
            return Maybe.Empty<Automation<TTaskId>>();
        }

        return automation.ToMaybe();
    }

    /// <inheritdoc cref="IDiscreteRepository{TEntity,TEntityId}.Count"/>>
    public int Count(ClaimsPrincipal user = null)
    {
        return _directoryPath.GetFiles("*.json", SearchOption.AllDirectories).Length;
    }

    /// <inheritdoc cref="IDiscreteRepository{TEntity,TEntityId}.Contains"/>>
    public bool Contains(string id, ClaimsPrincipal user = null)
    {
        return Get(id, user).HasValue;
    }

    /// <inheritdoc cref="IDiscreteRepository{TEntity,TEntityId}.GetAll"/>>
    /// <typeparam name="TEntity"><see cref="Automation{TTaskId}"/></typeparam>
    public IEnumerable<Automation<TTaskId>> GetAll(ClaimsPrincipal user = null)
    {
        var files = _directoryPath.GetFiles("*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (TryReadJson(file.FullName, out var automation))
            {
                yield return automation;
            }
        }
    }

    /// <inheritdoc cref="IDiscreteRepository{TEntity,TEntityId}.GetIds"></inheritdoc>
    public IEnumerable<string> GetIds(ClaimsPrincipal user = null)
    {
        return GetAll(user).Select(au => au.Id);
    }

    /// <inheritdoc cref="IUpdatableRepository{TEntity,TEntityId}.Add">></inheritdoc>
    public void Add(Automation<TTaskId> entity, ClaimsPrincipal user = null)
    {
        var id = entity.Id;
        var parsedId = ParseId(id);
        var file = new FileInfo(Path.Combine(_directoryPath.FullName, parsedId.RelativeDirectory, parsedId.FileName));
        if (!file.FullName.StartsWith(_directoryPath.FullName))
        {
            throw new ArgumentException($"The id '{id}' is not in the correct format.");
        }

        if (!file.Directory.Exists)
        {
            file.Directory.Create();
        }

        if (!TryWriteJson(file.FullName, entity))
        {
            throw new InvalidOperationException($"Failed to write the automation '{id}' to the file system.");
        }

        TouchVersion();
    }

    /// <inheritdoc cref="IUpdatableRepository{TEntity,TEntityId}.Remove">></inheritdoc>
    public void Remove(string id, ClaimsPrincipal user = null)
    {
        var parsedId = ParseId(id);
        var filePath = Path.Combine(_directoryPath.FullName, parsedId.RelativeDirectory, parsedId.FileName);
        if (!filePath.StartsWith(_directoryPath.FullName))
        {
            return;
        }

        var file = new FileInfo(filePath);
        if (file.Exists)
        {
            file.Delete();
            TouchVersion();
        }
    }

    /// <inheritdoc cref="IUpdatableRepository{TEntity,TEntityId}.Update">Where TEntity is <see cref="Automation{TaskId}"/></inheritdoc>
    public void Update(Automation<TTaskId> entity, ClaimsPrincipal user = null)
    {
        Add(entity, user);
    }

    /// <inheritdoc cref="IGroupedRepository{TEntity}.ContainsGroup"/>>
    public bool ContainsGroup(string group, ClaimsPrincipal user = null)
    {
        if (group == string.Empty)
        {
            return true;
        }

        var directory = new DirectoryInfo(Path.Combine(_directoryPath.FullName, group));
        if (!directory.FullName.StartsWith(_directoryPath.FullName))
        {
            return false;
        }

        return directory.Exists;
    }

    /// <inheritdoc cref="IGroupedRepository{TEntity}.GetByGroup"/>>
    /// <typeparam name="TEntity"><see cref="Automation{TTaskId}"/></typeparam>
    public IEnumerable<Automation<TTaskId>> GetByGroup(string group, ClaimsPrincipal user = null)
    {
        var directory = new DirectoryInfo(Path.Combine(_directoryPath.FullName, group));
        if (!directory.FullName.StartsWith(_directoryPath.FullName))
        {
            return Enumerable.Empty<Automation<TTaskId>>();
        }

        if (!directory.Exists)
        {
            return Enumerable.Empty<Automation<TTaskId>>();
        }

        var files = directory.GetFiles("*.json", SearchOption.AllDirectories);
        return files
            .Select(f => TryReadJson(f.FullName, out var automation) ? automation : null)
            .Where(a => a != null);
    }

    /// <inheritdoc cref="IGroupedRepository{TEntity}.GetFullNames(string, ClaimsPrincipal)"/>>
    public IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
    {
        var directory = new DirectoryInfo(Path.Combine(_directoryPath.FullName, group));
        if (!directory.FullName.StartsWith(_directoryPath.FullName))
        {
            return Enumerable.Empty<string>();
        }

        if (!directory.Exists)
        {
            return Enumerable.Empty<string>();
        }

        if (!directory.Exists)
        {
            return Enumerable.Empty<string>();
        }

        var files = directory.GetFiles("*.json", SearchOption.AllDirectories);
        return files.Select(f => f.FullName.Substring(_directoryPath.FullName.Length + 1) // +1 to remove the leading directory separator
            .Replace('\\', '/')
            .Replace(".json", ""));
    }

    /// <inheritdoc cref="IGroupedRepository{TEntity}.GetFullNames(ClaimsPrincipal)"/>>
    public IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
    {
        var files = _directoryPath.GetFiles("*.json", SearchOption.AllDirectories);
        return files
            .Select(f => TryReadJson(f.FullName, out var automation) ? automation.FullName : null)
            .Where(a => a != null);
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

    private bool TryReadJson(string filePath, out Automation<TTaskId> automation)
    {
        var contents = File.ReadAllText(filePath);
        automation = JsonSerializer.Deserialize<Automation<TTaskId>>(contents, _jsonSerializerOptions);
        return automation != null;
    }

    private bool TryWriteJson(string filePath, Automation<TTaskId> automation)
    {
        var contents = JsonSerializer.Serialize(automation, _jsonSerializerOptions);
        if (string.IsNullOrWhiteSpace(contents))
        {
            return false;
        }

        File.WriteAllText(filePath, contents);
        return true;
    }

    private DirectoryAutomationRepositoryId ParseId(string id)
    {
        var splits = id.Split(new[] { '/', '\\', Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        splits = splits.Where(s => s is not "..").ToArray();
        if (!splits.Any())
        {
            throw new ArgumentException($"The id '{id}' is not in the correct format. Expected a path in the form of 'group1/groups2..n/automation'.");
        }

        var path = Path.Combine(splits.Take(splits.Length - 1).ToArray());
        var name = splits.Last();
        if (!name.EndsWith(".json"))
        {
            name += ".json";
        }

        return new DirectoryAutomationRepositoryId(name, path);
    }

    private struct DirectoryAutomationRepositoryId
    {
        public DirectoryAutomationRepositoryId(string fileName, string relativeDirectory)
        {
            FileName = fileName;
            RelativeDirectory = relativeDirectory;
        }

        public string FileName { get; set; }
        public string RelativeDirectory { get; set; }
    }
}

public class DirectoryAutomationRepository : DirectoryAutomationRepository<string>, IAutomationRepository
{
    public DirectoryAutomationRepository(string filePath) : base(filePath)
    {
    }
}