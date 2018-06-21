using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Puzzle.Core.Multitenancy.Internal;
using Puzzle.Core.Multitenancy.Internal.Resolvers;
using PuzzleCMS.UnitsTests.Base;
using Xunit;

namespace PuzzleCMS.UnitsTests.Multitenancy
{
    public class WebHostBuilderExtensionsTests
    {

        [Fact]
        public async Task ThrowArgumentNullExceptionWhenOptionsExtensionsWebHostBuilderExtensionsHasNullParameter()
        {
            // Arrange

            // Act
            // Assert

            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartup<TestStartup>(null,null);
            })).ConfigureAwait(false);

            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                 UseUnobtrusiveMulitenancyStartup<TestStartup, TestTenant, TestTenantMemoryCacheResolver>(null,null);
            })).ConfigureAwait(false);

            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartupWithDefaultConvention<TestStartup>(null);
            })).ConfigureAwait(false);

            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartupWithDefaultConvention<TestStartup>(null);
            })).ConfigureAwait(false);


            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartup<TestStartup,TestTenant, TestTenantMemoryCacheResolver>(null,typeof(TestStartup),null,true,null);
            })).ConfigureAwait(false);


        }


        [Fact]
        public async Task ThrowArgumentNullException_IfMultitenancyOptionsNotFoundInConfiguration()
        {

            Exception ex = await Assert.ThrowsAsync<Exception>(() => Task.Run(() =>
            {
                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartup<TestStartup>(new WebHostBuilder(), new ConfigurationBuilder().Build());
            })).ConfigureAwait(false);
            Assert.Contains(ex.Message, "MultitenancyOptions not found in configuration.");

            ex = await Assert.ThrowsAsync<Exception>(() => Task.Run(() =>
            {
                IConfigurationRoot memory = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"MultitenancyOptions:OtherTokens:TenantFolder", string.Empty}
                })
                .Build();

                Puzzle.Core.Multitenancy.Extensions.
                WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartup<TestStartup>(new WebHostBuilder(), memory);
            })).ConfigureAwait(false);
            Assert.Contains(ex.Message, "MultitenancyOptions not found in configuration.");
        }

    }
}
