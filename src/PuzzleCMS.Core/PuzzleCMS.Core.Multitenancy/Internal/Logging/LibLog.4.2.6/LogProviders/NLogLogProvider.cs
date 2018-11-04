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
namespace PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// NLogLogProvider.
    /// </summary>
    [Multitenancy.ExcludeFromCodeCoverage]
    internal class NLogLogProvider : LogProviderBase
    {
        private readonly Func<string, object> getLoggerByNameDelegate;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogManager", Justification = "Pending")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NLog", Justification = "Pending")]
        public NLogLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("NLog.LogManager not found");
            }

            getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride { get; set; } = true;

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        public override Logger GetLogger(string name)
        {
            return new NLogLogger(getLoggerByNameDelegate(name)).Log;
        }

        protected override OpenNdc GetOpenNdcMethod()
        {
            Type ndcContextType = Type.GetType("NLog.NestedDiagnosticsContext, NLog");
            MethodInfo pushMethod = ndcContextType.GetMethodPortable("Push", typeof(string));
            ParameterExpression messageParam = Expression.Parameter(typeof(string), "message");
            MethodCallExpression pushMethodCall = Expression.Call(null, pushMethod, messageParam);
            return Expression.Lambda<OpenNdc>(pushMethodCall, messageParam).Compile();
        }

        protected override OpenMdc GetOpenMdcMethod()
        {
            Type mdcContextType = Type.GetType("NLog.MappedDiagnosticsContext, NLog");

            MethodInfo setMethod = mdcContextType.GetMethodPortable("Set", typeof(string), typeof(string));
            MethodInfo removeMethod = mdcContextType.GetMethodPortable("Remove", typeof(string));
            ParameterExpression keyParam = Expression.Parameter(typeof(string), "key");
            ParameterExpression valueParam = Expression.Parameter(typeof(string), "value");

            MethodCallExpression setMethodCall = Expression.Call(null, setMethod, keyParam, valueParam);
            MethodCallExpression removeMethodCall = Expression.Call(null, removeMethod, keyParam);

            Action<string, string> set = Expression
                .Lambda<Action<string, string>>(setMethodCall, keyParam, valueParam)
                .Compile();
            Action<string> remove = Expression
                .Lambda<Action<string>>(removeMethodCall, keyParam)
                .Compile();

            return (key, value) =>
            {
                set(key, value);
                return new DisposableAction(() => remove(key));
            };
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("NLog.LogManager, NLog");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethodPortable("GetLogger", typeof(string));
            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, nameParam);
            return Expression.Lambda<Func<string, object>>(methodCall, nameParam).Compile();
        }

        internal class NLogLogger
        {
            private static readonly object LevelTrace;
            private static readonly object LevelDebug;
            private static readonly object LevelInfo;
            private static readonly object LevelWarn;
            private static readonly object LevelError;
            private static readonly object LevelFatal;

            private static readonly Func<string, object, string, Exception, object> LogEventInfoFact;

            private readonly dynamic logger;

#pragma warning disable S3963 // "static" fields should be initialized inline
            static NLogLogger()
            {
                try
                {
                    Type logEventLevelType = Type.GetType("NLog.LogLevel, NLog");
                    if (logEventLevelType == null)
                    {
#pragma warning disable S3877 
                        throw new InvalidOperationException("Type NLog.LogLevel was not found.");
#pragma warning restore S3877
                    }

                    List<FieldInfo> levelFields = logEventLevelType.GetFieldsPortable().ToList();
                    LevelTrace = levelFields.First(x => x.Name == "Trace").GetValue(null);
                    LevelDebug = levelFields.First(x => x.Name == "Debug").GetValue(null);
                    LevelInfo = levelFields.First(x => x.Name == "Info").GetValue(null);
                    LevelWarn = levelFields.First(x => x.Name == "Warn").GetValue(null);
                    LevelError = levelFields.First(x => x.Name == "Error").GetValue(null);
                    LevelFatal = levelFields.First(x => x.Name == "Fatal").GetValue(null);

                    Type logEventInfoType = Type.GetType("NLog.LogEventInfo, NLog");
                    if (logEventInfoType == null)
                    {
                        throw new InvalidOperationException("Type NLog.LogEventInfo was not found.");
                    }

                    MethodInfo createLogEventInfoMethodInfo = logEventInfoType.GetMethodPortable(
                        "Create",
                        logEventLevelType,
                        typeof(string),
                        typeof(Exception),
                        typeof(IFormatProvider),
                        typeof(string),
                        typeof(object[]));
                    ParameterExpression loggerNameParam = Expression.Parameter(typeof(string));
                    ParameterExpression levelParam = Expression.Parameter(typeof(object));
                    ParameterExpression messageParam = Expression.Parameter(typeof(string));
                    ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception));
                    UnaryExpression levelCast = Expression.Convert(levelParam, logEventLevelType);
                    MethodCallExpression createLogEventInfoMethodCall = Expression.Call(
                        null,
                        createLogEventInfoMethodInfo,
                        levelCast,
                        loggerNameParam,
                        exceptionParam,
                        Expression.Constant(
                            null,
                            typeof(IFormatProvider)),
                        messageParam,
                        Expression.Constant(null, typeof(object[])));
                    LogEventInfoFact = Expression.Lambda<Func<string, object, string, Exception, object>>(
                        createLogEventInfoMethodCall,
                        loggerNameParam,
                        levelParam,
                        messageParam,
                        exceptionParam).Compile();
                }
                catch(Exception ex)
                {
                    Console.Write(ex);
                }
            }
