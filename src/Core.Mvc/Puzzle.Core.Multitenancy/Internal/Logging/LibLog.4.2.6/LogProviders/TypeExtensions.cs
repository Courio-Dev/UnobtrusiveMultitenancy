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
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// TypeExtensions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class TypeExtensions
    {
        internal static ConstructorInfo GetConstructorPortable(this Type type, params Type[] types)
        {
#if LIBLOG_PORTABLE
            return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(constructor =>
                            constructor.GetParameters()
                                       .Select(parameter => parameter.ParameterType)
                                       .SequenceEqual(types));
#else
            return type.GetConstructor(types);
#endif
        }

        internal static MethodInfo GetMethodPortable(this Type type, string name)
        {
#if LIBLOG_PORTABLE
            return type.GetRuntimeMethods().SingleOrDefault(m => m.Name == name);
#else
            return type.GetMethod(name);
#endif
        }

        internal static MethodInfo GetMethodPortable(this Type type, string name, params Type[] types)
        {
#if LIBLOG_PORTABLE
            return type.GetRuntimeMethod(name, types);
#else
            return type.GetMethod(name, types);
#endif
        }

        internal static PropertyInfo GetPropertyPortable(this Type type, string name)
        {
#if LIBLOG_PORTABLE
            return type.GetRuntimeProperty(name);
#else
            return type.GetProperty(name);
#endif
        }

        internal static IEnumerable<FieldInfo> GetFieldsPortable(this Type type)
        {
#if LIBLOG_PORTABLE
            return type.GetRuntimeFields();
#else
            return type.GetFields();
#endif
        }

        internal static Type GetBaseTypePortable(this Type type)
        {
#if LIBLOG_PORTABLE
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

#if LIBLOG_PORTABLE
        internal static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod;
        }

        internal static MethodInfo GetSetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.SetMethod;
        }
#endif

#if !LIBLOG_PORTABLE
        internal static object CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, methodInfo);
        }
#endif

        internal static Assembly GetAssemblyPortable(this Type type)
        {
#if LIBLOG_PORTABLE
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }
    }
}
