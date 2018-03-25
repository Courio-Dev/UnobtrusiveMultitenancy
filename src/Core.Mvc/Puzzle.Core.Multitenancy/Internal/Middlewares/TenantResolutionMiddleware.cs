namespace Puzzle.Core.Multitenancy.Internal.Middlewares
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using System;
    using System.Threading.Tasks;

    internal class TenantResolutionMiddleware<TTenant>
    {
        private readonly RequestDelegate next;
        private readonly ILogger<TenantResolutionMiddleware<TTenant>> logger;
        private readonly ITenantResolver<TTenant> tenantResolver;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware<TTenant>> logger, ITenantResolver<TTenant> tenantResolver)
        {
            this.next = next ?? throw new ArgumentNullException($"Argument {nameof(next)} must not be null");
            this.logger = logger ?? throw new ArgumentNullException($"Argument {nameof(logger)} must not be null");
            this.tenantResolver = tenantResolver ?? throw new ArgumentNullException($"Argument {nameof(tenantResolver)} must not be null");
        }

        public ILogger<TenantResolutionMiddleware<TTenant>> Logger => logger;

        public ITenantResolver<TTenant> TenantResolver => tenantResolver;

        public async Task Invoke(HttpContext context)
        {
            if (next == null) throw new ArgumentNullException($"Argument {nameof(next)} must not be null");
            if (TenantResolver == null) throw new ArgumentNullException($"Argument {nameof(TenantResolver)} must not be null");

            Logger.LogDebug("Resolving TenantContext using {loggerType}.", TenantResolver.GetType().Name);

            var tenantContext = await TenantResolver.ResolveAsync(context);

            if (tenantContext != null)
            {
                Logger.LogDebug("TenantContext Resolved. Adding to HttpContext.");
                context.SetTenantContext(tenantContext);
            }
            else
            {
                Logger.LogDebug("TenantContext Not Resolved.");
            }

            await next.Invoke(context);
        }
    }

    //public class TenantResolutionMiddleware<TTenant>
    //{
    //    private readonly RequestDelegate next;
    //    private readonly IApplicationBuilder rootApp;
    //    private readonly ILogger log;
    //    private readonly ITenantResolver<TTenant> tenantResolver;

    //    public TenantResolutionMiddleware(ILoggerFactory loggerFactory, RequestDelegate next, IApplicationBuilder rootApp, ITenantResolver<TTenant> tenantResolver)
    //    {
    //        if (loggerFactory == null) throw new ArgumentNullException($"Argument {nameof(loggerFactory)} must not be null");
    //        if (next == null) throw new ArgumentNullException($"Argument {nameof(next)} must not be null");
    //        if (rootApp == null) throw new ArgumentNullException($"Argument {nameof(rootApp)} must not be null");
    //        if (tenantResolver == null) throw new ArgumentNullException($"Argument {nameof(tenantResolver)} must not be null");

    //        this.next = next;
    //        this.rootApp = rootApp;
    //        this.log = loggerFactory.CreateLogger<TenantResolutionMiddleware<TTenant>>();
    //        this.tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    //    }

    //    public async Task Invoke(HttpContext context)
    //    {
    //        if (context == null) throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
    //        if (tenantResolver == null) throw new ArgumentNullException($"Argument {nameof(tenantResolver)} must not be null");

    //        log.LogDebug("Resolving TenantContext using {loggerType}.", tenantResolver.GetType().Name);
    //        var tenantContext = await tenantResolver.ResolveAsync(context);

    //        if (tenantContext != null)
    //        {
    //            log.LogDebug("TenantContext Resolved. Adding to HttpContext.");
    //            context.SetTenantContext(tenantContext);
    //        }
    //        else
    //        {
    //            log.LogDebug("TenantContext Not Resolved.");
    //        }

    //        await next.Invoke(context);
    //    }
    //}
}