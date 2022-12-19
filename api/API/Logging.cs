using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Replicate;
using Replicate.MetaData.Policy;
using Replicate.Serialization;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace API {
    #region Boiler Plate Garbage
    [ProviderAlias("APILogging")]
    public class LoggingProvider : ILoggerProvider {
        IServiceProvider Services;
        public LoggingProvider(IServiceProvider services) {
            Services = services;
        }

        public ILogger CreateLogger(string categoryName) => new Logging(Services);

        public void Dispose() { }
    }
    [ConfigOptions(Section = "APILogging")]
    public class LoggingOptions {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public bool UseJson { get; set; } = false;
        public bool ThrowOnError { get; set; } = false;
    }
    [ReplicateType]
    public struct LogMessage {
        [SkipNull]
        public string RequestId;
        [SkipNull]
        public string LogLevel;
        [SkipNull]
        public string Message;
        [SkipNull]
        public string Exception;
    }
    public class Logging : ILogger {
        public static void Configure(ILoggingBuilder logging) {
            logging.ClearProviders();
            logging.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, LoggingProvider>());
            LoggerProviderOptions.RegisterProviderOptions
                <LoggingOptions, LoggingProvider>(logging.Services);
            logging.AddFilter("Microsoft.EntityFrameworkCore", (_) => false);
        }
        private static Dictionary<int, int> expectedEvents = new Dictionary<int, int>();
        public static void Expect(EventId eventId, int count = 1) {
            Debug.Assert(count >= 1);
            expectedEvents.TryGetValue(eventId.Id, out int curCount);
            expectedEvents[eventId.Id] = curCount + count;
        }
        private LoggingOptions Config;
        private IHttpContextAccessor ContextAccessor;
        private IReplicateSerializer Serializer;
        public Logging(IServiceProvider services) {
            Config = services.GetRequiredService<IOptions<LoggingOptions>>().Value;
            ContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            Serializer = services.GetRequiredService<IReplicateSerializer>();
        }
        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= Config.LogLevel;
        private static bool RemoveExpectation(EventId eventId) {
#if DEBUG
            if (expectedEvents.ContainsKey(eventId.Id)) {
                if (--expectedEvents[eventId.Id] <= 0) expectedEvents.Remove(eventId.Id);
                return true;
            }
            return false;
#else
            return false;
#endif
        }

        #endregion
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            string messageString = null;
            if (Config.UseJson) {
                LogMessage message = new LogMessage() {
                    Exception = exception?.ToString(),
                    LogLevel = logLevel.ToString(),
                    Message = formatter(state, exception),
                    RequestId = ContextAccessor.HttpContext?.TraceIdentifier
                };
                messageString = $"{DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff")} {Serializer.SerializeString(message)}";
            } else {
                messageString = $"{DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff")} {logLevel.ToString()[0]} {formatter(state, exception)}";
                if (exception != null)
                    messageString += Environment.NewLine + exception.ToString();
            }
            bool expected = RemoveExpectation(eventId);
            if (logLevel >= LogLevel.Error && Config.ThrowOnError && !expected)
                throw new Exception(messageString);
            Console.WriteLine(messageString);
        }
    }
}
