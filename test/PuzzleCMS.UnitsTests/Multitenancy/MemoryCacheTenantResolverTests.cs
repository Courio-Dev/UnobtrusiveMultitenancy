namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using PuzzleCMS.UnitsTests.Base;
    using Xunit;
    using static PuzzleCMS.UnitsTests.Base.MultitenancyBaseFixture;

    public class MemoryCacheTenantResolverTests : MultitenancyBaseTest
    {
        private const string RequestPathTenant11 = "/tenant-1-1";
        private const string RequestPathTenant12 = "/tenant-1-2";
        private const string RequestPathTenant13 = "/tenant-1-3";

        public MemoryCacheTenantResolverTests(MultitenancyBaseFixture testBaseFixture)
            : base(testBaseFixture)
        {
        }

        [Theory]
        [ClassData(typeof(CacheTenantResolverTestData))]
        public async Task CannotResolveMemoryCaheAppTenantResolver_IfParamsIsnNull(
            IMemoryCache cache, ILoggerFactory loggerFactory, MemoryCacheTenantResolverOptions options)
        {
            Task res() => Task.Run(() =>
            {
                TestTenantMemoryCacheResolver resolver = new TestTenantMemoryCacheResolver(cache, loggerFactory, options, cacheExpirationInSeconds: 1);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(res).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.NotNull(ex.Message);
        }

        [Fact]
        public async Task CannotResolve_TenantContextFromMemoryCacheResolver_IfHttpContextIsNull()
        {
            // Arrange
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(Fixture.UrlTenant1);

            // Act
            Task res() => Task.Run(async () =>
            {
                TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(null);
            });

            // Assert
            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(res).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.Contains("context", ex.Message);
        }

        [Fact]
        public async Task CannotResolve_TenantContextFromCaheAppTenantResolver_IfHttpContextIsNull()
        {
            // Arrange
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(Fixture.UrlTenant1);

            // Act
            Task res() => Task.Run(async () =>
            {
                TenantContext<AppTenant> tenantContext = await harness.CachingAppTenantResolver.ResolveAsync(null);
            });

            // Assert
            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(res).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.Contains("context", ex.Message);
        }

        [Fact]
        public async Task ReturnNullTenantContext_FromMemoryCacheResolver_IfCaheNotFound()
        {
            // Arrange
            TestHarness harness = new TestHarness();
            HttpContext context = new DefaultHttpContext();
            context.Request.Path = null;
            context.Request.Host = new HostString("/test-tenant-notfound");

            // Act
            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            // Assert
            Assert.Null(tenantContext);
        }

        [Fact]
        public async Task ReturnNullTenantContext_FromCaheAppTenantResolver_IfCaheNotFound()
        {
            // Arrange
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext("/test-tenant-notfound");

            // Act
            TenantContext<AppTenant> tenantContext = await harness.CachingAppTenantResolver.ResolveAsync(context);

            // Assert
            Assert.Null(tenantContext);
        }

        [Fact]
        public async Task CanResolve_TenantContext()
        {
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(Fixture.UrlTenant1);

            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            Assert.NotNull(tenantContext);
            Assert.Equal("Tenant 1", tenantContext.Tenant.Name);
        }

        [Fact]
        public async Task CanResolve_TenantContext_FromCachingAppTenant()
        {
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(Fixture.UrlTenant1);

            TenantContext<AppTenant> tenantContext = await harness.CachingAppTenantResolver.ResolveAsync(context);

            Assert.NotNull(tenantContext);
            Assert.Equal("Tenant 1", tenantContext.Tenant.Name);
        }

        [Fact]
        public async Task CannotResolve_TenantContext_IfParamsIsnNull()
        {
            // Arrange
            TestHarness harness = new TestHarness();

            Task res() => Task.Run(() =>
            {
                CachingAppTenantResolver cachingAppTenantResolver = new CachingAppTenantResolver(
               harness.Cache,
               new LoggerFactory().AddConsole(),
               null);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(res).ConfigureAwait(false);
            Assert.Contains("optionsMonitor", ex.Message);
        }

        [Fact]
        public async Task CanRetrieve_TenantContext_FromCachingAppTenant()
        {
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(RequestPathTenant11);

            TenantContext<AppTenant> tenantContext = await harness.CachingAppTenantResolver.ResolveAsync(context);

            Assert.True(harness.Cache.TryGetValue(RequestPathTenant12, out TenantContext<AppTenant> cachedTenant));

            Assert.Equal(tenantContext.Tenant.Name, cachedTenant.Tenant.Name);
        }

        [Fact]
        public async Task CanRetrieve_TenantContext_FromCache()
        {
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(RequestPathTenant11);

            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            Assert.True(harness.Cache.TryGetValue(RequestPathTenant12, out TenantContext<TestTenant> cachedTenant));

            Assert.Equal(tenantContext.Tenant.Name, cachedTenant.Tenant.Name);
        }

        [Fact]
        public async Task CanRetrieveTenantContext_FromCacheUsingLinkedIdentifier()
        {
            TestHarness harness = new TestHarness();
            HttpContext context = CreateContext(RequestPathTenant11);

            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            Assert.True(harness.Cache.TryGetValue(RequestPathTenant12, out TenantContext<TestTenant> cachedTenant));

            Assert.Equal(tenantContext.Tenant.Name, cachedTenant.Tenant.Name);
        }

        [Fact]
        public async Task ShouldDisposeTenantOnEviction_FromCacheByDefault()
        {
            TestHarness harness = new TestHarness(cacheExpirationInSeconds: 1);
            HttpContext context = CreateContext(RequestPathTenant11);

            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            Thread.Sleep(1000);

            // force MemoryCache to examine itself cache for pending evictions.
            harness.Cache.Get(RequestPathTenant12);

            // and give it a moment to works its magic.
            Thread.Sleep(100);

            // should also expire tenant context by default
            Assert.False(harness.Cache.TryGetValue(RequestPathTenant11, out TenantContext<TestTenant> cachedTenant), "Tenant Exists");
            Assert.True(tenantContext.Tenant.Disposed);
            Assert.Null(cachedTenant);
        }

        [Fact]
        public async Task ShouldEvictAllCacheEntriesOf_TenantContextByDefault()
        {
            TestHarness harness = new TestHarness(cacheExpirationInSeconds: 10);

            // first request
            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(CreateContext(RequestPathTenant11));

            // cache should have all 3 entries
            Assert.NotNull(harness.Cache.Get(RequestPathTenant11));
            Assert.NotNull(harness.Cache.Get(RequestPathTenant12));
            Assert.NotNull(harness.Cache.Get(RequestPathTenant13));

            // expire
            harness.Cache.Remove(RequestPathTenant11);

            Thread.Sleep(500);

            // look it up again so it registers
            harness.Cache.TryGetValue(RequestPathTenant11, out TenantContext<TestTenant> cachedTenant);

            Thread.Sleep(500);

            // pear is expired - because apple is
            Assert.False(harness.Cache.TryGetValue(RequestPathTenant13, out cachedTenant), "Tenant Exists");

            // should also expire tenant context by default
            Assert.True(tenantContext.Tenant.Disposed);
        }

        [Fact]
        public async Task CanEvictSingleCacheEntry_OfTenantContext()
        {
            TestHarness harness = new TestHarness(cacheExpirationInSeconds: 2, evictAllOnExpiry: false);
            HttpContext context = CreateContext(RequestPathTenant11);

            // first request for tenant 1
            await harness.Resolver.ResolveAsync(CreateContext(RequestPathTenant11));

            // wait 1 second
            Thread.Sleep(1000);

            // second request
            await harness.Resolver.ResolveAsync(CreateContext(RequestPathTenant12));

            // wait 1 second
            Thread.Sleep(1000);

            // apple is expired
            Assert.False(harness.Cache.TryGetValue(RequestPathTenant11, out TenantContext<TestTenant> cachedTenant), "Tenant Exists");

            // pear is not expired
            Assert.True(harness.Cache.TryGetValue(RequestPathTenant12, out cachedTenant), "Tenant Does Not Exist");
        }

        [Fact]
        public async Task CanDisposeOnEviction()
        {
            TestHarness harness = new TestHarness(cacheExpirationInSeconds: 1, disposeOnEviction: true);
            HttpContext context = CreateContext(RequestPathTenant11);

            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            Thread.Sleep(2 * 1000);

            // access it again so that MemoryCache examines it's cache for pending evictions
            harness.Cache.Get(RequestPathTenant12);

            Thread.Sleep(1 * 1000);

            // access it again and we should see the eviction
            Assert.True(tenantContext.Tenant.Disposed);
        }

        [Fact]
        public async Task CannotDispose_OnEviction()
        {
            TestHarness harness = new TestHarness(cacheExpirationInSeconds: 1, disposeOnEviction: false);
            HttpContext context = CreateContext(RequestPathTenant11);

            TenantContext<TestTenant> tenantContext = await harness.Resolver.ResolveAsync(context);

            Thread.Sleep(1 * 1000);

            // access it again so that MemoryCache examines it's cache for pending evictions
            harness.Cache.Get(RequestPathTenant12);

            Thread.Sleep(1 * 1000);

            // access it again and even though it's disposed, it should not be evicted
            Assert.False(tenantContext.Tenant.Disposed);
        }

        private HttpContext CreateContext(string requestPath)
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Path = requestPath;
            context.Request.Host = new HostString(requestPath);

            return context;
        }

        private class CacheTenantResolverTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { null, new LoggerFactory().AddConsole(), new MemoryCacheTenantResolverOptions() };
                yield return new object[] { new TestHarness().Cache, null, new MemoryCacheTenantResolverOptions() };
                yield return new object[] { new TestHarness().Cache, new LoggerFactory().AddConsole(), null };
                yield return new object[] { null, null, null };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
