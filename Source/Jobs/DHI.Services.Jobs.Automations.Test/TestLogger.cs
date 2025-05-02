namespace DHI.Services.Jobs.Automations.Test;

using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class TestLogger : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestLogger(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _testOutputHelper.WriteLine($"{DateTime.UtcNow:HH:mm:ss} {logLevel.ToString()} {message}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel > LogLevel.Trace;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }
}