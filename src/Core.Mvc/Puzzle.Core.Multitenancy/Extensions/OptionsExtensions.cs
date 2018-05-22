namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal static class OptionsExtensions
    {
        /// <summary>
        /// Add MultitenanCy Options.
        /// </summary>
        /// <param name="services">An IServiceCollection.</param>
        /// <param name="multitenancyConfig">The object which containd multitenant config.</param>
        /// <returns>IServiceCollection.</returns>
        internal static IServiceCollection AddMultitenancyOptions(this IServiceCollection services, MultiTenancyConfig multitenancyConfig)
        {
            if (services == null)
            {
                throw new ArgumentNullException($"Argument {nameof(services)} must not be null");
            }

            // services.AddSingleton<IOptionsMonitor<MultitenancyOptions>, MonitorMultitenancyOptions>();
            services.Configure<MultitenancyOptions>(multitenancyConfig.Config.GetSection(nameof(MultitenancyOptions)));

            // services.AddSingleton<IConfigureOptions<MultitenancyOptions>, ConfigureMultitenancyOptionsSetup>();

            // services.Configure<MultitenancyOptions>(multitenancyConfig.Config.GetSection(nameof(MultitenancyOptions)));
            // services.AddScoped(cfg => cfg.GetService<IOptionsMonitor<MultitenancyOptions>>().CurrentValue);
            // services.AddSingleton<IOptionsMonitor<MultitenancyOptions>, MonitorMultitenancyOptions>();
            services.AddSingleton<IPostConfigureOptions<MultitenancyOptions>, MultitenancyPostConfigureOptions>();

            return services;
        }
    }
}
