namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Puzzle.Core.Multitenancy.Internal.Logging;
    using Puzzle.Core.Multitenancy.Internal.Middlewares;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;

    internal static class MultitenancyApplicationBuilderExtensions
    {
       /* internal static IApplicationBuilder UseMultitenancy<TTenant>(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException($"Argument {nameof(app)} must not be null");
            }

            using (IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ILog<TenantResolutionMiddleware<TTenant>> logger = app.ApplicationServices.GetRequiredService<ILog<TenantResolutionMiddleware<TTenant>>>();
                ITenantResolver<TTenant> tenantResolver = serviceScope.ServiceProvider.GetRequiredService<ITenantResolver<TTenant>>();
                app.Use(next => new TenantResolutionMiddleware<TTenant>(next, app, logger, tenantResolver).Invoke);
                return app;
            }
        }*/
        public static IApplicationBuilder UseMultitenancy<TTenant>(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException($"Argument {nameof(app)} must not be null");
            }
            return app.UseMiddleware<TenantResolutionMiddleware<TTenant>>();
        }
    }
}
