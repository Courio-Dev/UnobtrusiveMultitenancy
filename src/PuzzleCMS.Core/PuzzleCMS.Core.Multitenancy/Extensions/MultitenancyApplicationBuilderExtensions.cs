namespace PuzzleCMS.Core.Multitenancy.Extensions
{
    using System;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal.Middlewares;
    using PuzzleCMS.Core.Multitenancy.Internal.Resolvers;

    internal static class MultitenancyApplicationBuilderExtensions
    {
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
