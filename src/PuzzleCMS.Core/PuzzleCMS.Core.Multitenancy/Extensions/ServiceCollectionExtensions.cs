using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PuzzleCMS.Core.Multitenancy.Constants;
using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
using PuzzleCMS.Core.Multitenancy.Internal.Logging;
using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
using PuzzleCMS.Core.Multitenancy.Internal.Options;

namespace PuzzleCMS.Core.Multitenancy.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <returns></returns>
        internal static void BuildTemporaryMulitenancyProviderAndValidate<TTenant>(
            this IWebHostBuilder hostBuilder, 
            string environment,
            bool throwErrorIfOptionsNotFound, 
            out bool hasTenants,
            IConfigurationRoot defaultMultitenancyConfiguration = null)
        {
            ServiceCollection services = new ServiceCollection();
            _ = AddPuzzleCMSMulitenancyCore<TTenant>(services, environment, defaultMultitenancyConfiguration);


            // Build an intermediate service provider
            using (ServiceProvider sp = services.BuildServiceProvider())
            {
                IMultitenancyOptionsProvider<TTenant> multitenancyOptionsProvider = sp.GetRequiredService<IMultitenancyOptionsProvider<TTenant>>();
                ValidateMultitenancyOptionsProvider(multitenancyOptionsProvider, throwErrorIfOptionsNotFound);
                hasTenants = multitenancyOptionsProvider.HasTenants;

                //clear
                services?.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="multitenantProvider"></param>
        /// <param name="throwErrorIfOptionsNotFound"></param>
        internal static void ValidateMultitenancyOptionsProvider<TTenant>(
            this IMultitenancyOptionsProvider<TTenant> multitenantProvider, bool throwErrorIfOptionsNotFound)
        {
            if (multitenantProvider == null)
            {
                throw new ArgumentNullException($"Argument {nameof(multitenantProvider)} must not be null");
            }

            MultitenancyOptions<TTenant> buildedOptions = multitenantProvider.MultitenancyOptions;
            if (throwErrorIfOptionsNotFound && (buildedOptions == null || !(buildedOptions?.Tenants?.Any() ?? false)))
            {
                Exception exception = new Exception("MultitenancyOptions not found in configuration.");
                throw exception;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        /// <param name="defaultMultitenancyConfiguration"></param>
        /// <returns></returns>
        internal static MultitenancyOptionsBuilder<TTenant> AddPuzzleCMSMulitenancyCore<TTenant>(
            this IServiceCollection services, string environment,IConfigurationRoot defaultMultitenancyConfiguration = null)
        {
            if (!(services
                .LastOrDefault(d => d.ServiceType == typeof(MultitenancyOptionsBuilder<TTenant>))?
                .ImplementationInstance is MultitenancyOptionsBuilder<TTenant> builder))
            {
                builder = new MultitenancyOptionsBuilder<TTenant>(services);
                services.AddSingleton(builder);

                AddDefaultPuzzleCMSServices<TTenant>(
                    serviceCollection: services, 
                    environment:environment,
                    defaultMultitenancyConfiguration:defaultMultitenancyConfiguration);
            }

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="environment"></param>
        /// <param name="defaultMultitenancyConfiguration"></param>
        private static void AddDefaultPuzzleCMSServices<TTenant>(
            IServiceCollection serviceCollection, string environment,IConfigurationRoot defaultMultitenancyConfiguration = null)
        {
            // Register IHttpContextAccessor.
            serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            MultiTenancyConfig<TTenant> multitenancyConfig = new MultiTenancyConfig<TTenant>(environment,defaultMultitenancyConfiguration);

            // Register MultiTenancyConfig<TTenant>.
            serviceCollection.AddSingleton(multitenancyConfig);

            // Override config.
            // Add logging
            multitenancyConfig.UseColoredConsoleLogProvider();
            
            serviceCollection.TryAdd(ServiceDescriptor.Singleton<ILogProvider>(LogProvider.CurrentLogProvider));
            serviceCollection.TryAdd(ServiceDescriptor.Singleton(typeof(ILog<>), typeof(Log<>)));
            serviceCollection.AddSingleton<MultiTenancyConfig<TTenant>>(serviceProvider => multitenancyConfig);

            // Register multitenancy options.
            AddMultitenancyOptions<TTenant>(serviceCollection, multitenancyConfig);
        }

        /// <summary>
        /// Add MultitenanCy Options.
        /// </summary>
        /// <param name="services">An IServiceCollection.</param>
        /// <param name="multitenancyConfig">The object which containd multitenant config.</param>
        /// <returns>IServiceCollection.</returns>
        /// <typeparam name="TTenant">Tenant object.</typeparam>
        private static void  AddMultitenancyOptions<TTenant>(IServiceCollection services, MultiTenancyConfig<TTenant> multitenancyConfig)
        {
            if (services == null)
            {
                throw new ArgumentNullException($"Argument {nameof(services)} must not be null");
            }

            if (multitenancyConfig == null)
            {
                throw new ArgumentNullException($"Argument {nameof(multitenancyConfig)} must not be null");
            }

            services.AddOptions();
            services.Configure<MultitenancyOptions<TTenant>>(multitenancyConfig.Config.GetSection(nameof(MultitenancyConstants.MultitenancyOptions)));
            services.AddSingleton<IPostConfigureOptions<MultitenancyOptions<TTenant>>, MultitenancyPostConfigureOptions<TTenant>>();
            services.AddSingleton(sp => sp.GetService<IOptionsMonitor<MultitenancyOptions<TTenant>>>().CurrentValue);

            services.AddSingleton<IMultitenancyOptionsProvider<TTenant>>(sp =>
            {
                return new MultitenancyOptionsProvider<TTenant>(sp,sp.GetRequiredService<MultiTenancyConfig<TTenant>>());
            });
        }
    }
}
