namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.UnitsTests.Base;
    using Xunit;

    public class MultitenantConfigTests
    {
        [Fact]
        public void WhenConstructMultitenantConfigTestsWithNullHostingEnvironmentWith__ThenThrowArgumentNullException()
        {
            Exception ex = Assert.Throws<NotImplementedException>(() => new MultiTenancyConfig<TestTenant>(null as IHostingEnvironment));
            Assert.NotNull(ex);
        }
    }
}
