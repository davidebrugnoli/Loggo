using Loggo.Enums;
using Loggo.Loggers;
using Loggo.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loggo.Providers.Implementations
{
    public class LoggoOpenTelemetry : ILogProvider
    {
        internal ILogger<OpenTelemetryLogger> _logger;

        public ILogProvider DebugSeverity()
        {
            throw new NotImplementedException();
        }

        public ILogProvider EnableConsole()
        {
            throw new NotImplementedException();
        }

        public ILogProvider EnableDatabase(string connectionString, string tableName)
        {
            throw new NotImplementedException();
        }

        public ILogProvider EnableElk(string connectionString, string indexName)
        {
            throw new NotImplementedException();
        }

        public ILogProvider EnableFile(string path)
        {
            throw new NotImplementedException();
        }

        public ILogProvider EnableRollingFile(string path, string maxFileSize, int maxFiles)
        {
            throw new NotImplementedException();
        }

        public ILogProvider ErrorSeverity()
        {
            throw new NotImplementedException();
        }

        public ILogProvider FatalSeverity()
        {
            throw new NotImplementedException();
        }

        public ILogProvider InfoSeverity()
        {
            throw new NotImplementedException();
        }

        public ILogProvider SetSeverity(eLogSeverities level)
        {
            throw new NotImplementedException();
        }

        public ILogProvider WarnSeverity()
        {
            throw new NotImplementedException();
        }
    }
}
