namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using PuzzleCMS.Core.Multitenancy.Extensions;
    using PuzzleCMS.Core.Multitenancy.Internal;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;
    using PuzzleCMS.Core.Multitenancy.Internal.Resolvers;

    /// <summary>
    /// The common fixture for multitenancy test.
    /// </summary>
    public class MultitenancyBaseFixture
    {
        internal const string environmentTest= "IntegrationTest";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyBaseFixture"/> class.
        /// </summary>
        public MultitenancyBaseFixture()
        {
            TestHarness harness = new TestHarness();

            MemoryCacheResolver = harness.TestTenantResolver;
        }

        internal string UrlTenant1 { get; } = "/tenant-1-1";

        internal string UrlTenant2 { get; } = "/tenant-2-1";

        /// <summary>
        /// Gets the config.
        /// </summary>
        //protected static IConfigurationRoot Config { get; } = new ConfigurationBuilder()
        //                                                  .SetBasePath(Directory.GetCurrentDirectory())
        //                                                  .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
        //                                                  .Build()
        // 

        private static Dictionary<string, string> baseConfig { get; } = new Dictionary<string, string>()
        {
                    {"MultitenancyOptions:OtherTokens:TenantFolder", "App_Tenants_Override"},

                    {"MultitenancyOptions:Tenants:0:Name", "Tenant 1"},
                    {"MultitenancyOptions:Tenants:0:Theme", "{DS}"},
                    {"MultitenancyOptions:Tenants:0:ConnectionString", "{TenantFolder}"},
                    {"MultitenancyOptions:Tenants:0:Hostnames:0", "/tenant-1-1" },
                    {"MultitenancyOptions:Tenants:0:Hostnames:1", "/tenant-1-2" },
                    {"MultitenancyOptions:Tenants:0:Hostnames:2", "/tenant-1-3" },
                    {"MultitenancyOptions:Tenants:0:Hostnames:3", "localhost:47887" },
                    {"MultitenancyOptions:Tenants:0:Hostnames:4", "localhost:44301"},
                    {"MultitenancyOptions:Tenants:0:Hostnames:5", "localhost:60000"},

                    {"MultitenancyOptions:Tenants:1:Name", "Tenant 2"},
                    {"MultitenancyOptions:Tenants:1:Theme",""},
                    {"MultitenancyOptions:Tenants:1:ConnectionString",""},
                    {"MultitenancyOptions:Tenants:1:Hostnames:0", "/tenant-2-1" },
                    {"MultitenancyOptions:Tenants:1:Hostnames:1", "/tenant-2-2" },
                    {"MultitenancyOptions:Tenants:1:Hostnames:2", "/tenant-2-3" },
                    {"MultitenancyOptions:Tenants:1:Hostnames:3", "localhost:44302"},
                    {"MultitenancyOptions:Tenants:1:Hostnames:4", "localhost:60001"},

                    {"MultitenancyOptions:Tenants:2:Name", "Tenant 3"},
                    {"MultitenancyOptions:Tenants:2:Theme",""},
                    {"MultitenancyOptions:Tenants:2:ConnectionString",""},
                    {"MultitenancyOptions:Tenants:2:Hostnames:0", "localhost:44304"},
                    {"MultitenancyOptions:Tenants:2:Hostnames:1", "localhost:44305"},

                    {"MultitenancyOptions:Tenants:3:Name", "Tenant 4"},
                    {"MultitenancyOptions:Tenants:3:Theme",""},
                    {"MultitenancyOptions:Tenants:3:ConnectionString", "xxx2898988"},
                    {"MultitenancyOptions:Tenants:3:Hostnames:0", "localhost:51261"},
        };

        protected static IConfigurationRoot Config { get; private set; } = new ConfigurationBuilder()
                .AddInMemoryCollection(baseConfig)
                .Build();

        protected static void UpdateConfiguration(Dictionary<string, string> additionnal)
        {
            foreach (KeyValuePair<string, string> kvp in additionnal)
            {
                Config[kvp.Key] = kvp.Value;
            }

            Config.Reload();
        }

        private ITenantResolver<TestTenant> MemoryCacheResolver { get; }

        internal static WebHostBuilder CreateWebHostBuilder<TStartup, TTenant, TResolver>()
            where TStartup : class
            where TTenant : class
            where TResolver : class, ITenantResolver<TTenant>
        {
            WebHostBuilder webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseEnvironment(environmentTest);
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
                MemoryCacheTenantResolverOptions options = new MemoryCacheTenantResolverOptions {
                    DisposeOnEviction = disposeOnEviction,
                    EvictAllEntriesOnExpiry = evictAllOnExpiry
                };

                ServiceProvider services = new ServiceCollection()
                        .AddSingleton<IOptionsFactory<MultitenancyOptions<TestTenant>>, MultitenancyOptionsTestTenantFactoryTests>()
                        .AddSingleton<IOptionsFactory<MultitenancyOptions<AppTenant>>, MultitenancyOptionsAppTenantFactoryTests>()
                        //.Configure<MultitenancyOptions>(o => { })
                        .BuildServiceProvider();

                TestMultitenancyOptionsProvider = new MultitenancyOptionsProvider<TestTenant>(new MultiTenancyConfig<TestTenant>(environmentTest, Config));
                AppTenantMultitenancyOptionsProvider = new MultitenancyOptionsProvider<AppTenant>(new MultiTenancyConfig<AppTenant>(environmentTest, Config));

                TestTenantResolver = new TestTenantMemoryCacheResolver(TestMultitenancyOptionsProvider, Cache, new Log<TestTenantMemoryCacheResolver>(LogProvider.CurrentLogProvider), options, cacheExpirationInSeconds);
                AppTenantResolver = new AppTenantResolver(AppTenantMultitenancyOptionsProvider, Cache, new Log<AppTenantResolver>(LogProvider.CurrentLogProvider));
            }

            public IMemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions()
            {
                // for testing purposes, we'll scan every 100 milliseconds
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
                Clock = new Microsoft.Extensions.Internal.SystemClock(),
            });

            public IMultitenancyOptionsProvider<TestTenant> TestMultitenancyOptionsProvider { get; }

            public IMultitenancyOptionsProvider<AppTenant> AppTenantMultitenancyOptionsProvider { get; }

            public ITenantResolver<TestTenant> TestTenantResolver { get; }

            public ITenantResolver<AppTenant> AppTenantResolver { get; }

            protected static ILog<MultitenancyBaseFixture> Logger { get; } = new Log<MultitenancyBaseFixture>(LogProvider.CurrentLogProvider);
        }
    }
}
