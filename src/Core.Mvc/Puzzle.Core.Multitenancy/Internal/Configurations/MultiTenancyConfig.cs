namespace Puzzle.Core.Multitenancy.Internal.Configurations
{
    using System;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal class MultiTenancyConfig
    {
        private readonly string environment;

        public MultiTenancyConfig(string environment, IConfiguration multitenancyConfiguration = null)
        {
            this.environment = environment;
            Config = BuildConfiguration(Ds, environment, multitenancyConfiguration);
        }

        public MultiTenancyConfig(IHostingEnvironment env) => throw new NotImplementedException();

        public IConfiguration Config { get; private set; }

        public MultitenancyOptions CurrentMultiTenacyOptionsValue => Config.GetSection(nameof(MultitenancyOptions)).Get<MultitenancyOptions>();

        protected char Ds => System.IO.Path.DirectorySeparatorChar;

        // public string Version => Config["version"] ?? "x.x.x.x";
        // public string DataFolder => (Config["DataFolder"] ?? "App_Data/Data").Replace("/", "\\");
        private static IConfiguration BuildConfiguration(char ds, string environment, IConfiguration multitenancyConfiguration = null)
        {
            const string configDirectory = "Configs";
            const string hosting = nameof(hosting);

            IConfiguration BuildDefaultConfiguration(string envName)
            {
                IConfigurationBuilder configurationBuilder = HostingStartupConfigurationExtensions.GetBaseConfigurationBuilder();
                IConfigurationRoot config = configurationBuilder
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
