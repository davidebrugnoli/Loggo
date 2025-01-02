namespace Loggo
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public class LoggoFactory : ILoggerFactory
    {
        private readonly List<ILoggerProvider> _providers = new List<ILoggerProvider>();
        private bool _disposed = false;

        public void AddProvider(ILoggerProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _providers.Add(provider);
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentNullException(nameof(categoryName));
            }

            var loggers = new List<ILogger>();
            foreach (var provider in _providers)
            {
                loggers.Add(provider.CreateLogger(categoryName));
            }

            return new CombinedLogger(loggers);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var provider in _providers)
                {
                    (provider as IDisposable)?.Dispose();
                }

                _disposed = true;
            }
        }
    }

    public class CombinedLogger : ILogger
    {
        private readonly IEnumerable<ILogger> _loggers;

        public CombinedLogger(IEnumerable<ILogger> loggers)
        {
            _loggers = loggers;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var disposables = new List<IDisposable>();

            foreach (var logger in _loggers)
            {
                disposables.Add(logger.BeginScope(state));
            }

            return new CombinedDisposable(disposables);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            foreach (var logger in _loggers)
            {
                if (logger.IsEnabled(logLevel))
                {
                    return true;
                }
            }

            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            foreach (var logger in _loggers)
            {
                if (logger.IsEnabled(logLevel))
                    logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }

    public class CombinedDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public CombinedDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
