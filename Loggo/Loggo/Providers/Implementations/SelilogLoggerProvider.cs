using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;

namespace Loggo.Providers.Implementations
{
    public class SelilogLoggerProvider : ILoggerProvider
    {
        private bool _enableConsole;
        private string _logFolder;
        private RollingInterval _rollingInterval;
        private long _maxFileSizeBytes = 10 * 1024 * 1024; // 10 MB;
        private Serilog.Core.Logger _logger;
        private Serilog.Events.LogEventLevel _logLevel;

        public SelilogLoggerProvider(bool enableConsole = true,
                                     string logFolder = null,
                                     RollingInterval rollingInterval = RollingInterval.Day,
                                     long maxFileSizeBytes = 10 * 1024 * 1024,
                                     Serilog.Events.LogEventLevel logLevel = Serilog.Events.LogEventLevel.Information)
        {
            _enableConsole = enableConsole;
            _logFolder = logFolder;
            _rollingInterval = rollingInterval;
            _maxFileSizeBytes = maxFileSizeBytes;
            _logLevel = logLevel;
        }

        private void ConfigureLogger()
        {
            var serilogConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(_logLevel);

            if (_enableConsole)
            {
                serilogConfiguration.WriteTo.Console();
            }

            if (!string.IsNullOrWhiteSpace(_logFolder))
            {
                Directory.CreateDirectory(_logFolder); // Ensure the directory exists
                var logFilePath = Path.Combine(_logFolder, $"log-{DateTime.Now:yyyyMMdd}.log");
                serilogConfiguration.WriteTo.File(logFilePath,
                    rollingInterval: _rollingInterval,
                    fileSizeLimitBytes: _maxFileSizeBytes,
                    rollOnFileSizeLimit: true);
            }

            _logger = serilogConfiguration.CreateLogger();
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            // Use SerilogLoggerFactory to create a Microsoft.Extensions.Logging.ILogger
            ConfigureLogger();
            return new SerilogLoggerFactory(_logger).CreateLogger(categoryName);
        }

        public void Dispose()
        {
            // Release Serilog resources
            _logger?.Dispose();
            Log.CloseAndFlush();
        }

        public SelilogLoggerProvider EnableConsole(bool yes = true)
        {
            _enableConsole = yes;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rollingInterval"></param>
        /// <param name="maxFileSizeBytes">The max size is 1GB (default value is 10MB)</param>
        /// <returns></returns>
        public SelilogLoggerProvider ToFile(string path, RollingInterval rollingInterval = RollingInterval.Infinite, long maxFileSizeBytes = (10 * 1024 * 1024))
        {
            _logFolder = path;
            _rollingInterval = rollingInterval;
            _maxFileSizeBytes = maxFileSizeBytes;
            return this;
        }

        public SelilogLoggerProvider SetLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logLevel = Serilog.Events.LogEventLevel.Verbose;
                    break;
                case LogLevel.Debug:
                    _logLevel = Serilog.Events.LogEventLevel.Debug;
                    break;
                case LogLevel.Information:
                    _logLevel = Serilog.Events.LogEventLevel.Information;
                    break;
                case LogLevel.Warning:
                    _logLevel = Serilog.Events.LogEventLevel.Warning;
                    break;
                case LogLevel.Error:
                    _logLevel = Serilog.Events.LogEventLevel.Error;
                    break;
                case LogLevel.Critical:
                    _logLevel = Serilog.Events.LogEventLevel.Fatal;
                    break;
                case LogLevel.None:
                    _logLevel = Serilog.Events.LogEventLevel.Debug;
                    break;
                default:
                    break;
            }
            return this;
        }
    }
}