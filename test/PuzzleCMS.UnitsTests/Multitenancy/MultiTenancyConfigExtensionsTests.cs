using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PuzzleCMS.UnitsTests.Base;
using Xunit;

namespace PuzzleCMS.UnitsTests.Multitenancy
{
    public class MultiTenancyConfigExtensionsTests
    {
        [Fact]
        public async Task ThrowArgumentNullException_WhenMultiTenancyConfigExtensions_HasNullParameter()
        {
            // Arrange

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                PuzzleCMS.
                Core.
                Multitenancy.Extensions.
                MultiTenancyConfigExtensions.
                UseColoredConsoleLogProvider<TestTenant>(null);

            })).ConfigureAwait(false);
            

        }

    }
}
