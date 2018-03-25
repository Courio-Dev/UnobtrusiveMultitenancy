using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Puzzle.Core.Multitenancy.Internal.Middlewares;
using Puzzle.Core.Multitenancy.Internal.Resolvers;
using System;

namespace Puzzle.Core.Multitenancy.Extensions
{
    internal static class MultitenancyApplicationBuilderExtensions
    {
        internal static IApplicationBuilder UseMultitenancy<TTenant>(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException($"Argument {nameof(app)} must not be null");

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger<TenantResolutionMiddleware<TTenant>>();
                ITenantResolver<TTenant> tenantResolver = serviceScope.ServiceProvider.GetRequiredService<ITenantResolver<TTenant>>();
                app.Use(next => new TenantResolutionMiddleware<TTenant>(next, logger, tenantResolver).Invoke);
                return app;
            }
        }
    }
}