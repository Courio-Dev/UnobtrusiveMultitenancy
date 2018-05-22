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
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// LoupeLogProvider.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class LoupeLogProvider : LogProviderBase
    {
        private static bool providerIsAvailableOverride = true;
        private readonly WriteDelegate logWriteDelegate;

        public LoupeLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Gibraltar.Agent.Log (Loupe) not found");
            }

            logWriteDelegate = GetLogWriteDelegate();
        }

        /// <summary>
        /// The form of the Loupe Log.Write method we're using.
        /// </summary>
        /// <param name="severity">severity.</param>
        /// <param name="logSystem">logSystem.</param>
        /// <param name="skipFrames">skipFrames.</param>
        /// <param name="exception">exception.</param>
        /// <param name="attributeToException">attributeToException.</param>
        /// <param name="writeMode">writeMode.</param>
        /// <param name="detailsXml">detailsXml.</param>
        /// <param name="category">category.</param>
        /// <param name="caption">caption.</param>
        /// <param name="description">description.</param>
        /// <param name="args">args.</param>
        internal delegate void WriteDelegate(
            int severity,
            string logSystem,
            int skipFrames,
            Exception exception,
            bool attributeToException,
            int writeMode,
            string detailsXml,
            string category,
            string caption,
            string description,
            params object[] args);

        /// <summary>
        /// Gets or sets a value indicating whether [provider is available override]. Used in tests.
        /// </summary>
        /// <value>
        /// <c>true</c> if [provider is available override]; otherwise, <c>false</c>.
        /// </value>
        public static bool ProviderIsAvailableOverride
        {
            get { return providerIsAvailableOverride; }
            set { providerIsAvailableOverride = value; }
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        public override Logger GetLogger(string name)
        {
            return new LoupeLogger(name, logWriteDelegate).Log;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("Gibraltar.Agent.Log, Gibraltar.Agent");
        }

        private static WriteDelegate GetLogWriteDelegate()
        {
            Type logManagerType = GetLogManagerType();
            Type logMessageSeverityType = Type.GetType("Gibraltar.Agent.LogMessageSeverity, Gibraltar.Agent");
            Type logWriteModeType = Type.GetType("Gibraltar.Agent.LogWriteMode, Gibraltar.Agent");

            MethodInfo method = logManagerType.GetMethodPortable(
                "Write",
                logMessageSeverityType,
                typeof(string),
                typeof(int),
                typeof(Exception),
                typeof(bool),
                logWriteModeType,
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(object[]));

            WriteDelegate callDelegate = (WriteDelegate)method.CreateDelegate(typeof(WriteDelegate));
            return callDelegate;
        }

        internal class LoupeLogger
        {
            private const string LogSystem = "LibLog";

            private readonly string category;
            private readonly WriteDelegate logWriteDelegate;
            private readonly int skipLevel;

            internal LoupeLogger(string category, WriteDelegate logWriteDelegate)
            {
                this.category = category;
                this.logWriteDelegate = logWriteDelegate;
#if DEBUG
                skipLevel = 2;
#else
                skipLevel = 1;
#endif
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                if (messageFunc == null)
                {
                    // nothing to log..
                    return true;
                }

                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);

                logWriteDelegate(
                    ToLogMessageSeverity(logLevel),
                    LogSystem,
                    skipLevel,
                    exception,
                    true,
                    0,
                    null,
                    category,
                    null,
                    messageFunc.Invoke());

                return true;
            }

            private static int ToLogMessageSeverity(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return TraceEventTypeValues.Verbose;
                    case LogLevel.Debug:
                        return TraceEventTypeValues.Verbose;
                    case LogLevel.Info:
                        return TraceEventTypeValues.Information;
                    case LogLevel.Warn:
                        return TraceEventTypeValues.Warning;
                    case LogLevel.Error:
                        return TraceEventTypeValues.Error;
                    case LogLevel.Fatal:
                        return TraceEventTypeValues.Critical;
                    default:
                        throw new ArgumentOutOfRangeException("logLevel");
                }
            }
        }
    }
}
