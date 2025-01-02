using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Loggo.Models
{
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