using Microsoft.Extensions.Logging;
using System;

namespace Loggo.Providers.Implementations
{
    public class LoggoOpenTelemetryProvider : ILoggerProvider
    {
        internal ILogger<OpenTelemetry.Logs.OpenTelemetryLoggerProvider> _provider;
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
