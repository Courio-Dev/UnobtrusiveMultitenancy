using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Puzzle.Core.Multitenancy.Internal
{
    internal class StartupMethodsMultitenant<TTenant> : StartupMethods
    {
        public StartupMethodsMultitenant(StartupMethods methods, Action<IServiceCollection, TTenant> configurePerTenantServices)
            : this(methods.StartupInstance,
                 methods.ConfigureDelegate,
                 methods.ConfigureServicesDelegate,
                 configurePerTenantServices)
        {
        }

        public StartupMethodsMultitenant(object instance,
            Action<IApplicationBuilder> configure,
            Func<IServiceCollection, IServiceProvider> configureServices,
            Action<IServiceCollection, TTenant> configurePerTenantServices)
        : base(instance, configure, configureServices)
        {
            Debug.Assert(instance != null);
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            ConfigurePerTenantServicesDelegate = configurePerTenantServices;
        }

        public Action<IServiceCollection, TTenant> ConfigurePerTenantServicesDelegate { get; }
    }
}