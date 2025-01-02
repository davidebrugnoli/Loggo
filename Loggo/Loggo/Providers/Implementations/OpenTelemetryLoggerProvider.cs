using Loggo.Loggers;
using Loggo.Models;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using System;
using System.Net.Http;

namespace Loggo.Providers.Implementations
{
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

        public virtual void Export(Models.LogRecord logRecord)
        {
            throw new NotImplementedException();
        }
    }
}