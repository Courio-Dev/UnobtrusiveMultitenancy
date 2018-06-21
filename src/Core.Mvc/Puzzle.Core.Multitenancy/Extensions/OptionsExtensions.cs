namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Constants;
    using Puzzle.Core.Multitenancy.Internal;
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
        /// <typeparam name="TTenant">Tenant object.</typeparam>
        internal static IServiceCollection AddMultitenancyOptions<TTenant>(this IServiceCollection services, MultiTenancyConfig<TTenant> multitenancyConfig)
        {
            if (services == null)
            {
                throw new ArgumentNullException($"Argument {nameof(services)} must not be null");
            }

            if (multitenancyConfig == null)
            {
                throw new ArgumentNullException($"Argument {nameof(multitenancyConfig)} must not be null");
            }

            services.Configure<MultitenancyOptions<TTenant>>(multitenancyConfig.Config.GetSection(nameof(MultitenancyConstants.MultitenancyOptions)));
            services.AddSingleton<IPostConfigureOptions<MultitenancyOptions<TTenant>>, MultitenancyPostConfigureOptions<TTenant>>();
            services.AddSingleton(sp => sp.GetService<IOptionsMonitor<MultitenancyOptions<TTenant>>>().CurrentValue);

            services.AddSingleton<IMultitenancyOptionsProvider<TTenant>>(sp=> new MultitenancyOptionsProvider<TTenant>(multitenancyConfig));

            return services;
        }
    }
}
