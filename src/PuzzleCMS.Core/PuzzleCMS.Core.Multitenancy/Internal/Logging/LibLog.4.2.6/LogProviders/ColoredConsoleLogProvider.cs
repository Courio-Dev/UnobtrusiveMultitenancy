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
    using System.Globalization;

    /// <summary>
    /// Provider for build logger for console.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ColoredConsoleLogProvider : ILogProvider
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>
        {
                { LogLevel.Fatal, ConsoleColor.Red },
                { LogLevel.Error, ConsoleColor.Yellow },
                { LogLevel.Warn, ConsoleColor.Magenta },
                { LogLevel.Info, ConsoleColor.White },
                { LogLevel.Debug, ConsoleColor.Gray },
                { LogLevel.Trace, ConsoleColor.DarkGray },
        };

        /// <summary>
        /// Gets the specified console named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference.</returns>
        public Logger GetLogger(string name)
        {
            return (logLevel, messageFunc, exception, formatParameters) =>
            {
                if (messageFunc == null)
                {
                    return true; // All log levels are enabled
                }

                if (Colors.TryGetValue(logLevel, out ConsoleColor consoleColor))
                {
                    ConsoleColor originalForground = Console.ForegroundColor;
                    try
                    {
                        Console.ForegroundColor = consoleColor;
                        WriteMessage(logLevel, name, messageFunc, formatParameters, exception);
                    }
                    finally
                    {
                        Console.ForegroundColor = originalForground;
                    }
                }
                else
                {
                    WriteMessage(logLevel, name, messageFunc, formatParameters, exception);
                }

                return true;
            };
        }

        /// <summary>
        /// Opens a nested diagnostics context.
        /// </summary>
        /// <param name="message">The message to add to the diagnostics context.</param>
        /// <returns>A disposable that when disposed removes the message from the context.</returns>
        public IDisposable OpenNestedContext(string message)
        {
            return NullDisposable.Instance;
        }

        /// <summary>
        /// Opens a mapped diagnostics context.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>A disposable that when disposed removes the map from the context.</returns>
        public IDisposable OpenMappedContext(string key, string value)
        {
            return NullDisposable.Instance;
        }

        private static void WriteMessage(
            LogLevel logLevel,
            string name,
            Func<string> messageFunc,
            object[] formatParameters,
            Exception exception)
        {
            string message = string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters);
            if (exception != null)
            {
                message = message + "|" + exception;
            }

            Console.WriteLine("{0} | {1} | {2} | {3}", DateTime.UtcNow, logLevel, name, message);
        }

        private sealed class NullDisposable : IDisposable
        {
            internal static readonly IDisposable Instance = new NullDisposable();

            public void Dispose()
            {
                Console.Write(nameof(NullDisposable));
            }
        }
    }
}
