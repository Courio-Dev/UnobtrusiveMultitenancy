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
    using Puzzle.Core.Multitenancy.Constants;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Logging;
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
        /// <typeparam name="TStartup">The stratup class.</typeparam>
        /// <param name="hostBuilder">The hostbuilder class.</param>
        /// <param name="multitenancyConfiguration">The configuration which contains MultitenancyOptions.</param>
        /// <param name="throwErrorIfOptionsNotFound">Boolean to indicate if throws exception when MultitenancyOptions is not found.</param>
        /// <param name="actionConfiguration">Additionnal config.</param>
        /// <returns>IWebHostBuilder.</returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup>(
            this IWebHostBuilder hostBuilder, 
            IConfigurationRoot multitenancyConfiguration, 
            bool throwErrorIfOptionsNotFound=true,
            Action<MultiTenancyConfig<AppTenant>> actionConfiguration = null)
            where TStartup : class
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.
                UseUnobtrusiveMulitenancyStartup<TStartup, AppTenant, AppTenantResolver>(
                typeof(TStartup), 
                multitenancyConfiguration,
                throwErrorIfOptionsNotFound,
                actionConfiguration: actionConfiguration);
        }

        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class.</typeparam>
        /// <typeparam name="TTenant">The tenant class.</typeparam>
        /// <typeparam name="TResolver">The Resolver tenant.</typeparam>
        /// <param name="hostBuilder">hostBuilder.</param>
        /// <param name="multitenancyConfiguration">The configuration which contains MultitenancyOptions.</param>
        /// <param name="throwErrorIfOptionsNotFound">Boolean to indicate if throws exception when MultitenancyOptions is not found.</param>
        /// <param name="actionConfiguration">Additionnal config.</param>
        /// <returns>IWebHostBuilder.</returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
            this IWebHostBuilder hostBuilder, 
            IConfigurationRoot multitenancyConfiguration = null, 
            bool throwErrorIfOptionsNotFound = true,
            Action<MultiTenancyConfig<TTenant>> actionConfiguration = null)
                where TStartup : class
                where TTenant : class
                where TResolver : class, ITenantResolver<TTenant>
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.
                UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
                typeof(TStartup),
                multitenancyConfiguration: multitenancyConfiguration,
                throwErrorIfOptionsNotFound: throwErrorIfOptionsNotFound,
                actionConfiguration: actionConfiguration);
        }

        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class.</typeparam>
        /// <param name="hostBuilder">hostBuilder.</param>
        /// <param name="throwErrorIfOptionsNotFound">Boolean to indicate if throws exception when MultitenancyOptions is not found.</param>
        /// <param name="actionConfiguration">Additionnal config.</param>
        /// <returns>IWebHostBuilder.</returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartupWithDefaultConvention<TStartup>(
            this IWebHostBuilder hostBuilder, 
            bool throwErrorIfOptionsNotFound = true,
            Action<MultiTenancyConfig<AppTenant>> actionConfiguration=null)
        where TStartup : class
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, AppTenant, AppTenantResolver>(
                typeof(TStartup), 
                multitenancyConfiguration: null,
                throwErrorIfOptionsNotFound: throwErrorIfOptionsNotFound,
                actionConfiguration: actionConfiguration);
        }

        /// <summary>
        /// Add multitenancy feature.
        /// </summary>
        /// <typeparam name="TStartup">The stratup class.</typeparam>
        /// <typeparam name="TTenant">The tenant class.</typeparam>
        /// <typeparam name="TResolver">The Resolver tenant.</typeparam>
        /// <param name="hostBuilder">hostBuilder.</param>
        /// <param name="startupType">The type of the startup class.</param>
        /// <param name="multitenancyConfiguration">The configuration which contains MultitenancyOptions.</param>
        /// <param name="throwErrorIfOptionsNotFound">Boolean to indicate if throws exception when MultitenancyOptions is not found.</param>
        /// <param name="actionConfiguration">Additionnal config.</param>
        /// <returns>IWebHostBuilder.</returns>
        internal static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
            this IWebHostBuilder hostBuilder,
            Type startupType,
            IConfigurationRoot multitenancyConfiguration,
            bool throwErrorIfOptionsNotFound,
            Action<MultiTenancyConfig<TTenant>> actionConfiguration)
                where TStartup : class
                where TTenant : class
                where TResolver : class, ITenantResolver<TTenant>
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            MultiTenancyConfig<TTenant> multitenancyConfig = new MultiTenancyConfig<TTenant>(
                hostBuilder.GetSetting("environment"), 
                multitenancyConfiguration);

            MultitenancyOptions<TTenant> buildedOptions = multitenancyConfig.CurrentMultiTenacyOptionsValue;

            if(throwErrorIfOptionsNotFound && (buildedOptions == null ||!(buildedOptions?.Tenants?.Any() ?? false)))
            {
                throw new Exception("MultitenancyOptions not found in configuration.");
            }

            if (!(buildedOptions?.Tenants?.Any() ?? false))
            {
                return hostBuilder.UseStartup<TStartup>();
            }
            else
            {
                string startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;

                return hostBuilder
                .UseDefaultServiceProvider(options => options.ValidateScopes = false)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);
                    hostBuilder.UseSetting(MultitenancyConstants.UseUnobstrusiveMulitenancyStartupKey, startupAssemblyName);
                    hostBuilder.UseSetting(WebHostDefaults.StartupAssemblyKey, null);
                })
                .ConfigureServices((WebHostBuilderContext ctx, IServiceCollection services) =>
                {
                    {
                        // Add logging
                        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILog<>), typeof(Log<>)));
                        multitenancyConfig.UseColoredConsoleLogProvider();

                        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

                        // Register multitenancy options.
                        services.AddMultitenancyOptions<TTenant>(multitenancyConfig);

                        services.AddMultitenancy<TTenant, TResolver>();

                        // manage IStartup
                        services.RemoveAll<IStartup>();


                        ServiceDescriptor descriptor = new ServiceDescriptor(
                            typeof(IStartupFilter),
                            sp => new MultitenantRequestStartupFilter<TStartup, TTenant>(),
                            ServiceLifetime.Transient);
                        services.Insert(0, descriptor);


                        ServiceDescriptor staruptDescriptor = new ServiceDescriptor(
                        typeof(IStartup),
                        (IServiceProvider provider) =>
                        {
                            IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                            StartupMethodsMultitenant<TTenant> methods = StartupLoaderMultitenant.LoadMethods<TTenant>(provider, startupType, hostingEnvironment.EnvironmentName);
                            return new ConventionMultitenantBasedStartup<TTenant>(methods);
                        }, lifetime: ServiceLifetime.Singleton);

                        // services.Add(staruptDescriptor);
                        services.Insert(1, staruptDescriptor);


                        services.AddSingleton<MultiTenancyConfig<TTenant>>(serviceProvider =>
                        {
                            /*// LogProvider logProvider = serviceProvider.GetService<LogProvider>();
                            if (logProvider != null)
                            {
                                // multitenancyConfig.UseLogProvider(new AspNetCoreMultiTenantLogProvider(loggerFactory));
                            }*/
                            actionConfiguration?.Invoke(multitenancyConfig);

                            return multitenancyConfig;
                        });
                    }
                })
                ;
            }
        }
    }
}
