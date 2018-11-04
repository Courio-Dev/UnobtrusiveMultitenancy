namespace PuzzleCMS.Core.Multitenancy.Extensions
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
    using PuzzleCMS.Core.Multitenancy.Constants;
    using PuzzleCMS.Core.Multitenancy.Internal;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;
    using PuzzleCMS.Core.Multitenancy.Internal.Resolvers;
    using PuzzleCMS.Core.Multitenancy.Internal.StartupFilters;

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
            bool throwErrorIfOptionsNotFound = true,
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
            Action<MultiTenancyConfig<AppTenant>> actionConfiguration = null)
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
            const string environment= nameof(environment);
            string env = hostBuilder.GetSetting(environment);

            hostBuilder.BuildTemporaryMulitenancyProviderAndValidate<TTenant>(env, throwErrorIfOptionsNotFound, out bool hasTenants, multitenancyConfiguration);
            if (!hasTenants && !throwErrorIfOptionsNotFound)
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
                    // Register feault services for multitenancy.
                    services.AddPuzzleCMSMulitenancyCore<TTenant>(env, multitenancyConfiguration);

                    services.AddMultitenancy<TTenant, TResolver>();           

                    // Manage IStartup.
                    services.RemoveAll<IStartup>();

                    //
                    ServiceDescriptor descriptor = BuildMultitenantRequestStartupFilter<TStartup, TTenant>();
                    services.Insert(0, descriptor);

                    //
                    ServiceDescriptor startuptDescriptor = BuildConventionMultitenantBasedStartup<TTenant>(startupType);
                    services.Insert(1, startuptDescriptor);
                });
            }
        }

        private static ServiceDescriptor BuildConventionMultitenantBasedStartup<TTenant>(Type startupType)
            where TTenant : class => new ServiceDescriptor(
                                typeof(IStartup),
                                (IServiceProvider provider) =>
                                {
                                    IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                                    StartupMethodsMultitenant<TTenant> methods = StartupLoaderMultitenant.LoadMethods<TTenant>(provider, startupType, hostingEnvironment.EnvironmentName);
                                    MultiTenancyConfig<TTenant> multitenancyConfig= provider.GetRequiredService<MultiTenancyConfig<TTenant>>();
                                    return new ConventionMultitenantBasedStartup<TTenant>(methods, multitenancyConfig.BuildTenantLogProvider());
                                }, lifetime: ServiceLifetime.Singleton);

        private static ServiceDescriptor BuildMultitenantRequestStartupFilter<TStartup, TTenant>()
            where TStartup : class
            where TTenant : class => new ServiceDescriptor(
                                    typeof(IStartupFilter),
                                    sp => new MultitenantRequestStartupFilter<TStartup, TTenant>(),
                                    ServiceLifetime.Transient);
    }
}
