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
using System;
using System.Linq;
using System.Reflection;

namespace Puzzle.Core.Multitenancy.Extensions
{
    /// <summary>
    /// Extensions methos for add multitenancy in new or existings applications.
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup>
            (this IWebHostBuilder hostBuilder, IConfiguration multitenancyConfiguration)
            where TStartup : class
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, AppTenant, CachingAppTenantResolver>(
                typeof(TStartup), multitenancyConfiguration
            );
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <typeparam name="TTenant"></typeparam>
        /// <typeparam name="TResolver"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="multitenancyConfiguration"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>
            (this IWebHostBuilder hostBuilder, IConfiguration multitenancyConfiguration = null)
                where TStartup : class
                where TTenant : class
                where TResolver : class, ITenantResolver<TTenant>

        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(
                typeof(TStartup),
                multitenancyConfiguration: multitenancyConfiguration
            );
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseUnobtrusiveMulitenancyStartupWithDefaultConvention<TStartup>(this IWebHostBuilder hostBuilder)
        where TStartup : class
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, AppTenant, CachingAppTenantResolver>(
                typeof(TStartup), multitenancyConfiguration: null
            );
        }

        /// <summary>
        /// Specify the startup type to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="startupType">The <see cref="Type"/> to be used.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        private static IWebHostBuilder UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>
            (this IWebHostBuilder hostBuilder,
            Type startupType,
            IConfiguration multitenancyConfiguration = null)
                where TStartup : class
                where TTenant : class
                where TResolver : class, ITenantResolver<TTenant>

        {
            var environment = hostBuilder.GetSetting("environment");
            var multitenancyConfig = new MultiTenancyConfig(environment, multitenancyConfiguration);

            if (multitenancyConfig.Config == null) throw new ArgumentNullException(nameof(multitenancyConfig));

            var buildedOptions = multitenancyConfig.CurrentMultiTenacyOptionsValue;

            if (!(buildedOptions?.Tenants?.Any() ?? false))
            {
                return hostBuilder.UseStartup<TStartup>();
            }
            else
            {
                var startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;

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
                        services.Configure<MultitenancyOptions>(multitenancyConfig.Config.GetSection(nameof(MultitenancyOptions)));
                        services.AddScoped(cfg => cfg.GetService<IOptionsMonitor<MultitenancyOptions>>().CurrentValue);
                        services.AddMultitenancy<TTenant, TResolver>();

                        //manage IStartup
                        services.RemoveAll<IStartup>();

                        var staruptDescriptor = new ServiceDescriptor(typeof(IStartup), (IServiceProvider provider) =>
                        {
                            var hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                            var methods = StartupLoaderMultitenant.LoadMethods<TTenant>(provider, startupType, hostingEnvironment.EnvironmentName);
                            return new ConventionMultitenantBasedStartup<TTenant>(methods);
                        }, ServiceLifetime.Singleton);
                        services.Add(staruptDescriptor);

                        var descriptor = new ServiceDescriptor(typeof(IStartupFilter), sp =>
                        {
                            var serviceFactoryForMultitenancy = sp.GetRequiredService<IServiceFactoryForMultitenancy<TTenant>>();
                            return new MultitenantRequestStartupFilter<TStartup, TTenant>(serviceFactoryForMultitenancy);
                        }, ServiceLifetime.Transient);
                        services.Insert(0, descriptor);
                    }
                })
                ;
            }
        }
    }
}