namespace DHI.Services.Jobs.Workflows;

using System;
using System.IO;
using Microsoft.Extensions.Logging;

/// <summary>
/// Write logs to file during a workflow
/// </summary>
[Obsolete("Use a logging framework, this will be removed in a future update")]
public class WorkflowLogger : ILogger
{
    private readonly string _folder;
    private readonly string _source;
    private readonly string _tag;
    private readonly LogLevel _minimumLogLevel;

    /// <summary>
    /// Creates a logger which write to a file. Minimum log level is set to Information
    /// </summary>
    /// <param name="folder">The directory to write to</param>
    /// <param name="source">The source of the logs</param>
    /// <param name="tag">The name of the file</param>
    public WorkflowLogger(string folder, string source, string tag)
    {
        _folder = folder;
        _source = source;
        _tag = tag;
        _minimumLogLevel = LogLevel.Information;
        if (!Directory.Exists(_folder))
        {
            Directory.CreateDirectory(_folder);
        }
    }

    /// <summary>
    /// Creates a new Workflow Logger
    /// </summary>
    /// <param name="folder">The directory to write to</param>
    /// <param name="source">The source of the logs</param>
    /// <param name="tag">The name of the file</param>
    /// <param name="minimumLogLevel">The minimum log level to write</param>
    public WorkflowLogger(string folder, string source, string tag, LogLevel minimumLogLevel)
    {
        _folder = folder;
        _source = source;
        _tag = tag;
        _minimumLogLevel = minimumLogLevel;
        if (!Directory.Exists(_folder))
        {
            Directory.CreateDirectory(_folder);
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var fileName = Path.Combine(_folder, $"{_tag}.log");

        var message = formatter(state, exception);

        var entry = $"LogDateTime:{DateTime.UtcNow.ToUniversalTime():yyyy-MM-dd HH:mm:ss}. LogSource:{_source}. LogLevel:{logLevel}. LogText:{message}{Environment.NewLine}";

        try
        {
            File.AppendAllText(fileName, entry);
        }
        catch
        {
            // do nothing, logging failure shouldn't crash the application
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel > _minimumLogLevel;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }
}