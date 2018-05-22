namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.UnitsTests.Base;
    using Xunit;

    /// <summary>
    /// Test for configure Multitenant.
    /// </summary>
    public class ConfigureMultitenantServicesBuilderTests : MultitenancyBaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureMultitenantServicesBuilderTests"/> class.
        /// </summary>
        public ConfigureMultitenantServicesBuilderTests(MultitenancyBaseFixture testBaseFixture)
            : base(testBaseFixture)
        {
        }

        [Fact]
        public async Task WhenConfigurePerTenantServicesHasMoreThanTwoArguments_ThrowException()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartup, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            using (TestServer server = new TestServer(builder))
            using (System.Net.Http.HttpClient client = server.CreateClient())
            {
                Task Res() => Task.Run(async () =>
                {
                    System.Net.Http.HttpResponseMessage response = await client.GetAsync("/tenant-1-1").ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                });

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(Res).ConfigureAwait(false);
                Assert.Equal(
                    "The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.",
                    ex.Message);
            }
        }

        [Fact]
        public async Task WhenConfigurePerTenantServicesHasNonValidArguments_ThrowException()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartup2, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            using (TestServer server = new TestServer(builder))
            using (System.Net.Http.HttpClient client = server.CreateClient())
            {
                Task Res() => Task.Run(async () =>
                {
                    System.Net.Http.HttpResponseMessage response = await client.GetAsync("/tenant-1-1").ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                });

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(Res).ConfigureAwait(false);
                Assert.Equal(
                    "The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.",
                    ex.Message);
            }
        }

        [Fact]
        public async Task WhenConfigurePerTenantServicesHasAllValidArguments_ThenOk()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartupValidArguments, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            using (TestServer server = new TestServer(builder))
            using (System.Net.Http.HttpClient client = server.CreateClient())
            {
                System.Net.Http.HttpResponseMessage response = await client.GetAsync("/tenant-1-1").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Assert
                Assert.Equal("Default", result);
                builder = null;
            }
        }

        [Fact]
        public void ConventionalStartupClass_WhenHasMulitpleOverrideConfigurePerTenantServicesWithEnv_ThrowsIfStartupBuildsTheContainerAsync()
        {
            // Arrange
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();
            ServiceProvider services = serviceCollection.BuildServiceProvider();

            // Act
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                // startup.ConfigureServicesDelegate(serviceCollection)
                StartupMethodsMultitenant<TestTenant> startup = StartupLoaderMultitenant.
                LoadMethods<TestTenant>(services, typeof(TestStartupMulitpleOverrideConfigurePerTenantServicesWithEnv), "IntegrationTest");
            });

            // Assert
            string expectedMessage = $"Having multiple overloads of method 'ConfigurePerTenantIntegrationTestServices' is not supported.";
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void ConventionalStartupClass_WhenHasMulitpleOverrideConfigurePerTenantServicesWitouthEnv_ThrowsIfStartupBuildsTheContainerAsync()
        {
            // Arrange
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();
            ServiceProvider services = serviceCollection.BuildServiceProvider();

            // Act
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                StartupMethodsMultitenant<TestTenant> startup = StartupLoaderMultitenant.
                LoadMethods<TestTenant>(services, typeof(TestStartupMulitpleOverrideConfigurePerTenantServicesWithoutEnv), string.Empty);
            });

            // Assert
            string expectedMessage = $"Having multiple overloads of method 'ConfigurePerTenantServices' is not supported.";
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureServicesHasInvalidOperationException_ThenThrowsInvalidOperationException()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartupConfigureServicesInvalidOperationException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(Res).ConfigureAwait(false);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureServicesHasException_ThenThrowsException()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartupConfigureServicesException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<Exception>(Res).ConfigureAwait(false);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureHasInvalidOperationException_ThenThrowsInvalidOperationException()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartupConfigureInvalidOperationException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(Res).ConfigureAwait(false);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        [Fact]
        public async Task ConventionalStartupClass_ConfigureHasException_ThenThrowsException()
        {
            // Arrange
            WebHostBuilder builder = CreateWebHostBuilder<TestStartupConfigureException, TestTenant, TestTenantMemoryCacheResolver>();

            // Act
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<Exception>(Res).ConfigureAwait(false);
            Assert.Equal("TestStartupInvalidOperationException", ex.Message);
        }

        private class TestStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant, MultiTenancyConfig config)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
                });
            }
        }

        private class TestStartup2
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void ConfigurePerTenantServices(IServiceCollection services, MultiTenancyConfig config)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
                });
            }
        }

        private class TestStartupValidArguments
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
                });
            }
        }

        private class TestStartupMulitpleOverrideConfigurePerTenantServicesWithEnv
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

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
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
                });
            }
        }

        private class TestStartupMulitpleOverrideConfigurePerTenantServicesWithoutEnv
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

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
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
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
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
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
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
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
