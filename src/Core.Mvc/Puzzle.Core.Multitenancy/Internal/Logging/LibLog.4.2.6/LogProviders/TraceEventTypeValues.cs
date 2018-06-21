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
    using System.Reflection;

    /// <summary>
    /// TraceEventTypeValues.
    /// </summary>
    [Multitenancy.ExcludeFromCodeCoverage]
    internal static class TraceEventTypeValues
    {
        internal static readonly Type Type;
        internal static readonly int Verbose;
        internal static readonly int Information;
        internal static readonly int Warning;
        internal static readonly int Error;
        internal static readonly int Critical;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Pending")]
        static TraceEventTypeValues()
        {
            Assembly assembly = typeof(Uri).GetAssemblyPortable(); // This is to get to the System.dll assembly in a PCL compatible way.
            if (assembly == null)
            {
                return;
            }

            Type = assembly.GetType("System.Diagnostics.TraceEventType");
            if (Type == null)
            {
                return;
            }

            Verbose = (int)Enum.Parse(Type, "Verbose", false);
            Information = (int)Enum.Parse(Type, "Information", false);
            Warning = (int)Enum.Parse(Type, "Warning", false);
            Error = (int)Enum.Parse(Type, "Error", false);
            Critical = (int)Enum.Parse(Type, "Critical", false);
        }
    }
}
