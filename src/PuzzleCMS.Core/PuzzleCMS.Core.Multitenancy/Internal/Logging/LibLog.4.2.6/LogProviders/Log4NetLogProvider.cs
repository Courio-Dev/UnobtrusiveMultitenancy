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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Log4NetLogProvider.
    /// </summary>
    [Multitenancy.ExcludeFromCodeCoverage]
    internal class Log4NetLogProvider : LogProviderBase
    {
        private readonly Func<string, object> getLoggerByNameDelegate;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogManager", Justification = "Pending")]
        public Log4NetLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("log4net.LogManager not found");
            }

            getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride { get; set; } = true;

        public override Logger GetLogger(string name)
        {
            return new Log4NetLogger(getLoggerByNameDelegate(name)).Log;
        }

        internal static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        protected override OpenNdc GetOpenNdcMethod()
        {
            Type logicalThreadContextType = Type.GetType("log4net.LogicalThreadContext, log4net");
            PropertyInfo stacksProperty = logicalThreadContextType.GetPropertyPortable("Stacks");
            Type logicalThreadContextStacksType = stacksProperty.PropertyType;
            PropertyInfo stacksIndexerProperty = logicalThreadContextStacksType.GetPropertyPortable("Item");
            Type stackType = stacksIndexerProperty.PropertyType;
            MethodInfo pushMethod = stackType.GetMethodPortable("Push");

            ParameterExpression messageParameter =
                Expression.Parameter(typeof(string), "message");

            MethodCallExpression callPushBody =
                Expression.Call(
                    Expression.Property(
                        Expression.Property(null, stacksProperty),
                        stacksIndexerProperty,
                        Expression.Constant("NDC")),
                    pushMethod,
                    messageParameter);

            OpenNdc result =
                Expression.Lambda<OpenNdc>(callPushBody, messageParameter)
                          .Compile();

            return result;
        }

        protected override OpenMdc GetOpenMdcMethod()
        {
            Type logicalThreadContextType = Type.GetType("log4net.LogicalThreadContext, log4net");
            PropertyInfo propertiesProperty = logicalThreadContextType.GetPropertyPortable("Properties");
            Type logicalThreadContextPropertiesType = propertiesProperty.PropertyType;
            PropertyInfo propertiesIndexerProperty = logicalThreadContextPropertiesType.GetPropertyPortable("Item");

            MethodInfo removeMethod = logicalThreadContextPropertiesType.GetMethodPortable("Remove");

            ParameterExpression keyParam = Expression.Parameter(typeof(string), "key");
            ParameterExpression valueParam = Expression.Parameter(typeof(string), "value");

            MemberExpression propertiesExpression = Expression.Property(null, propertiesProperty);

            BinaryExpression setProperties = Expression.Assign(Expression.Property(propertiesExpression, propertiesIndexerProperty, keyParam), valueParam);

            MethodCallExpression removeMethodCall = Expression.Call(propertiesExpression, removeMethod, keyParam);

            Action<string, string> set = Expression
                .Lambda<Action<string, string>>(setProperties, keyParam, valueParam)
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
            return Type.GetType("log4net.LogManager, log4net");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethodPortable("GetLogger", typeof(string));
            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, nameParam);
            return Expression.Lambda<Func<string, object>>(methodCall, nameParam).Compile();
        }

        internal class Log4NetLogger
        {
            private static readonly object CallerStackBoundaryTypeSync = new object();
            private static Type callerStackBoundaryType;

            private readonly dynamic logger;

            private readonly object levelDebug;
            private readonly object levelInfo;
            private readonly object levelWarn;
            private readonly object levelError;
            private readonly object levelFatal;
            private readonly Func<object, object, bool> isEnabledForDelegate;
            private readonly Action<object, object> logDelegate;
            private readonly Func<object, Type, object, string, Exception, object> createLoggingEvent;
            private readonly Action<object, string, object> loggingEventPropertySetter;

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ILogger", Justification = "Pending")]
            internal Log4NetLogger(dynamic logger)
            {
                this.logger = logger.Logger;

                Type logEventLevelType = Type.GetType("log4net.Core.Level, log4net");
                if (logEventLevelType == null)
                {
                    throw new InvalidOperationException("Type log4net.Core.Level was not found.");
                }

                List<FieldInfo> levelFields = logEventLevelType.GetFieldsPortable().ToList();
                levelDebug = levelFields.First(x => x.Name == "Debug").GetValue(null);
                levelInfo = levelFields.First(x => x.Name == "Info").GetValue(null);
                levelWarn = levelFields.First(x => x.Name == "Warn").GetValue(null);
                levelError = levelFields.First(x => x.Name == "Error").GetValue(null);
                levelFatal = levelFields.First(x => x.Name == "Fatal").GetValue(null);

                Type loggerType = Type.GetType("log4net.Core.ILogger, log4net");
                if (loggerType == null)
                {
                    throw new InvalidOperationException("Type log4net.Core.ILogger, was not found.");
                }

                ParameterExpression instanceParam = Expression.Parameter(typeof(object));
                UnaryExpression instanceCast = Expression.Convert(instanceParam, loggerType);
                ParameterExpression levelParam = Expression.Parameter(typeof(object));
                UnaryExpression levelCast = Expression.Convert(levelParam, logEventLevelType);
                isEnabledForDelegate = GetIsEnabledFor(loggerType, logEventLevelType, instanceCast, levelCast, instanceParam, levelParam);

                Type loggingEventType = Type.GetType("log4net.Core.LoggingEvent, log4net");

                createLoggingEvent = GetCreateLoggingEvent(instanceParam, instanceCast, levelParam, levelCast, loggingEventType);

                logDelegate = GetLogDelegate(loggerType, loggingEventType, instanceCast, instanceParam);

                loggingEventPropertySetter = GetLoggingEventPropertySetter(loggingEventType);
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                if (messageFunc == null)
                {
                    return IsLogLevelEnable(logLevel);
                }

                if (!IsLogLevelEnable(logLevel))
                {
                    return false;
                }

                string message = messageFunc();

                string formattedMessage = LogMessageFormatter.FormatStructuredMessage(
                    message,
                    formatParameters,
                    out IEnumerable<string> patternMatches);

                // determine correct caller - this might change due to jit optimizations with method inlining
                if (callerStackBoundaryType == null)
                {
                    lock (CallerStackBoundaryTypeSync)
                    {
#if !LIBLOG_PORTABLE
                        StackTrace stack = new StackTrace();
                        Type thisType = GetType();
                        s_callerStackBoundaryType = Type.GetType("LoggerExecutionWrapper");
                        for (int i = 1; i < stack.FrameCount; i++)
                        {
                            if (!IsInTypeHierarchy(thisType, stack.GetFrame(i).GetMethod().DeclaringType))
                            {
                                s_callerStackBoundaryType = stack.GetFrame(i - 1).GetMethod().DeclaringType;
                                break;
                            }
                        }
#else
#pragma warning disable S2696 // Instance members should not write to "static" fields
                        callerStackBoundaryType = typeof(LoggerExecutionWrapper);
#pragma warning restore S2696 // Instance members should not write to "static" fields
#endif
                    }
                }

                object translatedLevel = TranslateLevel(logLevel);

                object loggingEvent = createLoggingEvent(logger, callerStackBoundaryType, translatedLevel, formattedMessage, exception);

                PopulateProperties(loggingEvent, patternMatches, formatParameters);

                logDelegate(logger, loggingEvent);

                return true;
            }

            private static Action<object, object> GetLogDelegate(Type loggerType, Type loggingEventType, UnaryExpression instanceCast, ParameterExpression instanceParam)
            {
                MethodInfo writeExceptionMethodInfo = loggerType.GetMethodPortable(
                    "Log",
                    loggingEventType);

                ParameterExpression loggingEventParameter =
                    Expression.Parameter(typeof(object), "loggingEvent");

                UnaryExpression loggingEventCasted =
                    Expression.Convert(loggingEventParameter, loggingEventType);

                MethodCallExpression writeMethodExp = Expression.Call(
                    instanceCast,
                    writeExceptionMethodInfo,
                    loggingEventCasted);

#pragma warning disable S1117 // Local variables should not shadow class fields
                Action<object, object> logDelegate = Expression.Lambda<Action<object, object>>(
#pragma warning restore S1117 // Local variables should not shadow class fields
                                                writeMethodExp,
                                                instanceParam,
                                                loggingEventParameter).Compile();

                return logDelegate;
            }

            private static Func<object, Type, object, string, Exception, object> GetCreateLoggingEvent(ParameterExpression instanceParam, UnaryExpression instanceCast, ParameterExpression levelParam, UnaryExpression levelCast, Type loggingEventType)
            {
                ParameterExpression callerStackBoundaryDeclaringTypeParam = Expression.Parameter(typeof(Type));
                ParameterExpression messageParam = Expression.Parameter(typeof(string));
                ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception));

                PropertyInfo repositoryProperty = loggingEventType.GetPropertyPortable("Repository");
                PropertyInfo levelProperty = loggingEventType.GetPropertyPortable("Level");

                ConstructorInfo loggingEventConstructor =
                    loggingEventType.GetConstructorPortable(typeof(Type), repositoryProperty.PropertyType, typeof(string), levelProperty.PropertyType, typeof(object), typeof(Exception));

                NewExpression newLoggingEventExpression =
                    Expression.New(
                        loggingEventConstructor,
                        callerStackBoundaryDeclaringTypeParam,
                        Expression.Property(instanceCast, "Repository"),
                        Expression.Property(instanceCast, "Name"),
                        levelCast,
                        messageParam,
                        exceptionParam);

