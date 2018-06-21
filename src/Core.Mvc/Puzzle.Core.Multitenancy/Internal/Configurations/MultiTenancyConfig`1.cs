namespace Puzzle.Core.Multitenancy.Internal.Configurations
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using Puzzle.Core.Multitenancy.Constants;
    using Puzzle.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    public class MultiTenancyConfig<TTenant>
    {
        private readonly string environment;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="multitenancyConfiguration"></param>
        public MultiTenancyConfig(string environment, IConfigurationRoot multitenancyConfiguration = null)
        {
            this.environment = environment;
            Config = BuildConfiguration(Ds, environment, multitenancyConfiguration);
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
        public MultitenancyOptions<TTenant> CurrentMultiTenacyOptionsValue => Config.
                                                                                    GetSection(nameof(MultitenancyConstants.MultitenancyOptions)).
                                                                                    Get<MultitenancyOptions<TTenant>>();
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
            //Config.Reload();
            Config = BuildConfiguration(Ds, environment);

            return (options : CurrentMultiTenacyOptionsValue, token: Config.GetReloadToken());
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
