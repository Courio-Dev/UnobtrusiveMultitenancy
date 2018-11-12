using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuzzleCMS.Core.Multitenancy.Extensions;
using PuzzleCMS.Core.Multitenancy.Internal;
using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;
using PuzzleCMS.Core.Multitenancy.Internal.Options;
using PuzzleCMS.UnitsTests.Base;
using Xunit;

namespace PuzzleCMS.UnitsTests.Multitenancy
{
    public class MultitenancyOptionsProviderBuilderExtensionsTests
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
                MultitenancyOptionsProviderBuilderExtensions.
                UseColoredConsoleLogProvider<TestTenant>(null);

            })).ConfigureAwait(false);


        }

        [Fact]
        public void DoNotThrow_WhenSetSeriLogProvider()
        {
            // Arrange
            IServiceProvider sp = new ServiceCollection().BuildServiceProvider();
            MultiTenancyConfig<TestTenant> config = sp.GetService<MultiTenancyConfig<TestTenant>>() ?? new MultiTenancyConfig<TestTenant>("");

            MultitenancyOptionsProviderBuilder<TestTenant> builder = new MultitenancyOptionsProviderBuilder<TestTenant>(
              new ServiceCollection(),
              new MultitenancyOptionsProvider<TestTenant>(sp, config));

            // Act
            // Assert
            PuzzleCMS.
            Core.
            Multitenancy.Extensions.
            MultitenancyOptionsProviderBuilderExtensions.
            UseSerilogLogProvider<TestTenant>(builder);
        }


        [Fact]
        public void CanAddCustomConfigureServices_WhenUseCustomServicesTenant()
        {
            // Arrange
            IServiceProvider sp = new ServiceCollection().BuildServiceProvider();
            MultiTenancyConfig<TestTenant> config = sp.GetService<MultiTenancyConfig<TestTenant>>() ?? new MultiTenancyConfig<TestTenant>("");

            MultitenancyOptionsProviderBuilder<TestTenant> builder = new MultitenancyOptionsProviderBuilder<TestTenant>(
              new ServiceCollection(),
              new MultitenancyOptionsProvider<TestTenant>(sp, config));

            // Act
            ILogProvider withFunc(IServiceCollection sc, TestTenant t, IConfiguration conf) => default;

            // Assert
            PuzzleCMS.
                Core.
                Multitenancy.Extensions.
            MultitenancyOptionsProviderBuilderExtensions.
            UseCustomServicesTenant<TestTenant>(builder, withFunc).
            UseCustomServicesTenant<TestTenant>(withFunc)
            ;

            Assert.NotNull(builder);
            Assert.NotNull(builder.BuildOptionsProviderBuilder());
            Assert.NotNull(builder.BuildOptionsProviderBuilder().TenantLogProvider);
        }

        [Fact]
        public void CanAddCustomConfigureServices_WhenUseConfigureServicesTenant()
        {
            // Arrange
            IServiceProvider sp = new ServiceCollection().BuildServiceProvider();
            MultiTenancyConfig<TestTenant> config = sp.GetService<MultiTenancyConfig<TestTenant>>() ?? new MultiTenancyConfig<TestTenant>("");

            MultitenancyOptionsProviderBuilder<TestTenant> builder = new MultitenancyOptionsProviderBuilder<TestTenant>(
              new ServiceCollection(),
              new MultitenancyOptionsProvider<TestTenant>(sp, config));

            // Act
            void withAction(IServiceCollection sc, TestTenant t) { Console.Write(""); }

            // Assert
            PuzzleCMS.Core
                .Multitenancy.Extensions
                    .MultitenancyOptionsProviderBuilderExtensions
                    .UseConfigureServicesTenant<TestTenant>(builder, withAction)
                    .UseConfigureServicesTenant<TestTenant>(withAction)
                    ;

            Assert.NotNull(builder);
            Assert.NotNull(builder.BuildOptionsProviderBuilder());
            Assert.NotNull(builder.BuildOptionsProviderBuilder().ConfigureServicesTenantList);

            Assert.Equal(2, builder.BuildOptionsProviderBuilder().ConfigureServicesTenantList.Count);
        }

        [Fact]
        public async Task ThrowException_IfMultitenancyOptionsNotFoundWhenBuildTemporaryMulitenancyProvider()
        {
            // Arrange.
            // Act
            void withAction(IServiceProvider p, MultitenancyOptionsProviderBuilder<TestTenant> a) { Console.Write(""); }

            // Assert.
            //// bool hasTenants;
            Exception ex = await Assert.ThrowsAsync<Exception>(() => Task.Run(() =>
            {

                new WebHostBuilder()
                    .GetTemporaryMulitenancyProviderAndValidate<TestTenant>("", true, null, withAction, out bool hasTenants);

                Assert.False(hasTenants);


            })).ConfigureAwait(false);
            Assert.Contains(ex.Message, "MultitenancyOptions not found in configuration.");
        }

        [Fact]
        public void DoeNotThrowException_IfMultitenancyOptionsNotFoundWhenBuildTemporaryMulitenancyProviderAndNotThrowsError()
        {
            // Arrange.          
            // Act.
            void withAction(IServiceProvider p, MultitenancyOptionsProviderBuilder<TestTenant> a) { Console.Write(""); }

            // Assert.
            new WebHostBuilder()
                .GetTemporaryMulitenancyProviderAndValidate<TestTenant>("", false, null, withAction, out bool hasTenants);

            Assert.False(hasTenants);
        }

        [Fact]
        public async Task ThrowArgumentNullException_IfArgsIsNullWhenCallValidateMultitenancyOptionsProvider()
        {
            // Arrange.

            // Act.
            // Assert.
            ArgumentNullException ex = await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() =>
            {
                ServiceCollectionExtensions.ValidateMultitenancyOptionsProvider<TestTenant>(null, true);
            })).ConfigureAwait(false);

            Assert.NotNull(ex);
        }


        [Theory]
        [ClassData(typeof(OptionsExtensionsTestTestTenantData))]
        public void ThrowArgumentNullExceptionWhenOptionsExtensionsTestTenantHasNullParameter(IServiceCollection services)
        {
            // Arrange

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                ServiceCollectionExtensions.AddMultitenancyOptions<TestTenant>(null, new MultiTenancyConfig<TestTenant>(""), optionsAction: (sp, opts) => { });
            });

            Assert.ThrowsAsync<ArgumentNullException>(Res);
        }

        [Theory]
        [ClassData(typeof(OptionsExtensionsTestAppTenantData))]
        public void ThrowArgumentNullExceptionWhenOptionsExtensionsHasNullParameter(IServiceCollection services)
        {
            // Arrange

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                ServiceCollectionExtensions.AddMultitenancyOptions<TestTenant>(services, null, optionsAction: (sp, opts) => { });
            });

            Assert.ThrowsAsync<ArgumentNullException>(Res);
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
