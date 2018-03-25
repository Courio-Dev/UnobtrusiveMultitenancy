using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Puzzle.Core.Multitenancy.Internal.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    internal class CachingAppTenantResolver : MemoryCacheTenantResolver<AppTenant>
    {
        private readonly IEnumerable<AppTenant> tenants;
        private readonly IOptionsMonitor<MultitenancyOptions> optionsMonitor;

        public CachingAppTenantResolver(IMemoryCache cache, ILoggerFactory loggerFactory/*, MultitenancyOptions options*/, IOptionsMonitor<MultitenancyOptions> optionsMonitor)
            : base(cache, loggerFactory)
        {
            //if(options == null) throw new ArgumentException(nameof(options));
            //tenants = options.Tenants;
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            this.tenants = this.optionsMonitor.CurrentValue.Tenants;
            this.optionsMonitor.OnChange(vals =>
            {
                //TODO : find a way to clear a cache.
                loggerFactory.CreateLogger<CachingAppTenantResolver>().LogDebug($"Config changed: {string.Join(", ", vals)}");
            });
        }

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

            var tenant = tenants.FirstOrDefault(t =>
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