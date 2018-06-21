﻿namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Logging;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal class AppTenantResolver : MemoryCacheTenantResolver<AppTenant>
    {
        private readonly ILog<AppTenantResolver> logger;

        public AppTenantResolver(
            IMultitenancyOptionsProvider<AppTenant> multitenancyOptionsProvider,
            IMemoryCache cache,
            ILog<AppTenantResolver> logger)
            : base(multitenancyOptionsProvider,cache, logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        protected override string GetContextIdentifier(HttpContext context) => context.Request.Host.Value.ToLower();

        /// <inheritdoc />
        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<AppTenant> context)=> context.Tenant.Hostnames;
        

        protected override Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<AppTenant> tenantContext = null;

            AppTenant tenant = Tenants.FirstOrDefault(t => t.Hostnames.Any(h => h.Equals(GetContextIdentifier(context))));

            if (tenant != null)
            {
                tenantContext = new TenantContext<AppTenant>(tenant);
            }

            return Task.FromResult(tenantContext);
        }

        protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return base.CreateCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
        }
    }
}
