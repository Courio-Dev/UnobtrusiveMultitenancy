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
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="defaultMultitenancyConfiguration"></param>
        public MultiTenancyConfig(string environment,IConfigurationRoot defaultMultitenancyConfiguration = null)
        {
            Environment = environment;
            DefaultMultitenancyConfiguration = defaultMultitenancyConfiguration;
            Config = BuildConfiguration(Ds, environment, defaultMultitenancyConfiguration);
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
        /// Environment name.
        /// </summary>
        public string Environment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IChangeToken ChangeTokenConfiguration => Config.GetReloadToken();

        /// <summary>
        ///     The type of context that these options are for (<typeparamref name="TTenant" />).
        /// </summary>
        public Type TenantType => typeof(TTenant);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IChangeToken ReloadConfiguration()
        {
            lock (SyncLock)
            {
                Config = BuildConfiguration(Ds, Environment, DefaultMultitenancyConfiguration);
            }
            return (Config.GetReloadToken());
        }

        /// <summary>
        /// 
        /// </summary>
        protected char Ds => System.IO.Path.DirectorySeparatorChar;


            

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
