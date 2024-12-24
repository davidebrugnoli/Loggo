using Loggo.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry.Logs;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;

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
    public sealed class OtelLokiLoggerProvider : OpenTelemetryLoggerProvider
    {
        public OtelLokiLoggerProvider() : base()
        {
        }
        public OtelLokiLoggerProvider(OpenTelemetryLoggerOptions options) : base(options)
        {
        }
        public OpenTelemetryLoggerProvider EnableGrafanaLoki(string baseUrl = null)
        {
            if (string.IsNullOrEmpty(baseUrl) == false)
            {
                _lokiBaseUrl = baseUrl;
            }
            return this;
        }
        public ILogger CreateLogger(string categoryName, string orgId)
        {
            return new OtelLokiLogger(categoryName, orgId, this);
        }
        public override async void Export(LogRecord logRecord)
        {
            if (string.IsNullOrEmpty(_lokiBaseUrl))
            {
                throw new InvalidOperationException("Loki base URL is not configured. Call EnableGrafanaLoki to configure it.");
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1_000_000; // Convert to nanoseconds


            var payload = new
            {
                streams = new[]
                {
                new
                {
                    stream = new { 
                        level = logRecord.LogLevel.ToString(), 
                        category =logRecord.CategoryName,
                        traceId = logRecord.EventId.Id.ToString()
                    },
                    values = new[]
                    {
                        new[] { timestamp.ToString(), logRecord.Message }
                    }
                }
            }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await ($"{_lokiBaseUrl}/loki/api/v1/push")
                .WithHeader("X-Scope-OrgID", logRecord.OrganizationId)
                .PostAsync(content);
        }
    }
    public class OpenTelemetryLoggerProvider : ILoggerProvider
    {
        protected readonly OpenTelemetryLoggerOptions _options;
        /// <summary>
        /// default http://localhost:3100
        /// </summary>
        protected string _lokiBaseUrl = "http://localhost:3100";
        protected readonly HttpClient _httpClient;

        public OpenTelemetryLoggerProvider()
        {
            if (_options == null)
            {
                //default options
                _options = new OpenTelemetryLoggerOptions();
            }
            _httpClient = new HttpClient();
        }
        public OpenTelemetryLoggerProvider(OpenTelemetryLoggerOptions options) : this()
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual ILogger CreateLogger(string categoryName)
        {
            return new OpenTelemetryLogger(categoryName, this);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public virtual void Export(LogRecord logRecord)
        {
            throw new NotImplementedException();
        }
    }

    public class LogRecord
    {
        [JsonIgnore]
        public string OrganizationId { get; set; }
        public string CategoryName { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public EventId EventId { get; set; }
    }
}