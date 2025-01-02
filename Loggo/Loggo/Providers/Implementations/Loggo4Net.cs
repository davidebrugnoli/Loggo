using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Loggo.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Loggo.Providers.Implementations
{
    /// <summary>
    /// Loggo implementation for log4net
    /// </summary>
    public class Loggo4Net : ILogProvider
    {
        ILog _logger;
        string _name;
        public Loggo4Net()
        {
            
        }
        public Loggo4Net(string name) : this()
        {
            this._name = name;
        }
        public Loggo4Net(ILog logger, string name) : this(name)
        {
            _logger = logger;
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public async Task DebugAsync(string message)
        {
            _logger.Debug(message);
        }

        public ILogProvider DebugSeverity()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);
            hierarchy.Root.Level = log4net.Core.Level.Debug;
            return this;
        }

        public ILogProvider EnableConsole()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);
            ConsoleAppender ca = new ConsoleAppender();
            hierarchy.Root.AddAppender(ca);
            return this;
        }

        public ILogProvider EnableDatabase(string connectionString, string tableName)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);
            AdoNetAppender adoNetAppender = new AdoNetAppender();
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
        public virtual ILogProvider EnableRollingFile(string path, string maxFileSize, int maxFiles)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            if(hierarchy.Root.Appenders.Any(x => x.Name == "RollingFileAppender") == false)
            {
                RollingFileAppender roller = new RollingFileAppender();
                roller.Name = "RollingFileAppender";
                roller.AppendToFile = false;
                roller.File = path;
                roller.Layout = patternLayout;
                roller.MaxSizeRollBackups = maxFiles;
                roller.MaximumFileSize = maxFileSize;
                roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                roller.StaticLogFileName = true;
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);
            }

            return this;
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public async Task ErrorAsync(string message)
        {
            _logger.Error(message);
        }

        public async Task ErrorAsync(Exception ex)
        {
            _logger.Error(ex);
        }

        public ILogProvider ErrorSeverity()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);
            hierarchy.Root.Level = log4net.Core.Level.Error;
            return this;
        }

        public void Fatal(string message)
        {
            throw new NotImplementedException();
        }

        public Task FatalAsync(string message)
        {
            throw new NotImplementedException();
        }

        public ILogProvider FatalSeverity()
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public Task InfoAsync(string message)
        {
            throw new NotImplementedException();
        }

        public ILogProvider InfoSeverity()
        {
            throw new NotImplementedException();
        }

        public ILogProvider SetSeverity(LogLevel level)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);
            log4net.Core.Level level4net = log4net.Core.Level.All;
            switch (level)
            {
                case LogLevel.Debug:
                    level4net = log4net.Core.Level.Debug;
                    break;
                case LogLevel.Information:
                    level4net = log4net.Core.Level.Info;
                    break;
                case LogLevel.Warning:
                    level4net = log4net.Core.Level.Warn;
                    break;
                case LogLevel.Error:
                    level4net = log4net.Core.Level.Error;
                    break;
                case LogLevel.Critical:
                    level4net = log4net.Core.Level.Critical;
                    break;
                default:
                    level4net = log4net.Core.Level.All;
                    break;
            }
            hierarchy.Root.Level = level4net;
            return this;
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public async Task WarnAsync(string message)
        {
            _logger.Warn(message);
        }

        public ILogProvider WarnSeverity()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(_name);
            hierarchy.Root.Level = log4net.Core.Level.Warn;
            return this;
        }
    }
}
