// ===============================================================================
// LibLog
//
// https://github.com/damianh/LibLog
// ===============================================================================
// Copyright Â© 2011-2015 Damian Hickey.  All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ===============================================================================

// ReSharper disable PossibleNullReferenceException
// Define LIBLOG_PORTABLE conditional compilation symbol for PCL compatibility
//
// Define LIBLOG_PUBLIC to enable ability to GET a logger (LogProvider.For<>() etc) from outside this library. NOTE:
// this can have unintended consequences of consumers of your library using your library to resolve a logger. If the
// reason is because you want to open this functionality to other projects within your solution,
// consider [InternalsVisibleTo] instead.
//
// Define LIBLOG_PROVIDERS_ONLY if your library provides its own logging API and you just want to use the
// LibLog providers internally to provide built in support for popular logging frameworks.
namespace Puzzle.Core.Multitenancy.Internal.Logging.LibLog.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// EntLibLogProvider
    /// </summary>
    [Multitenancy.ExcludeFromCodeCoverage]
    internal class EntLibLogProvider : LogProviderBase
    {
        private const string TypeTemplate = "Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging";
        private static readonly Type LogEntryType;
        private static readonly Type LoggerType;
        private static readonly Type TraceEventTypeType;
        private static readonly Action<string, string, int> WriteLogEntry;
        private static readonly Func<string, int, bool> ShouldLogEntry;
        private static bool providerIsAvailableOverride = true;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Pending")]
        static EntLibLogProvider()
        {
            LogEntryType = Type.GetType(string.Format(CultureInfo.InvariantCulture, TypeTemplate, "LogEntry"));
            LoggerType = Type.GetType(string.Format(CultureInfo.InvariantCulture, TypeTemplate, "Logger"));
            TraceEventTypeType = TraceEventTypeValues.Type;
            if (LogEntryType == null
                 || TraceEventTypeType == null
                 || LoggerType == null)
            {
                return;
            }

            WriteLogEntry = GetWriteLogEntry();
            ShouldLogEntry = GetShouldLogEntry();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EnterpriseLibrary", Justification = "Pending")]
        public EntLibLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Microsoft.Practices.EnterpriseLibrary.Logging.Logger not found");
            }
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return providerIsAvailableOverride; }
            set { providerIsAvailableOverride = value; }
        }

        public override Logger GetLogger(string name)
        {
            return new EntLibLogger(name, WriteLogEntry, ShouldLogEntry).Log;
        }

        internal static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride
                 && TraceEventTypeType != null
                 && LogEntryType != null;
        }

        private static Action<string, string, int> GetWriteLogEntry()
        {
            // new LogEntry(...)
            ParameterExpression logNameParameter = Expression.Parameter(typeof(string), "logName");
            ParameterExpression messageParameter = Expression.Parameter(typeof(string), "message");
            ParameterExpression severityParameter = Expression.Parameter(typeof(int), "severity");

            MemberInitExpression memberInit = GetWriteLogExpression(
                messageParameter,
                Expression.Convert(severityParameter, TraceEventTypeType),
                logNameParameter);

            // Logger.Write(new LogEntry(....));
            MethodInfo writeLogEntryMethod = LoggerType.GetMethodPortable("Write", LogEntryType);
            MethodCallExpression writeLogEntryExpression = Expression.Call(writeLogEntryMethod, memberInit);

            return Expression.Lambda<Action<string, string, int>>(
                writeLogEntryExpression,
                logNameParameter,
                messageParameter,
                severityParameter).Compile();
        }

        private static Func<string, int, bool> GetShouldLogEntry()
        {
            // new LogEntry(...)
            ParameterExpression logNameParameter = Expression.Parameter(typeof(string), "logName");
            ParameterExpression severityParameter = Expression.Parameter(typeof(int), "severity");

            MemberInitExpression memberInit = GetWriteLogExpression(
                Expression.Constant("***dummy***"),
                Expression.Convert(severityParameter, TraceEventTypeType),
                logNameParameter);

            // Logger.Write(new LogEntry(....));
            MethodInfo writeLogEntryMethod = LoggerType.GetMethodPortable("ShouldLog", LogEntryType);
            MethodCallExpression writeLogEntryExpression = Expression.Call(writeLogEntryMethod, memberInit);

            return Expression.Lambda<Func<string, int, bool>>(
                writeLogEntryExpression,
                logNameParameter,
                severityParameter).Compile();
        }

        private static MemberInitExpression GetWriteLogExpression(
            Expression message,
            Expression severityParameter,
            ParameterExpression logNameParameter)
        {
            Type entryType = LogEntryType;
            MemberInitExpression memberInit = Expression.MemberInit(
                Expression.New(entryType),
                Expression.Bind(entryType.GetPropertyPortable("Message"), message),
                Expression.Bind(entryType.GetPropertyPortable("Severity"), severityParameter),
                Expression.Bind(
                    entryType.GetPropertyPortable("TimeStamp"),
                    Expression.Property(null, typeof(DateTime).GetPropertyPortable("UtcNow"))),
                Expression.Bind(
                    entryType.GetPropertyPortable("Categories"),
                    Expression.ListInit(
                        Expression.New(typeof(List<string>)),
                        typeof(List<string>).GetMethodPortable("Add", typeof(string)),
                        logNameParameter)));
            return memberInit;
        }

        internal class EntLibLogger
        {
            private readonly string loggerName;
            private readonly Action<string, string, int> writeLog;
            private readonly Func<string, int, bool> shouldLog;

            internal EntLibLogger(string loggerName, Action<string, string, int> writeLog, Func<string, int, bool> shouldLog)
            {
                this.loggerName = loggerName;
                this.writeLog = writeLog;
                this.shouldLog = shouldLog;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                int severity = MapSeverity(logLevel);
                if (messageFunc == null)
                {
                    return shouldLog(loggerName, severity);
                }

                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);
                if (exception != null)
                {
                    return LogException(logLevel, messageFunc, exception);
                }

                writeLog(loggerName, messageFunc(), severity);
                return true;
            }

            public bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                int severity = MapSeverity(logLevel);
                string message = messageFunc() + Environment.NewLine + exception;
                writeLog(loggerName, message, severity);
                return true;
            }

            private static int MapSeverity(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Fatal:
                        return TraceEventTypeValues.Critical;
                    case LogLevel.Error:
                        return TraceEventTypeValues.Error;
                    case LogLevel.Warn:
                        return TraceEventTypeValues.Warning;
                    case LogLevel.Info:
                        return TraceEventTypeValues.Information;
                    default:
                        return TraceEventTypeValues.Verbose;
                }
            }
        }
    }
}
