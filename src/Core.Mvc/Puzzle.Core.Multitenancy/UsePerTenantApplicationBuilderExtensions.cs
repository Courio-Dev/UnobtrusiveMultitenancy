namespace Microsoft.AspNetCore.Builder
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Logging;
    using Puzzle.Core.Multitenancy.Internal.Middlewares;
    using Puzzle.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// Extensions to use UsePerTenant.
    /// </summary>
    public static class UsePerTenantApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure action per tenant.
        /// </summary>
        /// <typeparam name="TTenant">Tenant object.</typeparam>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request.</param>
        /// <param name="configuration">Define a configuration per tenant.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder UsePerTenant<TTenant>(this IApplicationBuilder app, Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException($"Argument {nameof(app)} must not be null");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException($"Argument {nameof(configuration)} must not be null");
            }

            IOptionsMonitor<MultitenancyOptions> optionsMonitor = app.ApplicationServices.GetRequiredService<IOptionsMonitor<MultitenancyOptions>>();
            ILog<TenantPipelineMiddleware<TTenant>> logger = app.ApplicationServices.GetRequiredService<ILog<TenantPipelineMiddleware<TTenant>>>();
            IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy = app.ApplicationServices.GetRequiredService<IServiceFactoryForMultitenancy<TTenant>>();
            app.Use(next => new TenantPipelineMiddleware<TTenant>(next, app, configuration, optionsMonitor, logger, serviceFactoryForMultitenancy).Invoke);
            return app;
        }
    }
}
