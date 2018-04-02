namespace Microsoft.AspNetCore.Builder
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Middlewares;
    using Puzzle.Core.Multitenancy.Internal.Options;

    public static class UsePerTenantApplicationBuilderExtensions
    {
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
            IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy = app.ApplicationServices.GetRequiredService<IServiceFactoryForMultitenancy<TTenant>>();
            app.Use(next => new TenantPipelineMiddleware<TTenant>(next, app, configuration, optionsMonitor, serviceFactoryForMultitenancy).Invoke);
            return app;
        }
    }
}
