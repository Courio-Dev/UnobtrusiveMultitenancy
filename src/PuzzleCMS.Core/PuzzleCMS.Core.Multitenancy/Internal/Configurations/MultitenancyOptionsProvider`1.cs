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
    using Microsoft.Extensions.Options;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;

    /// <inheritdoc/>
    /// <typeparam name="TTenant"></typeparam>
    public class MultitenancyOptionsProvider<TTenant> : IMultitenancyOptionsProvider<TTenant>
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

            ConfigureServicesTenantList = new List<Action<IServiceCollection, TTenant>>();
            LogProvider = (sc, t, conf) => default;
        }

        /// <summary>
        /// Specific tenant log provider.
        /// </summary>
        protected Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> LogProvider { get; private set; }

        /// <inheritdoc/>
        public MultitenancyOptions<TTenant> MultitenancyOptions
        {
            get
            {
                return options ?? (options = FinishBuildOptions(ServiceProvider, MultiTenancyConfig.Config));
            }
        }

        /// <inheritdoc/>
        public MultiTenancyConfig<TTenant> MultiTenancyConfig { get; }

        /// <inheritdoc/>
        public IChangeToken ChangeTokenConfiguration => MultiTenancyConfig.ChangeTokenConfiguration;


        /// <summary>
        /// The current service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <inheritdoc/>
        public bool HasTenants => (MultitenancyOptions?.Tenants?.Any() ?? false);

        /// <summary>
        /// List additionnal services to specific tenants.
        /// </summary>
        public IList<Action<IServiceCollection, TTenant>> ConfigureServicesTenantList { get; private set; }



        /// <summary>
        /// Specific tenant log provider.
        /// </summary>
        public Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> TenantLogProvider
        {
            get
            {
                return BuildTenantLogProvider();
            }
        }


        /// <summary>
        /// Set Tenant LogProvider.
        /// </summary>
        /// <param name="func"></param>
        internal void SetTenantLogProvider(Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> func)
        {
            if (func != null)
            {
                LogProvider = func;
            }
        }

        /// <summary>
        /// Add additionnal ConfigureServices for specific tenant.
        /// </summary>
        /// <param name="action">Action to configure services.</param>
        internal void AddServicesTenant(Action<IServiceCollection, TTenant> action)
        {
            if (action != null)
            {
                ConfigureServicesTenantList?.Add(action);
            }
        }

        /// <summary>
        /// Build additionals configure services for each tenant.
        /// </summary>
        /// <returns></returns>
        internal Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> BuildTenantLogProvider()
        {
            // https://stackoverflow.com/questions/2559807/how-do-i-combine-several-actiont-into-a-single-actiont-in-c
            // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/how-to-combine-delegates-multicast-delegates
            Action<IServiceCollection, TTenant> action = (sc, tenant) => { };
            foreach (Action<IServiceCollection, TTenant> singleAction in ConfigureServicesTenantList)
            {
                action += singleAction;
            }

            ILogProvider func(IServiceCollection sc, TTenant t, IConfiguration conf)
            {
                action(sc, t);
                return LogProvider(sc, t, conf);
            }


            return func;
        }

        /// <inheritdoc/>
        public void Reload()
        {
            OnReloadConfig(null);
        }

        private readonly object SyncLock = new object();
        private void OnReloadConfig(object state)
        {
            lock (SyncLock)
            {
                Console.WriteLine($" {ChangeTokenConfiguration} Configuration changed. ");
                IChangeToken token = MultiTenancyConfig.ReloadConfiguration();
                options = null;
                // The token will change each time it reloads, so we need to register again.
                token?.RegisterChangeCallback(OnReloadConfig, null);
            }
        }

        /// <summary>
        /// Finish build options.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private MultitenancyOptions<TTenant> FinishBuildOptions(IServiceProvider serviceProvider, IConfigurationRoot config)
        {
            IConfigurationSection multitenantConfiguration = config.GetSection(nameof(MultitenancyConstants.MultitenancyOptions));

            MultitenancyOptions<TTenant> internalOptions = ActivatorUtilities.
                                                           GetServiceOrCreateInstance<IOptionsMonitor<MultitenancyOptions<TTenant>>>(serviceProvider).
                                                           CurrentValue;
            internalOptions.TenantsConfigurations = multitenantConfiguration.GetSection(MultitenancyConstants.Tenants)?.GetChildren();


            return internalOptions;
        }


    }
}
