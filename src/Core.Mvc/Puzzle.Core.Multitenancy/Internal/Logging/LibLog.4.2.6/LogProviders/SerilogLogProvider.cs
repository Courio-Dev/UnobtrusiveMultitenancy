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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// SerilogLogProvider.
    /// </summary>
    [Multitenancy.ExcludeFromCodeCoverage]
    internal class SerilogLogProvider : LogProviderBase
    {
        private static bool providerIsAvailableOverride = true;
        private readonly Func<string, object> getLoggerByNameDelegate;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Serilog", Justification = "Pending")]
        public SerilogLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Serilog.Log not found");
            }

            getLoggerByNameDelegate = GetForContextMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return providerIsAvailableOverride; }
            set { providerIsAvailableOverride = value; }
        }

        public override Logger GetLogger(string name)
        {
            return new SerilogLogger(getLoggerByNameDelegate(name)).Log;
        }

        internal static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        protected override OpenNdc GetOpenNdcMethod()
        {
            return message => GetPushProperty()("NDC", message);
        }

        protected override OpenMdc GetOpenMdcMethod()
        {
            return (key, value) => GetPushProperty()(key, value);
        }

        private static Func<string, string, IDisposable> GetPushProperty()
        {
            Type ndcContextType = Type.GetType("Serilog.Context.LogContext, Serilog") ??
                                  Type.GetType("Serilog.Context.LogContext, Serilog.FullNetFx");

            MethodInfo pushPropertyMethod = ndcContextType.GetMethodPortable(
                "PushProperty",
                typeof(string),
                typeof(object),
                typeof(bool));

            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            ParameterExpression destructureObjectParam = Expression.Parameter(typeof(bool), "destructureObjects");
            MethodCallExpression pushPropertyMethodCall = Expression
                .Call(null, pushPropertyMethod, nameParam, valueParam, destructureObjectParam);
            Func<string, object, bool, IDisposable> pushProperty = Expression
                .Lambda<Func<string, object, bool, IDisposable>>(
                    pushPropertyMethodCall,
                    nameParam,
                    valueParam,
                    destructureObjectParam)
                .Compile();

            return (key, value) => pushProperty(key, value, false);
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("Serilog.Log, Serilog");
        }

        private static Func<string, object> GetForContextMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethodPortable("ForContext", typeof(string), typeof(object), typeof(bool));
            ParameterExpression propertyNameParam = Expression.Parameter(typeof(string), "propertyName");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            ParameterExpression destructureObjectsParam = Expression.Parameter(typeof(bool), "destructureObjects");
            MethodCallExpression methodCall = Expression.Call(
            null,
            method,
            new Expression[] { propertyNameParam, valueParam, destructureObjectsParam });
            Func<string, object, bool, object> func = Expression.Lambda<Func<string, object, bool, object>>(
                methodCall,
                propertyNameParam,
                valueParam,
                destructureObjectsParam)
                .Compile();
            return name => func("SourceContext", name, false);
        }

        internal class SerilogLogger
        {
            private static readonly object DebugLevel;
            private static readonly object ErrorLevel;
            private static readonly object FatalLevel;
            private static readonly object InformationLevel;
            private static readonly object VerboseLevel;
            private static readonly object WarningLevel;
            private static readonly Func<object, object, bool> IsEnabled;
            private static readonly Action<object, object, string, object[]> Write;
            private static readonly Action<object, object, Exception, string, object[]> WriteException;
            private readonly object logger;

            [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Pending")]
            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Pending")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ILogger", Justification = "Pending")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogEventLevel", Justification = "Pending")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Serilog", Justification = "Pending")]
            static SerilogLogger()
            {
                Type logEventLevelType = Type.GetType("Serilog.Events.LogEventLevel, Serilog");
                if (logEventLevelType == null)
                {
                    throw new InvalidOperationException("Type Serilog.Events.LogEventLevel was not found.");
                }

                DebugLevel = Enum.Parse(logEventLevelType, "Debug", false);
                ErrorLevel = Enum.Parse(logEventLevelType, "Error", false);
                FatalLevel = Enum.Parse(logEventLevelType, "Fatal", false);
                InformationLevel = Enum.Parse(logEventLevelType, "Information", false);
                VerboseLevel = Enum.Parse(logEventLevelType, "Verbose", false);
                WarningLevel = Enum.Parse(logEventLevelType, "Warning", false);

                // Func<object, object, bool> isEnabled = (logger, level) => { return ((SeriLog.ILogger)logger).IsEnabled(level); }
                Type loggerType = Type.GetType("Serilog.ILogger, Serilog");
                if (loggerType == null)
                {
                    throw new InvalidOperationException("Type Serilog.ILogger was not found.");
                }

                MethodInfo isEnabledMethodInfo = loggerType.GetMethodPortable("IsEnabled", logEventLevelType);
                ParameterExpression instanceParam = Expression.Parameter(typeof(object));
                UnaryExpression instanceCast = Expression.Convert(instanceParam, loggerType);
                ParameterExpression levelParam = Expression.Parameter(typeof(object));
                UnaryExpression levelCast = Expression.Convert(levelParam, logEventLevelType);
                MethodCallExpression isEnabledMethodCall = Expression.Call(instanceCast, isEnabledMethodInfo, levelCast);
                IsEnabled = Expression.Lambda<Func<object, object, bool>>(isEnabledMethodCall, instanceParam, levelParam).Compile();

                // Action<object, object, string> Write =
                // (logger, level, message, params) => { ((SeriLog.ILoggerILogger)logger).Write(level, message, params); }
                MethodInfo writeMethodInfo = loggerType.GetMethodPortable("Write", logEventLevelType, typeof(string), typeof(object[]));
                ParameterExpression messageParam = Expression.Parameter(typeof(string));
                ParameterExpression propertyValuesParam = Expression.Parameter(typeof(object[]));
                MethodCallExpression writeMethodExp = Expression.Call(
                    instanceCast,
                    writeMethodInfo,
                    levelCast,
                    messageParam,
                    propertyValuesParam);
                Expression<Action<object, object, string, object[]>> expression = Expression.Lambda<Action<object, object, string, object[]>>(
                    writeMethodExp,
                    instanceParam,
                    levelParam,
                    messageParam,
                    propertyValuesParam);
                Write = expression.Compile();

                // Action<object, object, string, Exception> WriteException =
                // (logger, level, exception, message) => { ((ILogger)logger).Write(level, exception, message, new object[]); }
                MethodInfo writeExceptionMethodInfo = loggerType.GetMethodPortable(
                    "Write",
                    logEventLevelType,
                    typeof(Exception),
                    typeof(string),
                    typeof(object[]));
                ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception));
                writeMethodExp = Expression.Call(
                    instanceCast,
                    writeExceptionMethodInfo,
                    levelCast,
                    exceptionParam,
                    messageParam,
                    propertyValuesParam);
                WriteException = Expression.Lambda<Action<object, object, Exception, string, object[]>>(
                    writeMethodExp,
                    instanceParam,
                    levelParam,
                    exceptionParam,
                    messageParam,
                    propertyValuesParam).Compile();
            }

            internal SerilogLogger(object logger)
            {
                this.logger = logger;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                object translatedLevel = TranslateLevel(logLevel);
                if (messageFunc == null)
                {
                    return IsEnabled(logger, translatedLevel);
                }

                if (!IsEnabled(logger, translatedLevel))
                {
                    return false;
                }

                if (exception != null)
                {
                    LogException(translatedLevel, messageFunc, exception, formatParameters);
                }
                else
                {
                    LogMessage(translatedLevel, messageFunc, formatParameters);
                }

                return true;
            }

            private static object TranslateLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Fatal:
                        return FatalLevel;

                    case LogLevel.Error:
                        return ErrorLevel;

                    case LogLevel.Warn:
                        return WarningLevel;

                    case LogLevel.Info:
                        return InformationLevel;

                    case LogLevel.Trace:
                        return VerboseLevel;

                    default:
                        return DebugLevel;
                }
            }

            private void LogMessage(object translatedLevel, Func<string> messageFunc, object[] formatParameters)
            {
                Write(logger, translatedLevel, messageFunc(), formatParameters);
            }

            private void LogException(object logLevel, Func<string> messageFunc, Exception exception, object[] formatParams)
            {
                WriteException(logger, logLevel, exception, messageFunc(), formatParams);
            }
        }
    }
}
