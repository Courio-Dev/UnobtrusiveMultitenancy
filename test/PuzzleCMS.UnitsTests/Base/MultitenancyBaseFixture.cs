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
    using Puzzle.Core.Multitenancy.Internal.Logging;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using Xunit;
    using static PuzzleCMS.UnitsTests.Base.MultitenancyBaseFixture;

    /// <summary>
    /// The common fixture for multitenancy test.
    /// </summary>
    public class MultitenancyBaseFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyBaseFixture"/> class.
        /// </summary>
        public MultitenancyBaseFixture()
        {
            TestHarness harness = new TestHarness();

            MemoryCacheResolver = harness.Resolver;
        }

        internal string UrlTenant1 { get; } = "/tenant-1-1";

        internal string UrlTenant2 { get; } = "/tenant-2-1";

        /// <summary>
        /// Gets the config.
        /// </summary>
        protected static IConfigurationRoot Config { get; } = new ConfigurationBuilder()
                                                          .SetBasePath(Directory.GetCurrentDirectory())
                                                          .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                                                          .Build()
                                                          ;

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
            webHostBuilder.UseWebRoot(Directory.GetCurrentDirectory());
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
            public TestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
            {
                MemoryCacheTenantResolverOptions options = new MemoryCacheTenantResolverOptions { DisposeOnEviction = disposeOnEviction, EvictAllEntriesOnExpiry = evictAllOnExpiry };
                Resolver = new TestTenantMemoryCacheResolver(Cache, new Log<TestTenantMemoryCacheResolver>(), options, cacheExpirationInSeconds);

                ServiceProvider services = new ServiceCollection()
                        .AddSingleton<IOptionsFactory<MultitenancyOptions>, MultitenancyOptionsFactoryTests>()
                        .Configure<MultitenancyOptions>(o => { })
                        .BuildServiceProvider();

                IOptionsMonitor<MultitenancyOptions> monitor = services.GetRequiredService<IOptionsMonitor<MultitenancyOptions>>();
                CachingAppTenantResolver = new CachingAppTenantResolver(Cache, new Log<CachingAppTenantResolver>(), monitor);
            }

            public IMemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions()
            {
                // for testing purposes, we'll scan every 100 milliseconds
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
                Clock = new Microsoft.Extensions.Internal.SystemClock(),
            });

            public ITenantResolver<TestTenant> Resolver { get; }

            public ITenantResolver<AppTenant> CachingAppTenantResolver { get; }

            protected static ILog<MultitenancyBaseFixture> Logger { get; } = new Log<MultitenancyBaseFixture>();
        }
    }
}
