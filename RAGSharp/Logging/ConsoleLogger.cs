using Microsoft.Extensions.Logging;
using System;

namespace RAGSharp.Logging
{
    /// <summary>
    /// Simple console logger with level filtering.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        private readonly string _category;
        private readonly LogLevel _minLevel;

        public ConsoleLogger(string category = "Default", LogLevel minLevel = LogLevel.Information)
        {
            _category = category;
            _minLevel = minLevel;
        }

        public IDisposable BeginScope<TState>(TState state) => DummyScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);

            Console.WriteLine("[" + logLevel + "] [" + _category + "] " + message);

            if (exception != null)
                Console.WriteLine("Exception: " + exception);
        }

        private sealed class DummyScope : IDisposable
        {
            public static readonly DummyScope Instance = new DummyScope();
            public void Dispose() { }
        }
    }
}
