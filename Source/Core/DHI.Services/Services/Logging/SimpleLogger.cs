#nullable enable
namespace DHI.Services.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Simple Logger.
    /// </summary>
    public class SimpleLogger : ILogger
    {
        private readonly string _filePath;
        private readonly LogLevel _minimumLogLevel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleLogger" /> class with minimum log level set to information
        /// </summary>
        /// <param name="filePath">The filePath.</param>
        public SimpleLogger(string filePath) : this(filePath, LogLevel.Information)
        {
            
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleLogger" /> class.
        /// </summary>
        /// <param name="filePath">The filePath.</param>
        /// <param name="minimumLogLevel"></param>
        public SimpleLogger(string filePath, LogLevel minimumLogLevel)
        {
            Guard.Against.NullOrEmpty(filePath, nameof(filePath));
            var directory = Path.GetDirectoryName(filePath);
            if (directory is not null && directory != "" && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _filePath = filePath;
            _minimumLogLevel = minimumLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var entry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} {eventId.Name} [{logLevel}] - {formatter(state, exception)}{Environment.NewLine}";
            try
            {
                File.AppendAllText(_filePath, entry);
            }
            catch
            {
                // ignored
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minimumLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return SimpleLoggerState.Attach(state.ToString());
        }

        private class SimpleLoggerState : IDisposable
        {
            private readonly Stack<string> _stateStack = new();

            private static readonly SimpleLoggerState _instance = new();

            public static SimpleLoggerState Attach(string state)
            {
                _instance._stateStack.Push(state);
                return _instance;
            }

            private SimpleLoggerState()
            {
            }

            public void Dispose()
            {
                if (_stateStack.Count > 0)
                {
                    _stateStack.Pop();
                }
            }
        }
    }
}