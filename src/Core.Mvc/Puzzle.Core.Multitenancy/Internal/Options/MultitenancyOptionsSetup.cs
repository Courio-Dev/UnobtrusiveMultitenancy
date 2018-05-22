namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Configurations;

    /// <summary>
    /// https://shazwazza.com/post/using-aspnet5-optionsmodel/
    /// https://github.com/Shazwazza/Smidge/blob/master/src/Smidge/Options/SmidgeOptionsSetup.cs
    /// https://rimdev.io/strongly-typed-configuration-settings-in-asp-net-core-part-ii/
    /// http://henkmollema.github.io/advanced-options-configuration-in-asp.net-core/
    /// https://andrewlock.net/access-services-inside-options-and-startup-using-configureoptions/
    /// http://blog.soat.fr/2015/09/asp-net-5-la-librairie-optionsmodel/.
    /// </summary>
    internal class MultitenancyOptionsSetup : ConfigureOptions<MultitenancyOptions>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public MultitenancyOptionsSetup(
            IServiceScopeFactory serviceScopeFactory,
            IServiceProvider serviceProvider,
            MultiTenancyConfig config)
            : base(options => ConfigureMultitenancyOptions(options, OrSection(config, nameof(MultitenancyOptions))))
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.Config == null)
            {
                throw new ArgumentNullException(nameof(config.Config));
            }

            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        /// <summary>
        /// Set the default options.
        /// </summary>
        /// <param name="options">The MultitenancyOptions.</param>
        /// <param name="config">The configuration which contains MultitenancyOptions.</param>
        public static void ConfigureMultitenancyOptions(MultitenancyOptions options, IConfiguration config)
        {
            // then we set the properties
            new ConfigureFromConfigurationOptions<MultitenancyOptions>(config).Configure(options);
        }

        /// <summary>
        /// Allows for configuring the options instance before options are set.
        /// </summary>
        /// <param name="options">The MultitenancyOptions.</param>
        public override void Configure(MultitenancyOptions options)
        {
            base.Configure(options);
            /*
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                using (var dbContext = provider.GetRequiredService<ApplicationDbContext>())
                {
                    options.AppTenants = dbContext.AppTenants.ToList();
                }
            }
            */
        }

        private static IConfigurationSection OrSection(MultiTenancyConfig config, string key)
        {
            return config.Config as IConfigurationSection ?? config.Config.GetSection(key);
        }
    }
}
