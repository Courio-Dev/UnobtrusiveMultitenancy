namespace PuzzleCMS.WebHost.Infrastructure.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Internal;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;
    using Logging = Puzzle.Core.Multitenancy.Internal.Logging;

    internal class AspNetCoreMultiTenantLogProvider : Logging.LibLog.ILogProvider
    {
        private readonly ILoggerFactory loggerFactory;

        public AspNetCoreMultiTenantLogProvider(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Logging.LibLog.ILog GetLogger(string name)
        {
            return new AspNetCoreMultiTenantLog(loggerFactory.CreateLogger(name));
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return null;
        }

        public IDisposable OpenNestedContext(string message)
        {
            return null;
        }

        Logger ILogProvider.GetLogger(string name) => throw new NotImplementedException();

        internal class AspNetCoreMultiTenantLog : ILog
        {
            private static readonly Func<object, Exception, string> MessageFormatterFunc = MessageFormatter;
            private static readonly object[] EmptyArgs = new object[0];

            private readonly ILogger targetLogger;

            public AspNetCoreMultiTenantLog(ILogger targetLogger)
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
