namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using Microsoft.AspNetCore.Http;
    using PuzzleCMS.Core.Multitenancy;
    using PuzzleCMS.Core.Multitenancy.Extensions;
    using Xunit;

    public class MultitenancyHttpContextExtensionsTests
    {
        [Fact]
        public void CannotSetTenantContext_WhenHttpContextIsNull()
        {
            HttpContext httpContext = null;
            TenantContext<AppTenantTest> tenantContext = new TenantContext<AppTenantTest>(new AppTenantTest(),0);

            Exception ex = Assert.Throws<ArgumentNullException>(() => httpContext.SetTenantContext(tenantContext));
            Assert.Contains($"Argument context must not be null", ex.Message);
        }

        [Fact]
        public void CannotSetTenantContext_WhenTenantContextIsNull()
        {
            HttpContext httpContext = new DefaultHttpContext();
            TenantContext<AppTenantTest> tenantContext = null;

            Exception ex = Assert.Throws<ArgumentNullException>(() => httpContext.SetTenantContext(tenantContext));
            Assert.Contains($"Argument tenantContext must not be null", ex.Message);
        }

        [Fact]
        public void CannotGetTenantContext_WhenHttpContextIsNull()
        {
            HttpContext httpContext = null;

            Exception ex = Assert.Throws<ArgumentNullException>(() => httpContext.GetTenantContext<AppTenantTest>());
            Assert.Contains($"Argument context must not be null", ex.Message);
        }

        [Fact]
        public void CannotGetTenant_WhenHttpContextIsNull()
        {
            HttpContext httpContext = null;

            Exception ex = Assert.Throws<ArgumentNullException>(() => httpContext.GetTenant<AppTenantTest>());
            Assert.Contains($"Argument context must not be null", ex.Message);
        }

        [Fact]
        public void ReturnNullTenantContext_WhenTenantContextIsNotFound()
        {
            HttpContext httpContext = new DefaultHttpContext();

            TenantContext<AppTenantTest> tenantContext = httpContext.GetTenantContext<AppTenantTest>();

            Assert.Null(tenantContext);
        }

        [Fact]
        public void ReturnNullDefautlClassTenant_WhenTenantIsNotFound()
        {
            HttpContext httpContext = new DefaultHttpContext();

            AppTenantTest tenant = httpContext.GetTenant<AppTenantTest>();

            Assert.Same(default(AppTenantTest), tenant);
        }

        [Fact]
        public void WhenConstrucTenantContextWithNullTeanntWith__ThenThrowArgumentNullException()
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new TenantContext<AppTenantTest>(null,0));
            Assert.NotNull(ex);
        }

        [Fact]
        public void CanGet_And_SetTenantContext()
        {
            HttpContext httpContext = new DefaultHttpContext();

            TenantContext<AppTenantTest> tenantContext = new TenantContext<AppTenantTest>(new AppTenantTest(),0);
            httpContext.SetTenantContext(tenantContext);

            Assert.Same(tenantContext, httpContext.GetTenantContext<AppTenantTest>());
        }

        [Fact]
        public void CanGet_TenantInstance()
        {
            HttpContext httpContext = new DefaultHttpContext();

            AppTenantTest tenant = new AppTenantTest { Name = "Name" };
            httpContext.SetTenantContext(new TenantContext<AppTenantTest>(tenant,0));

            Assert.Same(tenant, httpContext.GetTenant<AppTenantTest>());
        }

        private class AppTenantTest
        {
            public string Name { get; set; }
        }
    }
}
