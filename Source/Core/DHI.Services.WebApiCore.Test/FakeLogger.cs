namespace DHI.Services.WebApiCore.Test
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public class FakeLogger : ILogger
    {
        public List<(LogLevel LogLevel, string Text)> LogEntries { get; } = new();

        private readonly Func<string, Exception?, string> _messageFormatter = (s, e) => $"{s}{Environment.NewLine}{e?.Message}";

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogEntries.Add((logLevel, _messageFormatter(state.ToString(), exception)));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }
    }
}