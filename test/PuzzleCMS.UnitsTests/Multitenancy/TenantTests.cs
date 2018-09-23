namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using PuzzleCMS.Core.Multitenancy.Internal;
    using Xunit;

    public class TenantTests
    {
        [Fact]
        public void WhenNullNameSsGiven_AndUseTenant_ThenThrowArgumentNullException()
        {
            AppTenant tenant = new AppTenant()
            {
                Name = null,
            };

            Exception ex = Assert.Throws<ArgumentNullException>(() => tenant.Id);
            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData("Tenant 1", "tenant-1")]
        [InlineData("Tenant        1", "tenant-1")]
        [InlineData("Tenant1", "tenant1")]
        [InlineData("Tenant-ééé-1", "tenant-eee-1")]
        [InlineData("Tenant__test", "tenanttest")]
        [InlineData("Tenant_test", "tenanttest")]
        [InlineData("Tenant.test", "tenanttest")]
        [InlineData("Tenant. test", "tenant-test")]
        public void EnsureNameIsSlugifyToGenerateId_WhenUseTenant(string name, string expectedId)
        {
            AppTenant tenant = new AppTenant()
            {
                Name = name,
            };

            Assert.Equal(expectedId, tenant.Id);
        }

        [Fact]
        public void EnsureSameTenantIsReturned_WhenTenantWrapper()
        {
            AppTenant tenant = new AppTenant()
            {
                Name = "Name",
            };
            TenantWrapper<AppTenant> tenantWrapper = new TenantWrapper<AppTenant>(tenant);

            Assert.Same(tenant, tenantWrapper.Value);
        }
    }
}
