namespace DHI.Services.Test.Notifications
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class TestLoggerMicrosoft : ILogger
    {
        private readonly ITestOutputHelper _output;
        private readonly bool _verbose;

        private readonly Stack<string> _state = new Stack<string>();

        public TestLoggerMicrosoft(ITestOutputHelper output, bool verbose = true)
        {
            _output = output;
            _verbose = verbose;
        }

        public List<string> Logs { get; set; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            _state.Push(JsonSerializer.Serialize(state));
            _output.WriteLine($"<{_state.Peek()}>");
            return new TestLoggerState(_state, _output);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _verbose || logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message;
            if (exception == null)
            {
                message = formatter(state, null);
            }
            else
            {
                message = $"{exception.Message}. {formatter(state, exception)}";
            }

            _output?.WriteLine(IndentState(message));
            Logs.Add(IndentState(message));
        }

        private string IndentState(string message)
        {
            return message.PadLeft(message.Length + _state.Count, ' ');
        }

        private class TestLoggerState : IDisposable
        {
            private readonly Stack<string> _state;
            private readonly ITestOutputHelper _outputHelper;

            public TestLoggerState(Stack<string> stateStack, ITestOutputHelper outputHelper)
            {
                _state = stateStack;
                _outputHelper = outputHelper;
            }

            public void Dispose()
            {
                _outputHelper.WriteLine($"</{_state.Pop()}>");
            }
        }
    }
}
