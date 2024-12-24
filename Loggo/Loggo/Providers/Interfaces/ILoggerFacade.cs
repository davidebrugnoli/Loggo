using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loggo.Providers.Interfaces
{
    public interface ILoggerFacade
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(Exception ex);
        void Fatal(string message);
        // Async methods 
        Task DebugAsync(string message);
        Task InfoAsync(string message);
        Task WarnAsync(string message);
        Task ErrorAsync(string message);
        Task ErrorAsync(Exception ex);
        Task FatalAsync(string message);
    }
}
