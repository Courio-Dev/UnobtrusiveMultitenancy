namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Xunit;

    public class MultitenantConfigTests
    {
        [Fact]
        public void WhenConstructMultitenantConfigTestsWithNullHostingEnvironmentWith__ThenThrowArgumentNullException()
        {
            Exception ex = Assert.Throws<NotImplementedException>(() => new MultiTenancyConfig(null as IHostingEnvironment));
            Assert.NotNull(ex);
        }
    }
}
