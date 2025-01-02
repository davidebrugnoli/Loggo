using Loggo.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Loggo.Models;
using Loggo.Providers.Implementations;

namespace Loggo.Loggers
{
    public class OtelLokiLogger : OpenTelemetryLogger
    {
        protected string _orgId = "BestPlaceToWork!";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="provider"></param>
        /// <param name="orgId">Organization ID: every time you will Log the X-Scope-OrgID will be set with the <paramref name="orgId"/> value.</param>
        public OtelLokiLogger(string categoryName, string orgId, OpenTelemetryLoggerProvider provider) : base(categoryName, provider)
        {
            this._orgId = orgId;
        }
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            // Utilizza OpenTelemetry per registrare il log
            var logRecord = new LogRecord
            {
                OrganizationId = this._orgId,
                CategoryName = base._categoryName,
                LogLevel = logLevel,
                Message = message,
                Exception = exception,
                EventId = eventId
            };

            _provider.Export(logRecord);
        }
    }
    public class OpenTelemetryLogger : ILogger, ILoggerFacade
    {
        protected readonly string _categoryName;
        protected readonly OpenTelemetryLoggerProvider _provider;

        public OpenTelemetryLogger(string categoryName, OpenTelemetryLoggerProvider provider)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Implementa un contesto di log se necessario
            return null;
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, new EventId(), message, null, (s, e) => s);
        }

        public Task DebugAsync(string message)
        {
            Debug(message);
            return Task.CompletedTask;
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, new EventId(), message, null, (s, e) => s);
        }

        public void Error(Exception ex)
        {
            Log(LogLevel.Error, new EventId(), ex.Message, ex, (s, e) => s);
        }

        public Task ErrorAsync(string message)
        {
            Error(message);
            return Task.CompletedTask;
        }

        public Task ErrorAsync(Exception ex)
        {
            Error(ex);
            return Task.CompletedTask;
        }

        public void Fatal(string message)
        {
            Log(LogLevel.Critical, new EventId(), message, null, (s, e) => s);
        }

        public Task FatalAsync(string message)
        {
            Fatal(message);
            return Task.CompletedTask;
        }

        public void Info(string message)
        {
            Log(LogLevel.Information, new EventId(), message, null, (s, e) => s);
        }

        public Task InfoAsync(string message)
        {
            Info(message);
            return Task.CompletedTask;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Abilita tutti i livelli di log per questo esempio
            return logLevel != LogLevel.None;
        }

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            // Utilizza OpenTelemetry per registrare il log
            var logRecord = new LogRecord
            {
                CategoryName = _categoryName,
                LogLevel = logLevel,
                Message = message,
                Exception = exception,
                EventId = eventId
            };

            _provider.Export(logRecord);
        }

        public void Warn(string message)
        {
            Log(LogLevel.Warning, new EventId(), message, null, (s, e) => s);
        }

        public Task WarnAsync(string message)
        {
            Warn(message);
            return Task.CompletedTask;
        }
    }
}