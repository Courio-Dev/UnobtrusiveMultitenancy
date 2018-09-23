namespace PuzzleCMS.Core.Multitenancy.Internal.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using PuzzleCMS.Core.Multitenancy.Constants;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    public class MultiTenancyConfig<TTenant>
    {
        private readonly object SyncLock = new object();

        /// <summary>
        /// Indicate if post configure neede.
        /// </summary>
        protected bool IsPostConfigureNeeded = true;

        internal IList<Action<IServiceCollection, TTenant>> ConfigureServicesTenantList { get; } = new List<Action<IServiceCollection, TTenant>>();

        internal Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> TenantLogProvider { get; private set; } = (sc, t, conf) => default;

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="defaultMultitenancyConfiguration"></param>
        /// <param name="setups">The configuration actions to run.</param>
        /// <param name="postConfigures">The initialization actions to run.</param>
        public MultiTenancyConfig(string environment,
            IConfigurationRoot defaultMultitenancyConfiguration = null,
            IEnumerable<IConfigureOptions<MultitenancyOptions<TTenant>>> setups = null,
            IEnumerable<IPostConfigureOptions<MultitenancyOptions<TTenant>>> postConfigures = null)
        {
            Environment = environment;
            DefaultMultitenancyConfiguration = defaultMultitenancyConfiguration;
            Config = BuildConfiguration(Ds, environment, defaultMultitenancyConfiguration);
            Setups = setups ?? Enumerable.Empty<IConfigureOptions<MultitenancyOptions<TTenant>>>();
            PostConfigures = postConfigures ?? Enumerable.Empty<IPostConfigureOptions<MultitenancyOptions<TTenant>>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        public MultiTenancyConfig(IHostingEnvironment env) => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        public IConfigurationRoot Config { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IConfigurationRoot DefaultMultitenancyConfiguration { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IConfigureOptions<MultitenancyOptions<TTenant>>> Setups { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IPostConfigureOptions<MultitenancyOptions<TTenant>>> PostConfigures { get; private set; }

        /// <summary>
        /// Environment name.
        /// </summary>
        public string Environment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MultitenancyOptions<TTenant> CurrentMultiTenacyOptionsValue
        {
            get
            {
                if (!IsPostConfigureNeeded)
                {
                    return storeCurrentMultiTenacyOptionsValue;
                 }
                else {
                    return (storeCurrentMultiTenacyOptionsValue=CreateOptions(Config, Options.DefaultName));
                }
            }
        }

        private MultitenancyOptions<TTenant> storeCurrentMultiTenacyOptionsValue;
        /// <summary>
        /// 
        /// </summary>
        public IChangeToken ChangeTokenConfiguration => Config.GetReloadToken();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (MultitenancyOptions<TTenant> options, IChangeToken token) ReloadConfiguration()
        {
            lock (SyncLock)
            {
                //Config.Reload();
                Config = BuildConfiguration(Ds, Environment, DefaultMultitenancyConfiguration);
            
                IsPostConfigureNeeded = true;
            }
            return (options: CurrentMultiTenacyOptionsValue, token: Config.GetReloadToken());
        }

        /// <summary>
        /// 
        /// </summary>
        protected char Ds => System.IO.Path.DirectorySeparatorChar;

        /// <summary>
        /// Add additionnal ConfigureServices for specific tenant.
        /// </summary>
        /// <param name="action">Action to configure services.</param>
        internal void SetConfigureServicesTenant(Action<IServiceCollection, TTenant> action)
        {
            if (action != null)
            {
                ConfigureServicesTenantList?.Add(action);
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
                TenantLogProvider = func;
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
                return TenantLogProvider(sc, t, conf);
            }


            return func;
    }
            

        private static IConfigurationRoot BuildConfiguration(char ds, string environment, IConfigurationRoot multitenancyConfiguration = null)
        {
            const string configDirectory = "Configs";
            const string hosting = nameof(hosting);


            IConfigurationRoot BuildDefaultConfiguration(string envName)
            {
                IConfigurationBuilder configurationBuilder = HostingStartupConfigurationExtensions.GetBaseConfigurationBuilder();
                IConfigurationRoot config = configurationBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"{nameof(MultitenancyConstants.MultitenancyOptions)}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{nameof(MultitenancyConstants.MultitenancyOptions)}.{envName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{configDirectory}{ds}{nameof(MultitenancyConstants.MultitenancyOptions)}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{configDirectory}{ds}{nameof(MultitenancyConstants.MultitenancyOptions)}.{envName}.json", optional: true, reloadOnChange: true)
                    //.AddEnvironmentVariables()
                    .Build()
                    ;

                return config;
            }

            return multitenancyConfiguration ?? (multitenancyConfiguration = BuildDefaultConfiguration(environment));
        }

        private MultitenancyOptions<TTenant> CreateOptions(IConfigurationRoot config, string name)
        {
            IConfigurationSection multitenantConfiguration = config.GetSection(nameof(MultitenancyConstants.MultitenancyOptions));
            MultitenancyOptions<TTenant> options = multitenantConfiguration.Get<MultitenancyOptions<TTenant>>();

            lock(SyncLock)
            {
                if (IsPostConfigureNeeded)
                {
                    IsPostConfigureNeeded = false;

                    if (options != null)
                    {
                        options.TenantsConfigurations = multitenantConfiguration.GetSection(MultitenancyConstants.Tenants)?.GetChildren();
                        foreach (IConfigureOptions<MultitenancyOptions<TTenant>> setup in Setups)
                        {
                            if (setup is IConfigureNamedOptions<MultitenancyOptions<TTenant>> namedSetup)
                            {
                                namedSetup.Configure(name, options);
                            }
                            else if (name == Options.DefaultName)
                            {
                                setup.Configure(options);
                            }
                        }
                        foreach (IPostConfigureOptions<MultitenancyOptions<TTenant>> post in PostConfigures)
                        {
                            post.PostConfigure(name, options);
                        }
                    }
                }
            }
            
            return options;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal static class HostingStartupConfigurationExtensions
        {
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static IConfigurationBuilder GetBaseConfigurationBuilder()
            {
                return new ConfigurationBuilder();
            }
        }
    }
}
