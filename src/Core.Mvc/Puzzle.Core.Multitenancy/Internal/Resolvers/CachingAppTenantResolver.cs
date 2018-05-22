namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Logging;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal class CachingAppTenantResolver : MemoryCacheTenantResolver<AppTenant>
    {
        // private readonly IEnumerable<AppTenant> Tenants;
        private readonly IOptionsMonitor<MultitenancyOptions> optionsMonitor;
        private readonly ILog<CachingAppTenantResolver> logger;

        public CachingAppTenantResolver(
            IMemoryCache cache,
            ILog<CachingAppTenantResolver> logger,
            IOptionsMonitor<MultitenancyOptions> optionsMonitor)
            : base(cache, logger)
        {
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // this.Tenants = this.optionsMonitor.CurrentValue.Tenants;
            this.optionsMonitor.OnChange(vals =>
            {
                // TODO : find a way to clear a cache.
                logger.Debug($"Config changed: {string.Join(", ", vals)}");
            });
        }

        protected IEnumerable<AppTenant> Tenants => optionsMonitor.CurrentValue.Tenants;

        protected override string GetContextIdentifier(HttpContext context)
        {
            return context.Request.Host.Value.ToLower();
        }

        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<AppTenant> context)
        {
            return context.Tenant.Hostnames;
        }

        protected override Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<AppTenant> tenantContext = null;

            AppTenant tenant = Tenants.FirstOrDefault(t =>
                t.Hostnames.Any(h => h.Equals(context.Request.Host.Value.ToLower())));

            if (tenant != null)
            {
                tenantContext = new TenantContext<AppTenant>(tenant);
            }

            return Task.FromResult(tenantContext);
        }

        protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return base.CreateCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
        }
    }
}
