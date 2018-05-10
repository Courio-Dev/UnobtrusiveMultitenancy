namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Constants;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using Puzzle.Core.Multitenancy.Internal.StartupFilters;

    /// <summary>
    /// Extensions methos for add multitenancy in new or existings applications.
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class</typeparam>
        /// <param name="hostBuilder">The hostbuilder class</param>
        /// <param name="multitenancyConfiguration">The configuration which contains MultitenancyOptions</param>
        /// <returns>IWebHostBuilder</returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup>(this IWebHostBuilder hostBuilder, IConfiguration multitenancyConfiguration)
            where TStartup : class
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, AppTenant, CachingAppTenantResolver>(
                typeof(TStartup), multitenancyConfiguration);
        }

        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class</typeparam>
        /// <typeparam name="TTenant">The tenant class</typeparam>
        /// <typeparam name="TResolver">The Resolver tenant</typeparam>
        /// <param name="hostBuilder">hostBuilder</param>
        /// <param name="multitenancyConfiguration">The configuration which contains MultitenancyOptions</param>
        /// <returns>IWebHostBuilder</returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
            this IWebHostBuilder hostBuilder, IConfiguration multitenancyConfiguration = null)
                where TStartup : class
                where TTenant : class
                where TResolver : class, ITenantResolver<TTenant>
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
                typeof(TStartup),
                multitenancyConfiguration: multitenancyConfiguration);
        }

        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class</typeparam>
        /// <param name="hostBuilder">hostBuilder</param>
        /// <returns>IWebHostBuilder</returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartupWithDefaultConvention<TStartup>(this IWebHostBuilder hostBuilder)
        where TStartup : class
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, AppTenant, CachingAppTenantResolver>(
                typeof(TStartup), multitenancyConfiguration: null);
        }

        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class</typeparam>
        /// <typeparam name="TTenant">The tenant class</typeparam>
        /// <typeparam name="TResolver">The Resolver tenant</typeparam>
        /// <param name="hostBuilder">hostBuilder</param>
        /// <param name="startupType">The type of the startup class</param>
        /// <param name="multitenancyConfiguration">The configuration which contains MultitenancyOptions</param>
        /// <returns>IWebHostBuilder</returns>
        private static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
            this IWebHostBuilder hostBuilder,
            Type startupType,
            IConfiguration multitenancyConfiguration)
                where TStartup : class
                where TTenant : class
                where TResolver : class, ITenantResolver<TTenant>
        {
            string environment = hostBuilder.GetSetting("environment");
            MultiTenancyConfig multitenancyConfig = new MultiTenancyConfig(environment, multitenancyConfiguration);

            if (multitenancyConfig.Config == null)
            {
                throw new ArgumentNullException(nameof(multitenancyConfig));
            }

            MultitenancyOptions buildedOptions = multitenancyConfig.CurrentMultiTenacyOptionsValue;

            if (!(buildedOptions?.Tenants?.Any() ?? false))
            {
                return hostBuilder.UseStartup<TStartup>();
            }
            else
            {
                string startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;

                return hostBuilder
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);
                    hostBuilder.UseSetting(MultitenancyConstants.UseUnobstrusiveMulitenancyStartupKey, startupAssemblyName);
                    hostBuilder.UseSetting(WebHostDefaults.StartupAssemblyKey, null);
                })
                .ConfigureServices((WebHostBuilderContext ctx, IServiceCollection services) =>
                {
                    {
                        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

                        services.AddSingleton((s) => multitenancyConfig);
                        /*
                        services.AddSingleton<IConfigureOptions<MultitenancyOptions>, MultitenancyOptionsSetup>();
                        //services.Configure<MultitenancyOptions>(multitenancyConfig.Config.GetSection(nameof(MultitenancyOptions)));
                        services.AddScoped(cfg => cfg.GetService<IOptionsMonitor<MultitenancyOptions>>().CurrentValue);
                        services.AddSingleton<IOptionsMonitor<MultitenancyOptions>, MonitorMultitenancyOptions>();
                        */

                        // Register multitenancy options.
                        services.AddMultitenancyOptions(multitenancyConfig);

                        services.AddMultitenancy<TTenant, TResolver>();

                        // manage IStartup
                        services.RemoveAll<IStartup>();

                        ServiceDescriptor descriptor = new ServiceDescriptor(
                        typeof(IStartupFilter),
                        sp =>
                        {
                            IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy = sp.GetRequiredService<IServiceFactoryForMultitenancy<TTenant>>();
                            return new MultitenantRequestStartupFilter<TStartup, TTenant>(serviceFactoryForMultitenancy);
                        },
                        lifetime: ServiceLifetime.Transient);
                        services.Insert(0, descriptor);

                        ServiceDescriptor staruptDescriptor = new ServiceDescriptor(
                        typeof(IStartup),
                        (IServiceProvider provider) =>
                        {
                            IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                            StartupMethodsMultitenant<TTenant> methods = StartupLoaderMultitenant.LoadMethods<TTenant>(provider, startupType, hostingEnvironment.EnvironmentName);
                            return new ConventionMultitenantBasedStartup<TTenant>(methods);
                        },
                        lifetime: ServiceLifetime.Singleton);

                        // services.Add(staruptDescriptor);
                        services.Insert(0, staruptDescriptor);
                    }
                })
                ;
            }
        }
    }
}
