using Microsoft.Extensions.Logging;

namespace Loggo.Providers.Interfaces
{
    public interface ILogProvider
    {
        ILogProvider EnableConsole();
        ILogProvider EnableFile(string path);
        ILogProvider EnableRollingFile(string path, string maxFileSize, int maxFiles);
        ILogProvider EnableDatabase(string connectionString, string tableName);
        ILogProvider EnableElk(string connectionString, string indexName);
        ILogProvider DebugSeverity();
        ILogProvider InfoSeverity();
        ILogProvider WarnSeverity();
        ILogProvider ErrorSeverity();
        ILogProvider FatalSeverity();
        ILogProvider SetSeverity(LogLevel level);
    }
}