#pragma warning disable S1117 // Local variables should not shadow class fields
                Func<object, Type, object, string, Exception, object> createLoggingEvent =
#pragma warning restore S1117 // Local variables should not shadow class fields
                    Expression.Lambda<Func<object, Type, object, string, Exception, object>>(
                                  newLoggingEventExpression,
                                  instanceParam,
                                  callerStackBoundaryDeclaringTypeParam,
                                  levelParam,
                                  messageParam,
                                  exceptionParam)
                              .Compile();

                return createLoggingEvent;
            }

            private static Func<object, object, bool> GetIsEnabledFor(
                Type loggerType,
                Type logEventLevelType,
                UnaryExpression instanceCast,
                UnaryExpression levelCast,
                ParameterExpression instanceParam,
                ParameterExpression levelParam)
            {
                MethodInfo isEnabledMethodInfo = loggerType.GetMethodPortable("IsEnabledFor", logEventLevelType);
                MethodCallExpression isEnabledMethodCall = Expression.Call(instanceCast, isEnabledMethodInfo, levelCast);

                Func<object, object, bool> result =
                    Expression.Lambda<Func<object, object, bool>>(isEnabledMethodCall, instanceParam, levelParam)
                              .Compile();

                return result;
            }

            private static Action<object, string, object> GetLoggingEventPropertySetter(Type loggingEventType)
            {
                ParameterExpression loggingEventParameter = Expression.Parameter(typeof(object), "loggingEvent");
                ParameterExpression keyParameter = Expression.Parameter(typeof(string), "key");
                ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

                PropertyInfo propertiesProperty = loggingEventType.GetPropertyPortable("Properties");
                PropertyInfo item = propertiesProperty.PropertyType.GetPropertyPortable("Item");

                BinaryExpression body =
                    Expression.Assign(
                        Expression.Property(
                            Expression.Property(
                                Expression.Convert(loggingEventParameter, loggingEventType),
                                propertiesProperty),
                            item,
                            keyParameter), valueParameter);

                Action<object, string, object> result =
                    Expression.Lambda<Action<object, string, object>>(body, loggingEventParameter, keyParameter, valueParameter)
                              .Compile();

                return result;
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

            private void PopulateProperties(object loggingEvent, IEnumerable<string> patternMatches, object[] formatParameters)
            {
                IEnumerable<KeyValuePair<string, object>> keyToValue =
                    patternMatches.Zip(
                        formatParameters,
                        (key, value) => new KeyValuePair<string, object>(key, value));

                foreach (KeyValuePair<string, object> keyValuePair in keyToValue)
                {
                    loggingEventPropertySetter(loggingEvent, keyValuePair.Key, keyValuePair.Value);
                }
            }

            private bool IsLogLevelEnable(LogLevel logLevel)
            {
                object level = TranslateLevel(logLevel);
                return isEnabledForDelegate(logger, level);
            }

            private object TranslateLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        return levelDebug;

                    case LogLevel.Info:
                        return levelInfo;

                    case LogLevel.Warn:
                        return levelWarn;

                    case LogLevel.Error:
                        return levelError;

                    case LogLevel.Fatal:
                        return levelFatal;

                    default:
                        throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
                }
            }
        }
    }
}
