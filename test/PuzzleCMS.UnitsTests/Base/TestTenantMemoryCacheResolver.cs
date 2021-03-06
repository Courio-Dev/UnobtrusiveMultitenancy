﻿namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using PuzzleCMS.Core.Multitenancy;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Resolvers;

    internal class TestTenantMemoryCacheResolver : MemoryCacheTenantResolver<TestTenant>
    {
        private readonly List<TestTenant> tenants = new List<TestTenant>()
                                           {
                                               new TestTenant()
                                                {
                                                    Name = "Tenant 1", Hostnames = new string[]
                                                    {
                                                       "/tenant-1-1",
                                                       "/tenant-1-2",
                                                       "/tenant-1-3",
                                                   },
                                                },
                                               new TestTenant()
                                                {
                                                    Name = "Tenant 2", Hostnames = new string[]
                                                    {
                                                       "/tenant-2-1",
                                                       "/tenant-2-1",
                                                       "/tenant-2-1",
                                                   },
                                                },
                                               new TestTenant()
                                                {
                                                    Name = "Tenant 2", Hostnames = new string[]
                                                    {
                                                    },
                                                },
                                           };

        private readonly int cacheExpirationInSeconds;

        public TestTenantMemoryCacheResolver(
            IMultitenancyOptionsProvider<TestTenant> optionsProvider,
            IMemoryCache cache, 
            ILog<TestTenantMemoryCacheResolver> logger, 
            int cacheExpirationInSeconds = 10)
           : this(optionsProvider,cache, logger, new MemoryCacheTenantResolverOptions(), cacheExpirationInSeconds)
        {
        }

        public TestTenantMemoryCacheResolver(
            IMultitenancyOptionsProvider<TestTenant> optionsProvider,
            IMemoryCache cache, 
            ILog log, 
            MemoryCacheTenantResolverOptions options, 
            int cacheExpirationInSeconds = 10)
            : base(optionsProvider,cache, log, options)
        {
            this.cacheExpirationInSeconds = cacheExpirationInSeconds;
        }

        protected override IEnumerable<TestTenant> Tenants => tenants;

        protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(cacheExpirationInSeconds));
        }

        protected override string GetContextIdentifier(HttpContext context)
        {
            return context.Request.Path;
        }

        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<TestTenant> context)
        {
            return context?.Tenant?.Hostnames;
        }

        protected override Task<TenantContext<TestTenant>> ResolveAsync(HttpContext context)
        {
            TestTenant tenant = tenants.FirstOrDefault(testTenant => testTenant.Hostnames.ToList().Contains(context.Request.Path));
            TenantContext<TestTenant> tenantContext = null;
            if (tenant != null)
            {
                int postion = GetTenantPositionWithPredicateResolver(context);
                tenantContext = new TenantContext<TestTenant>(tenant, postion);

                tenantContext.Properties.Add("Created", DateTime.UtcNow);
            }

            return Task.FromResult(tenantContext);
        }

        protected override Func<HttpContext, TestTenant, bool> PredicateResolver() => (c, t) => t.Hostnames.ToList().Contains(c.Request.Path);
    }
}
