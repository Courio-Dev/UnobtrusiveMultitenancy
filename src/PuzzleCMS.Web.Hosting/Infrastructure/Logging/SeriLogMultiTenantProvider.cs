namespace PuzzleCMS.WebHost.Infrastructure.Logging
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Internal;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using Serilog;
    using Serilog.Context;
    using Serilog.Extensions.Logging;
    using Logging = PuzzleCMS.Core.Multitenancy.Internal.Logging;

    internal class SeriLogProvider : Logging.LibLog.ILogProvider
    {
        private readonly SerilogLoggerProvider loggerProvider;

        public SeriLogProvider(SerilogLoggerProvider loggerProvider)
        {
            this.loggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
        }

        public Logging.LibLog.ILog GetLogger(string name) => new SeriLogCoreMultiTenantLog( loggerProvider.CreateLogger(name));
        Logger ILogProvider.GetLogger(string name) => new SeriLogCoreMultiTenantLog(loggerProvider.CreateLogger(name)).Log;


        public IDisposable OpenMappedContext(string key, string value) => LogContext.PushProperty(key, value);
        public IDisposable OpenNestedContext(string message) => LogContext.PushProperty("NDC", message);


        internal class SeriLogCoreMultiTenantLog : ILog
        {
            private static readonly Func<object, Exception, string> MessageFormatterFunc = MessageFormatter;
            private static readonly object[] EmptyArgs = new object[0];

            private readonly Microsoft.Extensions.Logging.ILogger targetLogger;

            public SeriLogCoreMultiTenantLog(Microsoft.Extensions.Logging.ILogger targetLogger)
            {
                this.targetLogger = targetLogger ?? throw new ArgumentNullException(nameof(targetLogger));
            }

            public bool Log(Logging.LibLog.LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
            {
                
                Microsoft.Extensions.Logging.LogLevel targetLogLevel = ToTargetLogLevel(logLevel);

                // When messageFunc is null
                // just determines is logging enabled.
                if (messageFunc == null)
                {
                    return targetLogger.IsEnabled(targetLogLevel);
                }

                targetLogger.Log(targetLogLevel, 0, CreateStateObject(messageFunc()), exception, MessageFormatterFunc);
                return true;
            }

            public bool Log(Logging.LibLog.LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
            {
                Microsoft.Extensions.Logging.LogLevel targetLogLevel = ToTargetLogLevel(logLevel);

                // When messageFunc is null
                // just determines is logging enabled.
                if (messageFunc == null)
                {
                    return targetLogger.IsEnabled(targetLogLevel);
                }

                targetLogger.Log(targetLogLevel, 0, CreateStateObject(messageFunc(), formatParameters), exception, MessageFormatterFunc);
                return true;
            }

            private static Microsoft.Extensions.Logging.LogLevel ToTargetLogLevel(Logging.LibLog.LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case Logging.LibLog.LogLevel.Trace:
                        return Microsoft.Extensions.Logging.LogLevel.Trace;

                    case Logging.LibLog.LogLevel.Debug:
                        return Microsoft.Extensions.Logging.LogLevel.Debug;

                    case Logging.LibLog.LogLevel.Info:
                        return Microsoft.Extensions.Logging.LogLevel.Information;

                    case Logging.LibLog.LogLevel.Warn:
                        return Microsoft.Extensions.Logging.LogLevel.Warning;

                    case Logging.LibLog.LogLevel.Error:
                        return Microsoft.Extensions.Logging.LogLevel.Error;

                    case Logging.LibLog.LogLevel.Fatal:
                        return Microsoft.Extensions.Logging.LogLevel.Critical;
                }

                return Microsoft.Extensions.Logging.LogLevel.None;
            }

            private static object CreateStateObject(string message, params object[] values)
            {
                return new FormattedLogValues(message, values ?? EmptyArgs);
            }

            private static string MessageFormatter(object state, Exception exception)
            {
                return state.ToString();
            }
        }
    }
}
