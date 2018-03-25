using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Puzzle.Core.Multitenancy.Internal;
using Puzzle.Core.Multitenancy.Internal.Configurations;
using PuzzleCMS.UnitsTests.Base;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PuzzleCMS.UnitsTests.Multitenancy
{
    public class ConfigureMultitenantServicesBuilderTests : MultitenancyBaseTest
    {
        public ConfigureMultitenantServicesBuilderTests(MultitenancyBaseFixture testBaseFixture)
            : base(testBaseFixture)
        {
        }

        [Fact]
        public async Task WhenConfigurePerTenantServicesHasMoreThanTwoArguments_ThrowException()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartup, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            using (var server = new TestServer(builder))
            using (var client = server.CreateClient())
            {
                Task res() => Task.Run(async () =>
                {
                    var response = await client.GetAsync("/tenant-1-1");
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                });

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(res);
                Assert.Equal("The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.",
                           ex.Message);
            }
        }

        [Fact]
        public async Task WhenConfigurePerTenantServicesHasNonValidArguments_ThrowException()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartup2, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            using (var server = new TestServer(builder))
            using (var client = server.CreateClient())
            {
                Task res() => Task.Run(async () =>
                {
                    var response = await client.GetAsync("/tenant-1-1");
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                });

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(res);
                Assert.Equal("The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.",
                           ex.Message);
            }
        }

        [Fact]
        public async Task WhenConfigurePerTenantServicesHasAllValidArguments_ThenOk()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartupValidArguments, TestTenant, TestTenantMemoryCacheResolver>();

            // Act

            using (var server = new TestServer(builder))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("/tenant-1-1");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal("Default", result);
                builder = null;
            }
        }

        [Fact]
        public void ConventionalStartupClass_WhenHasMulitpleOverrideConfigurePerTenantServicesWithEnv_ThrowsIfStartupBuildsTheContainerAsync()
        {
            //Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();
            var services = serviceCollection.BuildServiceProvider();

            //Act
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                //startup.ConfigureServicesDelegate(serviceCollection)
                var startup = StartupLoaderMultitenant.
                LoadMethods<TestTenant>(services, typeof(TestStartupMulitpleOverrideConfigurePerTenantServicesWithEnv), "IntegrationTest");
            });

            //Assert
            var expectedMessage = $"Having multiple overloads of method 'ConfigurePerTenantIntegrationTestServices' is not supported.";
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void ConventionalStartupClass_WhenHasMulitpleOverrideConfigurePerTenantServicesWitouthEnv_ThrowsIfStartupBuildsTheContainerAsync()
        {
            //Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();
            var services = serviceCollection.BuildServiceProvider();

            //Act
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var startup = StartupLoaderMultitenant.
                LoadMethods<TestTenant>(services, typeof(TestStartupMulitpleOverrideConfigurePerTenantServicesWithoutEnv), "");
            });

            //Assert
            var expectedMessage = $"Having multiple overloads of method 'ConfigurePerTenantServices' is not supported.";
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureServicesHasInvalidOperationException_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartupConfigureServicesInvalidOperationException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task res() => Task.Run(() =>
            {
                var server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(res);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureServicesHasException_ThenThrowsException()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartupConfigureServicesException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task res() => Task.Run(() =>
            {
                var server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<Exception>(res);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureHasInvalidOperationException_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartupConfigureInvalidOperationException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task res() => Task.Run(() =>
            {
                var server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(res);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureHasException_ThenThrowsException()
        {
            // Arrange
            var builder = CreateWebHostBuilder<TestStartupConfigureException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task res() => Task.Run(() =>
            {
                var server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<Exception>(res);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        private class TestStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            /// <inheritdoc />
            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant, MultiTenancyConfig config)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartup2
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            /// <inheritdoc />
            public void ConfigurePerTenantServices(IServiceCollection services, MultiTenancyConfig config)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartupValidArguments
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            /// <inheritdoc />
            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartupMulitpleOverrideConfigurePerTenantServicesWithEnv
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            /// <inheritdoc />
            public void ConfigurePerTenantIntegrationTestServices(IServiceCollection services, TestTenant tenant)
            {
            }

            public void ConfigurePerTenantIntegrationTestServices(IServiceCollection services, TestTenant tenant, TestTenant tenant1)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartupMulitpleOverrideConfigurePerTenantServicesWithoutEnv
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            /// <inheritdoc />
            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
            }

            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant, TestTenant tenant1)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartupConfigureServicesInvalidOperationException
        {
            public void ConfigureServices(IServiceCollection services)
            {
                throw new InvalidOperationException("TestStartupInvalidOperationException");
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartupConfigureInvalidOperationException
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                throw new InvalidOperationException("TestStartupInvalidOperationException");
            }
        }

        private class TestStartupConfigureServicesException
        {
            public void ConfigureServices(IServiceCollection services)
            {
                throw new Exception("TestStartupInvalidOperationException");
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync(("Default"));
                });
            }
        }

        private class TestStartupConfigureException
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                throw new Exception("TestStartupInvalidOperationException");
            }
        }
    }
}