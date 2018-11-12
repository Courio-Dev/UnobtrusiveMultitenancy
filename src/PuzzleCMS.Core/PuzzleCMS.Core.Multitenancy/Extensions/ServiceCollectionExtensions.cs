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
using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;
using PuzzleCMS.Core.Multitenancy.Internal.Options;

namespace PuzzleCMS.Core.Multitenancy.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="environment"></param>
        /// <param name="throwErrorIfOptionsNotFound"></param>
        /// <param name="hasTenants"></param>
        /// <param name="defaultMultitenancyConfiguration"></param>
        /// <param name="optionsAction">additionnal options for config MultitenancyOptionsProviderBuilder.</param>
        /// <returns></returns>
        internal static void GetTemporaryMulitenancyProviderAndValidate<TTenant>(
            this IWebHostBuilder hostBuilder,
            string environment,
            bool throwErrorIfOptionsNotFound,
            IConfigurationRoot defaultMultitenancyConfiguration,
            Action<IServiceProvider, MultitenancyOptionsProviderBuilder<TTenant>> optionsAction,
            out bool hasTenants)
        {
            ServiceCollection services = new ServiceCollection();
            AddPuzzleCMSMulitenancyCore<TTenant>(services, environment, defaultMultitenancyConfiguration, optionsAction);


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
                throw new ArgumentNullException($"Argument {nameof(multitenantProvider)} must not be null.");
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
        /// <param name="optionsAction">additionnal options for config MultitenancyOptionsProviderBuilder.</param>
        /// <returns></returns>
        internal static void AddPuzzleCMSMulitenancyCore<TTenant>(
            this IServiceCollection services,
            string environment,
            IConfigurationRoot defaultMultitenancyConfiguration,
            Action<IServiceProvider, MultitenancyOptionsProviderBuilder<TTenant>> optionsAction)
        {
            AddDefaultPuzzleCMSServices<TTenant>(
                serviceCollection: services,
                environment: environment,
                defaultMultitenancyConfiguration: defaultMultitenancyConfiguration,
                optionsAction: optionsAction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="environment"></param>
        /// <param name="defaultMultitenancyConfiguration"></param>
        /// <param name="optionsAction">additionnal options for config MultitenancyOptionsProviderBuilder.</param>
        private static void AddDefaultPuzzleCMSServices<TTenant>(
            IServiceCollection serviceCollection,
            string environment,
            IConfigurationRoot defaultMultitenancyConfiguration,
            Action<IServiceProvider, MultitenancyOptionsProviderBuilder<TTenant>> optionsAction)
        {
            // Register IHttpContextAccessor.
            serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            MultiTenancyConfig multitenancyConfig = new MultiTenancyConfig(environment, defaultMultitenancyConfiguration);

            // Register MultiTenancyConfig.
            serviceCollection.AddSingleton(multitenancyConfig);

            // Add config.
            serviceCollection.AddSingleton<MultiTenancyConfig>(serviceProvider => multitenancyConfig);

            // Add logging
            MultitenancyOptionsProviderBuilderExtensions.SetDefaultLogProvider();
            serviceCollection.TryAdd(ServiceDescriptor.Singleton<ILogProvider>(LogProvider.CurrentLogProvider));
            serviceCollection.TryAdd(ServiceDescriptor.Singleton(typeof(ILog<>), typeof(Log<>)));

            // Register multitenancy options.
            AddMultitenancyOptions<TTenant>(serviceCollection, multitenancyConfig, optionsAction);
        }

        /// <summary>
        /// Add MultitenanCy Options.
        /// </summary>
        /// <param name="serviceCollection">An IServiceCollection.</param>
        /// <param name="multitenancyConfig">The object which containd multitenant config.</param>
        /// <param name="optionsAction">additionnal options for config MultitenancyOptionsProviderBuilder.</param>
        /// <returns>IServiceCollection.</returns>
        /// <typeparam name="TTenant">Tenant object.</typeparam>
        internal static void AddMultitenancyOptions<TTenant>(
            this IServiceCollection serviceCollection,
            MultiTenancyConfig multitenancyConfig,
            Action<IServiceProvider, MultitenancyOptionsProviderBuilder<TTenant>> optionsAction)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException($"Argument {nameof(serviceCollection)} must not be null");
            }

            if (multitenancyConfig == null)
            {
                throw new ArgumentNullException($"Argument {nameof(multitenancyConfig)} must not be null");
            }

            serviceCollection.AddOptions();
            serviceCollection.Configure<MultitenancyOptions<TTenant>>(multitenancyConfig.Config.GetSection(nameof(MultitenancyConstants.MultitenancyOptions)));
            serviceCollection.AddSingleton<IPostConfigureOptions<MultitenancyOptions<TTenant>>, MultitenancyPostConfigureOptions<TTenant>>();

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(IMultitenancyOptionsProvider<TTenant>),
                    p => MultitenancyOptionsProviderFactory<TTenant>(p, optionsAction),
                    ServiceLifetime.Singleton));
        }

        private static IMultitenancyOptionsProvider<TTenant> MultitenancyOptionsProviderFactory<TTenant>(
           IServiceProvider sp,
           Action<IServiceProvider, MultitenancyOptionsProviderBuilder<TTenant>> optionsAction)
        {

            MultitenancyOptionsProviderBuilder<TTenant> builder = new MultitenancyOptionsProviderBuilder<TTenant>(
                new ServiceCollection(),
                new MultitenancyOptionsProvider<TTenant>(sp, sp.GetRequiredService<MultiTenancyConfig>()));

            // Add default logging provider i.e ColoredConsoleLogProvider.
            builder.UseColoredConsoleLogProvider();

            //// builder.UseApplicationServiceProvider(applicationServiceProvider);
            optionsAction?.Invoke(sp, builder);
            
            return builder.BuildOptionsProviderBuilder();
        }

        
    }
}
