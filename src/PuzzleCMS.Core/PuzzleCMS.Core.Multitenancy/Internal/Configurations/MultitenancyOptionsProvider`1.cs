namespace PuzzleCMS.Core.Multitenancy.Internal.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Extensions.DependencyInjection;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;
    using System.Linq;
    using PuzzleCMS.Core.Multitenancy.Constants;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    internal class MultitenancyOptionsProvider<TTenant> : IMultitenancyOptionsProvider<TTenant>
    {
        private MultitenancyOptions<TTenant> options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="multiTenancyConfig"></param>
        public MultitenancyOptionsProvider(IServiceProvider serviceProvider, MultiTenancyConfig<TTenant> multiTenancyConfig)
        {
            ServiceProvider = serviceProvider;
            MultiTenancyConfig = multiTenancyConfig;
            ChangeTokenConfiguration?.RegisterChangeCallback(OnReloadConfig, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public MultitenancyOptions<TTenant> MultitenancyOptions
        {
            get
            {
                return options ??(options= FinishBuildOptions(ServiceProvider, MultiTenancyConfig.Config));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MultiTenancyConfig<TTenant> MultiTenancyConfig { get; }

        /// <summary>
        /// 
        /// </summary>
        public IChangeToken ChangeTokenConfiguration => MultiTenancyConfig.ChangeTokenConfiguration;


        /// <summary>
        /// The current service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        public bool HasTenants => (MultitenancyOptions?.Tenants?.Any() ?? false);

        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {
            OnReloadConfig(null);
        }

        private void OnReloadConfig(object state)
        {
            Console.WriteLine($" {ChangeTokenConfiguration} Configuration changed. ");
            IChangeToken token = MultiTenancyConfig.ReloadConfiguration();

            // The token will change each time it reloads, so we need to register again.
            token?.RegisterChangeCallback(OnReloadConfig, null);
        }

        /// <summary>
        /// Finish build options.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private MultitenancyOptions<TTenant> FinishBuildOptions(IServiceProvider serviceProvider,IConfigurationRoot config)
        {
            IConfigurationSection multitenantConfiguration = config.GetSection(nameof(MultitenancyConstants.MultitenancyOptions));

            MultitenancyOptions<TTenant> internalOptions = ActivatorUtilities.GetServiceOrCreateInstance<MultitenancyOptions<TTenant>>(serviceProvider);
            internalOptions.TenantsConfigurations = multitenantConfiguration.GetSection(MultitenancyConstants.Tenants)?.GetChildren();


            return internalOptions;
        }
    }
}
