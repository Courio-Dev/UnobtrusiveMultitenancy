namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using Xunit;
    using static PuzzleCMS.UnitsTests.Base.MultitenancyBaseFixture;

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
                                                       "/tenant-1-3"
                                                   }
                                                },
                                               new TestTenant()
                                                {
                                                    Name = "Tenant 2", Hostnames = new string[]
                                                    {
                                                       "/tenant-2-1",
                                                       "/tenant-2-1",
                                                       "/tenant-2-1"
                                                   }
                                                },
                                               new TestTenant()
                                                {
                                                    Name = "Tenant 2", Hostnames = new string[]
                                                    {
                                                    }
                                                }
                                           };

        private readonly int cacheExpirationInSeconds;

        public TestTenantMemoryCacheResolver(IMemoryCache cache, ILoggerFactory loggerFactory, int cacheExpirationInSeconds = 10)
           : this(cache, loggerFactory, new MemoryCacheTenantResolverOptions(), cacheExpirationInSeconds)
        {
        }

        public TestTenantMemoryCacheResolver(IMemoryCache cache, ILoggerFactory loggerFactory, MemoryCacheTenantResolverOptions options, int cacheExpirationInSeconds = 10)
            : base(cache, loggerFactory, options)
        {
            this.cacheExpirationInSeconds = cacheExpirationInSeconds;
        }

        protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(cacheExpirationInSeconds));
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
                 tenantContext = new TenantContext<TestTenant>(tenant);

                tenantContext.Properties.Add("Created", DateTime.UtcNow);
            }

            return Task.FromResult(tenantContext);
        }
    }
}
