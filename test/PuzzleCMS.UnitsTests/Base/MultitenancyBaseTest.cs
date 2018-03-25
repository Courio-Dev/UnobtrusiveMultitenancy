using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Puzzle.Core.Multitenancy;
using Puzzle.Core.Multitenancy.Extensions;
using Puzzle.Core.Multitenancy.Internal.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PuzzleCMS.UnitsTests.Base
{
    public class MultitenancyBaseFixture
    {
        private ITenantResolver<TestTenant> MemoryCacheResolver { get; }

        protected readonly IConfigurationRoot Config = new ConfigurationBuilder()
                                                          .SetBasePath(Directory.GetCurrentDirectory())
                                                          .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                                                          .Build()
                                                          ;

        public MultitenancyBaseFixture()
        {
            var harness = new TestHarness();

            MemoryCacheResolver = harness.Resolver;
        }

        internal WebHostBuilder CreateWebHostBuilder<TStartup, TTenant, TResolver>()
            where TStartup : class
             where TTenant : class
             where TResolver : class, ITenantResolver<TTenant>
        {
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseEnvironment("IntegrationTest");
            webHostBuilder.UseKestrel();
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
            webHostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(Config);

            return webHostBuilder;
        }

        private class TestHarness
        {
            private static ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();

            public IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions()
            {
                // for testing purposes, we'll scan every 100 milliseconds
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
                Clock = new Microsoft.Extensions.Internal.SystemClock()
            });

            public TestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
            {
                var options = new MemoryCacheTenantResolverOptions { DisposeOnEviction = disposeOnEviction, EvictAllEntriesOnExpiry = evictAllOnExpiry };
                Resolver = new TestTenantMemoryCacheResolver(Cache, loggerFactory, options, cacheExpirationInSeconds);
            }

            public ITenantResolver<TestTenant> Resolver { get; }
        }
    }

    public class MultitenancyBaseTest : IClassFixture<MultitenancyBaseFixture>
    {
        private MultitenancyBaseFixture fixture;

        public MultitenancyBaseTest(MultitenancyBaseFixture fixture)
        {
            this.fixture = fixture;
        }

        protected WebHostBuilder CreateWebHostBuilder<TStartup, TTenant, TResolver>()
           where TStartup : class
            where TTenant : class
            where TResolver : class, ITenantResolver<TTenant>
        {
            return fixture.CreateWebHostBuilder<TStartup, TTenant, TResolver>();
        }
    }

    internal class TestTenantMemoryCacheResolver : MemoryCacheTenantResolver<TestTenant>
    {
        private readonly List<TestTenant> tenants = new List<TestTenant>()
                                           {
                                               new TestTenant() { Name = "Tenant 1", Hostnames = new string[] {
                                                   "/tenant-1-1",
                                                   "/tenant-1-2",
                                                   "/tenant-1-3"
                                               }},
                                               new TestTenant() { Name = "Tenant 2", Hostnames = new string[] {
                                                   "/tenant-2-1",
                                                   "/tenant-2-1",
                                                   "/tenant-2-1"
                                               } }
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
            var tenant = tenants.FirstOrDefault(testTenant => testTenant.Hostnames.ToList().Contains(context.Request.Path));

            var tenantContext = new TenantContext<TestTenant>(tenant);

            tenantContext.Properties.Add("Created", DateTime.UtcNow);

            return Task.FromResult(tenantContext);
        }
    }
}