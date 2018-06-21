using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Puzzle.Core.Multitenancy.Extensions;
using Puzzle.Core.Multitenancy.Internal;
using Puzzle.Core.Multitenancy.Internal.Configurations;
using PuzzleCMS.UnitsTests.Base;
using Xunit;

namespace PuzzleCMS.UnitsTests.Multitenancy
{
    public class OptionsExtensionsTests
    {
        [Theory]
        [ClassData(typeof(OptionsExtensionsTestTestTenantData))]
        public async Task ThrowArgumentNullExceptionWhenOptionsExtensionsTestTenantHasNullParameter(IServiceCollection services)
        {
            // Arrange

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                OptionsExtensions.AddMultitenancyOptions<TestTenant>(services, null);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
        }

        [Theory]
        [ClassData(typeof(OptionsExtensionsTestAppTenantData))]
        public async Task ThrowArgumentNullExceptionWhenOptionsExtensionsAppTenantHasNullParameter(IServiceCollection services)
        {
            // Arrange

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                OptionsExtensions.AddMultitenancyOptions<TestTenant>(services, null);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
        }

        private class OptionsExtensionsTestTestTenantData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { (IServiceCollection)null };
                yield return new object[] { new ServiceCollection() };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class OptionsExtensionsTestAppTenantData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { (IServiceCollection)null };
                yield return new object[] { new ServiceCollection() };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
