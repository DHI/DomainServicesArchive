namespace DHI.Services.Scalars.Test
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public class FakeLogger : ILogger
    {
        public List<string> LogEntries { get; } = new List<string>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            LogEntries.Add(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel > LogLevel.Trace;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }
    }
}