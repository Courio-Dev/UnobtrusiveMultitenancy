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
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// LogMessageFormatter.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class LogMessageFormatter
    {
        private static readonly Regex Pattern = new Regex(@"(?<!{){@?(?<arg>[^\d{][^ }]*)}");

        
#pragma warning disable S125 // Sections of code should not be commented out
// private static readonly Regex Pattern = new Regex(@"(?<!{){@?(?<arg>[^ :{}]+)(?<format>:[^}]+)?}", RegexOptions.Compiled);

        /// <summary>
        /// Some logging frameworks support structured logging, such as serilog. This will allow you to add names to structured data in a format string:
        /// For example: Log("Log message to {user}", user). This only works with serilog, but as the user of LibLog, you don't know if serilog is actually
        /// used. So, this class simulates that. it will replace any text in {curly braces} with an index number.
        ///
        /// "Log {message} to {user}" would turn into => "Log {0} to {1}". Then the format parameters are handled using regular .net string.Format.
        /// </summary>
        /// <param name="messageBuilder">The message builder.</param>
        /// <param name="formatParameters">The format parameters.</param>
        /// <returns>Func of string.</returns>
        public static Func<string> SimulateStructuredLogging(Func<string> messageBuilder, object[] formatParameters)
#pragma warning restore S125 // Sections of code should not be commented out
        {
            if (formatParameters == null || formatParameters.Length == 0)
            {
                return messageBuilder;
            }

            return () =>
            {
                string targetMessage = messageBuilder();
                return FormatStructuredMessage(targetMessage, formatParameters, out IEnumerable<string> patternMatches);
            };
        }

        public static string FormatStructuredMessage(string targetMessage, object[] formatParameters, out IEnumerable<string> patternMatches)
        {
            if (formatParameters.Length == 0)
            {
                patternMatches = Enumerable.Empty<string>();
                return targetMessage;
            }

            List<string> processedArguments = new List<string>();
            patternMatches = processedArguments;

            foreach (Match match in Pattern.Matches(targetMessage))
            {
                string arg = match.Groups["arg"].Value;

                if (!int.TryParse(arg, out int notUsed))
                {
                    int argumentIndex = processedArguments.IndexOf(arg);
                    if (argumentIndex == -1)
                    {
                        argumentIndex = processedArguments.Count;
                        processedArguments.Add(arg);
                    }

                    targetMessage = ReplaceFirst(targetMessage, match.Value, "{" + argumentIndex + match.Groups["format"].Value + "}");
                }
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, targetMessage, formatParameters);
            }
            catch (FormatException ex)
            {
                throw new FormatException("The input string '" + targetMessage + "' could not be formatted using string.Format", ex);
            }
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
