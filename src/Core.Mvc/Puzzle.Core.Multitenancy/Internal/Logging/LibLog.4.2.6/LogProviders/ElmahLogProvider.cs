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
    /*
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    [ExcludeFromCoverage]
    public class ElmahLogProvider : ILogProvider
    {
        private const LogLevel DefaultMinLevel = LogLevel.Error;
        private static bool providerIsAvailableOverride = true;
        private readonly Type errorType;

        private readonly LogLevel minLevel;
        private readonly Func<object> getErrorLogDelegate;

        public ElmahLogProvider()
            : this(DefaultMinLevel)
        {
        }

        public ElmahLogProvider(LogLevel minLevel)
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("`Elmah.ErrorLog` or `Elmah.Error` type not found");
            }

            this.minLevel = minLevel;

            errorType = GetErrorType();
            getErrorLogDelegate = GetGetErrorLogMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return providerIsAvailableOverride; }
            set { providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name)
        {
            return new ElmahLog(minLevel, getErrorLogDelegate(), errorType);
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null && GetErrorType() != null;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("Elmah.ErrorLog, Elmah");
        }

        private static Type GetHttpContextType()
        {
            return Type.GetType(
                $"System.Web.HttpContext, System.Web, Version={Environment.Version}, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        }

        private static Type GetErrorType()
        {
            return Type.GetType("Elmah.Error, Elmah");
        }

        private static Func<object> GetGetErrorLogMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            Type httpContextType = GetHttpContextType();
            MethodInfo method = logManagerType.GetMethod("GetDefault", new[] { httpContextType });
            ConstantExpression contextValue = Expression.Constant(null, httpContextType);
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] { contextValue });
            return Expression.Lambda<Func<object>>(methodCall).Compile();
        }

        internal class ElmahLog : ILog
        {
            private readonly LogLevel minLevel;
            private readonly Type errorType;
            private readonly dynamic errorLog;

            public ElmahLog(LogLevel minLevel, dynamic errorLog, Type errorType)
            {
                this.minLevel = minLevel;
                this.errorType = errorType;
                this.errorLog = errorLog;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                if (messageFunc == null)
                {
                    return logLevel >= minLevel;
                }

                string message = messageFunc();

                dynamic error = exception == null
                    ? Activator.CreateInstance(errorType)
                    : Activator.CreateInstance(errorType, exception);

                error.Message = message;
                error.Type = logLevel.ToString();
                error.Time = DateTime.Now;
                error.ApplicationName = "Hangfire";

                try
                {
                    errorLog.Log(error);
                }
                catch (Exception ex)
                {
                    Debug.Print("Error: {0}\n{1}", ex.Message, ex.StackTrace);
                }

                return true;
            }
        }
    }

    */
}
