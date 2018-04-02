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

    public class MultitenancyBaseFixture
    {
        public MultitenancyBaseFixture()
        {
            TestHarness harness = new TestHarness();

            MemoryCacheResolver = harness.Resolver;
        }

        protected static IConfigurationRoot Config { get; } = new ConfigurationBuilder()
                                                          .SetBasePath(Directory.GetCurrentDirectory())
                                                          .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                                                          .Build()
                                                          ;

        internal string UrlTenant1 { get; } = "/tenant-1-1";

        internal string UrlTenant2 { get; } = "/tenant-2-1";

        private ITenantResolver<TestTenant> MemoryCacheResolver { get; }

        internal static WebHostBuilder CreateWebHostBuilder<TStartup, TTenant, TResolver>()
            where TStartup : class
             where TTenant : class
             where TResolver : class, ITenantResolver<TTenant>
        {
            WebHostBuilder webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseEnvironment("IntegrationTest");
            webHostBuilder.UseKestrel();
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
            webHostBuilder.UseUnobtrusiveMulitenancyStartup<TStartup, TTenant, TResolver>(Config);

            return webHostBuilder;
        }

        internal static TestHarness CreateTestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
        {
            TestHarness harness = new TestHarness();
            return harness;
        }

        internal class TestHarness
        {
            public IMemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions()
            {
                // for testing purposes, we'll scan every 100 milliseconds
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
                Clock = new Microsoft.Extensions.Internal.SystemClock()
            });

            public TestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
            {
                MemoryCacheTenantResolverOptions options = new MemoryCacheTenantResolverOptions { DisposeOnEviction = disposeOnEviction, EvictAllEntriesOnExpiry = evictAllOnExpiry };
                Resolver = new TestTenantMemoryCacheResolver(Cache, LoggerFactory, options, cacheExpirationInSeconds);

                ServiceProvider services = new ServiceCollection()
                        .AddSingleton<IOptionsFactory<MultitenancyOptions>, MultitenancyOptionsFactoryTests>()
                        .Configure<MultitenancyOptions>(o => { })
                        .BuildServiceProvider();

                IOptionsMonitor<MultitenancyOptions> monitor = services.GetRequiredService<IOptionsMonitor<MultitenancyOptions>>();
                CachingAppTenantResolver =new CachingAppTenantResolver(Cache, LoggerFactory, monitor);
            }

            public ITenantResolver<TestTenant> Resolver { get; }

            public ITenantResolver<AppTenant> CachingAppTenantResolver { get; }

            

            protected static ILoggerFactory LoggerFactory { get; } = new LoggerFactory().AddConsole();
        }
    }

    public class MultitenancyBaseTest : IClassFixture<MultitenancyBaseFixture>
    {
        protected MultitenancyBaseFixture fixture;

        public MultitenancyBaseTest(MultitenancyBaseFixture fixture)
        {
            this.fixture = fixture;
        }

        protected WebHostBuilder CreateWebHostBuilder<TStartup, TTenant, TResolver>()
           where TStartup : class
           where TTenant : class
           where TResolver : class, ITenantResolver<TTenant>
        {
            return MultitenancyBaseFixture.CreateWebHostBuilder<TStartup, TTenant, TResolver>();
        }

        internal TestHarness CreateTestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
        {
            return MultitenancyBaseFixture.CreateTestHarness(
                disposeOnEviction: disposeOnEviction, 
                cacheExpirationInSeconds: cacheExpirationInSeconds, 
                evictAllOnExpiry: evictAllOnExpiry);
        }
    }

    internal class TestTenantMemoryCacheResolver : MemoryCacheTenantResolver<TestTenant>
    {
        private readonly List<TestTenant> tenants = new List<TestTenant>()
                                           {
                                               new TestTenant(){
                                                    Name = "Tenant 1", Hostnames = new string[]
                                                    {
                                                       "/tenant-1-1",
                                                       "/tenant-1-2",
                                                       "/tenant-1-3"
                                                   }
                                                },
                                               new TestTenant(){
                                                    Name = "Tenant 2", Hostnames = new string[]
                                                    {
                                                       "/tenant-2-1",
                                                       "/tenant-2-1",
                                                       "/tenant-2-1"
                                                   }
                                                }
                                               ,
                                               new TestTenant(){
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
