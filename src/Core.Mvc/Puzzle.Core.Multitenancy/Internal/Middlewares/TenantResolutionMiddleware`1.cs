namespace Puzzle.Core.Multitenancy.Internal.Middlewares
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal.Logging;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;

    internal class TenantResolutionMiddleware<TTenant>
    {
        private readonly RequestDelegate next;
        private readonly IServiceScopeFactory scopeFactory;

        public TenantResolutionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            this.next = next ?? throw new ArgumentNullException($"Argument {nameof(next)} must not be null");
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException($"Argument {nameof(scopeFactory)} must not be null");
        }

        public async Task Invoke(
            HttpContext httpContext, 
            ILog<TenantResolutionMiddleware<TTenant>> logger, 
            ITenantResolver<TTenant> tenantResolver)
        {
            logger?.Debug($"Resolving TenantContext using {tenantResolver.GetType().Name}.");
            TenantContext<TTenant> tenantContext = await tenantResolver.ResolveAsync(httpContext).ConfigureAwait(false);

            if (tenantContext != null)
            {
                logger?.Debug("TenantContext Resolved. Adding to HttpContext.");
                httpContext?.SetTenantContext(tenantContext);
            }
            else
            {
                logger?.Warn("TenantContext Not Resolved.");
            }

            //using (logger.Info($"Tenant:{httpContext.GetTenant<TTenant>()}"))
            {
                await next.Invoke(httpContext).ConfigureAwait(false);
            }
        }
    }
}
