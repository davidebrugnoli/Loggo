using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using Flurl.Http;
using Loggo.Models;
using Loggo.Loggers;
using OpenTelemetry.Logs;

namespace Loggo.Providers.Implementations
{
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
        /// <summary>
        /// forse se ne deve occupare il logger dell'export?
        /// </summary>
        /// <param name="logRecord"></param>
        public override async void Export(Models.LogRecord logRecord)
        {
            try
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
                await $"{_lokiBaseUrl}/loki/api/v1/push"
                    .WithHeader("X-Scope-OrgID", logRecord.OrganizationId)
                    .PostAsync(content);
            }
            catch (FlurlHttpException flurlEx)
            {
                Console.WriteLine(flurlEx.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}