#pragma warning restore S3963 // "static" fields should be initialized inline

            internal NLogLogger(dynamic logger)
            {
                this.logger = logger;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Pending")]
            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                if (messageFunc == null)
                {
                    return IsLogLevelEnable(logLevel);
                }

                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);

                if (LogEventInfoFact != null)
                {
                    if (IsLogLevelEnable(logLevel))
                    {
                        object nlogLevel = TranslateLevel(logLevel);
                        Type s_callerStackBoundaryType;
#if !LIBLOG_PORTABLE
                        StackTrace stack = new StackTrace();
                        Type thisType = GetType();
                        Type knownType0 = typeof(LoggerExecutionWrapper);
                        Type knownType1 = typeof(LogExtensions);
                        //Maybe inline, so we may can't found any LibLog classes in stack
                        s_callerStackBoundaryType = null;
                        for (int i = 0; i < stack.FrameCount; i++)
                        {
                            Type declaringType = stack.GetFrame(i).GetMethod().DeclaringType;
                            if (!IsInTypeHierarchy(thisType, declaringType) &&
                                !IsInTypeHierarchy(knownType0, declaringType) &&
                                !IsInTypeHierarchy(knownType1, declaringType))
                            {
                                if (i > 1)
                                    s_callerStackBoundaryType = stack.GetFrame(i - 1).GetMethod().DeclaringType;
                                break;
                            }
                        }
#else
                        s_callerStackBoundaryType = null;
#endif
#pragma warning disable S2583 // Conditionally executed blocks should be reachable
                        if (s_callerStackBoundaryType != null)
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                        {
                            logger.Log(s_callerStackBoundaryType, LogEventInfoFact(logger.Name, nlogLevel, messageFunc(), exception));
                        }
                        else
                        {
                            logger.Log(LogEventInfoFact(logger.Name, nlogLevel, messageFunc(), exception));
                        }

                        return true;
                    }

                    return false;
                }

                if (exception != null)
                {
                    return LogException(logLevel, messageFunc, exception);
                }

                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug(messageFunc());
                            return true;
                        }

                        break;

                    case LogLevel.Info:
                        if (logger.IsInfoEnabled)
                        {
                            logger.Info(messageFunc());
                            return true;
                        }

                        break;

                    case LogLevel.Warn:
                        if (logger.IsWarnEnabled)
                        {
                            logger.Warn(messageFunc());
                            return true;
                        }

                        break;

                    case LogLevel.Error:
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(messageFunc());
                            return true;
                        }

                        break;

                    case LogLevel.Fatal:
                        if (logger.IsFatalEnabled)
                        {
                            logger.Fatal(messageFunc());
                            return true;
                        }

                        break;

                    default:
                        if (logger.IsTraceEnabled)
                        {
                            logger.Trace(messageFunc());
                            return true;
                        }

                        break;
                }

                return false;
            }

#pragma warning disable S1144 // Unused private types or members should be removed
            private static bool IsInTypeHierarchy(Type currentType, Type checkType)
            {
                while (currentType != null && currentType != typeof(object))
                {
                    if (currentType == checkType)
                    {
                        return true;
                    }

                    currentType = currentType.GetBaseTypePortable();
                }

                return false;
            }
#pragma warning restore S1144 // Unused private types or members should be removed

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Pending")]
            private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (logger.IsDebugEnabled)
                        {
                            logger.DebugException(messageFunc(), exception);
                            return true;
                        }

                        break;

                    case LogLevel.Info:
                        if (logger.IsInfoEnabled)
                        {
                            logger.InfoException(messageFunc(), exception);
                            return true;
                        }

                        break;

                    case LogLevel.Warn:
                        if (logger.IsWarnEnabled)
                        {
                            logger.WarnException(messageFunc(), exception);
                            return true;
                        }

                        break;

                    case LogLevel.Error:
                        if (logger.IsErrorEnabled)
                        {
                            logger.ErrorException(messageFunc(), exception);
                            return true;
                        }

                        break;

                    case LogLevel.Fatal:
                        if (logger.IsFatalEnabled)
                        {
                            logger.FatalException(messageFunc(), exception);
                            return true;
                        }

                        break;

                    default:
                        if (logger.IsTraceEnabled)
                        {
                            logger.TraceException(messageFunc(), exception);
                            return true;
                        }

                        break;
                }

                return false;
            }

            private bool IsLogLevelEnable(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        return logger.IsDebugEnabled;

                    case LogLevel.Info:
                        return logger.IsInfoEnabled;

                    case LogLevel.Warn:
                        return logger.IsWarnEnabled;

                    case LogLevel.Error:
                        return logger.IsErrorEnabled;

                    case LogLevel.Fatal:
                        return logger.IsFatalEnabled;

                    default:
                        return logger.IsTraceEnabled;
                }
            }

            private object TranslateLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return LevelTrace;

                    case LogLevel.Debug:
                        return LevelDebug;

                    case LogLevel.Info:
                        return LevelInfo;

                    case LogLevel.Warn:
                        return LevelWarn;

                    case LogLevel.Error:
                        return LevelError;

                    case LogLevel.Fatal:
                        return LevelFatal;

                    default:
                        throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
                }
            }
        }
    }
}
