using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Puzzle.Core.Multitenancy.Internal.Options;
using System;

namespace Puzzle.Core.Multitenancy.Internal.Configurations
{
    internal class MultiTenancyConfig
    {
        private readonly char DS = System.IO.Path.DirectorySeparatorChar;
        private readonly string environment;
        public readonly IConfiguration Config;

        public MultiTenancyConfig(string environment, IConfiguration multitenancyConfiguration = null)
        {
            this.environment = environment;
            this.Config = BuildConfiguration(DS, environment, multitenancyConfiguration);
        }

        public MultiTenancyConfig(IHostingEnvironment env) => throw new NotImplementedException();

        public MultitenancyOptions CurrentMultiTenacyOptionsValue => Config.GetSection(nameof(MultitenancyOptions)).Get<MultitenancyOptions>();

        //public string Version => Config["version"] ?? "x.x.x.x";
        //public string DataFolder => (Config["DataFolder"] ?? "App_Data/Data").Replace("/", "\\");
        private static IConfiguration BuildConfiguration(char ds, string environment, IConfiguration multitenancyConfiguration = null)
        {
            const string configDirectory = "Configs";
            const string hosting = nameof(hosting);

            IConfiguration BuildDefaultConfiguration(string envName)
            {
                var configurationBuilder = HostingStartupConfigurationExtensions.GetBaseConfigurationBuilder();
                var config = configurationBuilder
                    .AddJsonFile($"{nameof(MultitenancyOptions)}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{nameof(MultitenancyOptions)}.{envName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{configDirectory}{ds}{nameof(MultitenancyOptions)}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{configDirectory}{ds}{nameof(MultitenancyOptions)}.{envName}.json", optional: true, reloadOnChange: true)
                    .Build()
                    ;

                return config;
            }

            return multitenancyConfiguration ?? (multitenancyConfiguration = BuildDefaultConfiguration(environment));
        }

        protected internal static class HostingStartupConfigurationExtensions
        {
            public static IConfigurationBuilder GetBaseConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                    ;
            }
        }
    }
}