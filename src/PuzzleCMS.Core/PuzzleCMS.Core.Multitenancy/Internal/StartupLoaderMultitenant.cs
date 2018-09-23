namespace PuzzleCMS.Core.Multitenancy.Internal
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Hosting.Internal;

    internal class StartupLoaderMultitenant
    {
        internal static StartupMethodsMultitenant<TTenant> LoadMethods<TTenant>(IServiceProvider hostingServiceProvider, Type startupType, string environmentName)
        {
            StartupMethods methods = StartupLoader.LoadMethods(hostingServiceProvider, startupType, environmentName);
            ConfigureMultitenantServicesBuilder<TTenant> servicesMethosPerTenant = FindConfigurePerTenantServicesDelegate<TTenant>(startupType, null, environmentName);

            return new StartupMethodsMultitenant<TTenant>(methods, servicesMethosPerTenant.Build(methods.StartupInstance));
        }

        private static ConfigureMultitenantServicesBuilder<TTenant> FindConfigurePerTenantServicesDelegate<TTenant>(Type startupType, string tenantName, string environmentName)
        {
            string pertenantKey = "PerTenant";
            string methodName = $@"Configure{pertenantKey}{{0}}Services";
            MethodInfo servicesMethod = FindMethod(startupType, methodName, environmentName, typeof(IServiceProvider))
                ?? FindMethod(startupType, methodName, environmentName, typeof(void));
            return new ConfigureMultitenantServicesBuilder<TTenant>(servicesMethod);
        }

        /// <summary>
        /// Take from :https://github.com/aspnet/Hosting/blob/rel/1.1.0/src/Microsoft.AspNetCore.Hosting/Internal/StartupLoader.cs.
        /// </summary>
        /// <param name="startupType">The type of the Startup Class.</param>
        /// <param name="methodName">The name of method to find in startup class.</param>
        /// <param name="environmentName">The environment(Dev,etc..).</param>
        /// <param name="returnType">The type of return method.</param>
        /// <returns>MethodInfo.</returns>
        private static MethodInfo FindMethod(Type startupType, string methodName, string environmentName, Type returnType = null)
        {
            // Copy the find method.
            // See https://github.com/aspnet/Hosting/blob/rel/1.1.0/src/Microsoft.AspNetCore.Hosting/Internal/StartupLoader.cs.
            string methodNameWithEnv = string.Format(CultureInfo.InvariantCulture, methodName, environmentName);
            string methodNameWithNoEnv = string.Format(CultureInfo.InvariantCulture, methodName, string.Empty);

            MethodInfo[] methods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            System.Collections.Generic.List<MethodInfo> selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithEnv, StringComparison.OrdinalIgnoreCase)).ToList();
            if (selectedMethods.Count > 1)
            {
                throw new InvalidOperationException(string.Format("Having multiple overloads of method '{0}' is not supported.", methodNameWithEnv));
            }

            if (selectedMethods.Count == 0)
            {
                selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithNoEnv, StringComparison.OrdinalIgnoreCase)).ToList();
                if (selectedMethods.Count > 1)
                {
                    throw new InvalidOperationException(string.Format("Having multiple overloads of method '{0}' is not supported.", methodNameWithNoEnv));
                }
            }

            MethodInfo methodInfo = selectedMethods.FirstOrDefault();
            if (methodInfo == null)
            {
                return null;
            }

            if (returnType != null && methodInfo.ReturnType != returnType)
            {
                return null;
            }

            return methodInfo;
        }
    }
}
