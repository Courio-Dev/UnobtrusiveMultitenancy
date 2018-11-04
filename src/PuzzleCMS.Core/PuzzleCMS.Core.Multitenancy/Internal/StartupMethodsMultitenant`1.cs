namespace PuzzleCMS.Core.Multitenancy.Internal
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class StartupMethodsMultitenant<TTenant> : StartupMethods
    {
        public StartupMethodsMultitenant(StartupMethods methods, Action<IServiceCollection, TTenant, IConfiguration> configurePerTenantServices)
            : this(
                methods.StartupInstance,
                methods.ConfigureDelegate,
                methods.ConfigureServicesDelegate,
                configurePerTenantServices)
        {
        }

        public StartupMethodsMultitenant(
            object instance,
            Action<IApplicationBuilder> configure,
            Func<IServiceCollection, IServiceProvider> configureServices,
            Action<IServiceCollection, TTenant, IConfiguration> configurePerTenantServices)
        : base(instance, configure, configureServices)
        {
            Debug.Assert(instance != null, nameof(instance));
            Debug.Assert(configure != null, nameof(configure));
            Debug.Assert(configureServices != null, nameof(configureServices));

            ConfigurePerTenantServicesDelegate = configurePerTenantServices;
        }

        public Action<IServiceCollection, TTenant, IConfiguration> ConfigurePerTenantServicesDelegate { get; }
    }
}
