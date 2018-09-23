namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using PuzzleCMS.Core.Multitenancy.Extensions;
    using PuzzleCMS.UnitsTests.Base;
    using Xunit;

    public class MiddlewareTests : MultitenancyBaseTest
    {
        public MiddlewareTests(MultitenancyBaseFixture testBaseFixture)
          : base(testBaseFixture)
        {
        }

        [Fact]
        public async Task ThrowExceptionWhenUserPertenant_HasConfigurationNullParameter()
        {
            // Arrange
            IWebHostBuilder builder = new WebHostBuilder().Configure(app =>
            {
                UsePerTenantApplicationBuilderExtensions.UsePerTenant<TestTenant>(app, null);
            });

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
            Assert.Contains("configuration", ex.Message);
        }

        [Fact]
        public async Task ThrowExceptionWhenUserPertenant_HasAppNullParameter()
        {
            // Arrange
            IWebHostBuilder builder = new WebHostBuilder().Configure(app =>
            {
                UsePerTenantApplicationBuilderExtensions.UsePerTenant<TestTenant>(null, null);
            });

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
            Assert.Contains("app", ex.Message);
        }

        [Fact]
        public async Task ThrowExceptionWhenUseMultitenancy_HasAppNullParameter()
        {
            // Arrange
            IWebHostBuilder builder = new WebHostBuilder().Configure(app =>
            {
                MultitenancyApplicationBuilderExtensions.UseMultitenancy<TestTenant>(null);
            });

            // Act
            // Assert
            Task Res() => Task.Run(() =>
            {
                TestServer server = new TestServer(builder);
            });

            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
            Assert.Contains("app", ex.Message);
        }

        [Fact]
        public async Task ThrowExceptionWhenUseUnobtrusiveMulitenancyStartupWithDefaultConvention_HasHostBuilderNullParameter()
        {
            // Arrange
            // Act
            Task Res() => Task.Run(() =>
            {
                PuzzleCMS.Core.Multitenancy.Extensions.WebHostBuilderExtensions.UseUnobtrusiveMulitenancyStartupWithDefaultConvention<TestStartup>(null);
            });

            // Assert
            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
            Assert.Contains("hostBuilder", ex.Message);
        }

        [Fact]
        public async Task ThrowExceptionWhenUseUnobtrusiveMulitenancyStartup_HasHostBuilderNullParameter()
        {
            // Arrange
            // Act
            Task Res() => Task.Run(() =>
            {
                PuzzleCMS.Core.Multitenancy.Extensions.WebHostBuilderExtensions.
                UseUnobtrusiveMulitenancyStartup<TestStartup, TestTenant, TestTenantMemoryCacheResolver>(null);
            });

            // Assert
            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
            Assert.Contains("hostBuilder", ex.Message);
        }

        [Fact]
        public async Task ThrowExceptionAddMultitenancy_HasServicesNullParameter()
        {
            // Arrange
            // Act
            Task Res() => Task.Run(() =>
            {
                MultitenancyServiceCollectionExtensions.AddMultitenancy<TestTenant, TestTenantMemoryCacheResolver>(null);
            });

            // Assert
            Exception ex = await Assert.ThrowsAsync<ArgumentNullException>(Res).ConfigureAwait(false);
            Assert.Contains("services", ex.Message);
        }

        /*
        [Fact]
        public async Task ThrowExceptionWhenHttpContextIsNull_InTenantPipelineMiddleware()
        {
            // Arrange
            // WebHostBuilder builder = CreateWebHostBuilder<TestStartup, TestTenant, TestTenantMemoryCacheResolver>();
            IConfigurationRoot config  = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                               .Build()
                               ;

            WebHostBuilder builder = new WebHostBuilder();
                builder.UseEnvironment("IntegrationTest");
                builder.UseKestrel();
                builder.UseContentRoot(Directory.GetCurrentDirectory());
                builder.UseUnobtrusiveMulitenancyStartup<TestStartup, TestTenant, TestTenantMemoryCacheResolver>(config);
                builder.Configure(app => {
                  //  app.ApplicationServices.GetService(typeof(TenantResolutionMiddleware<TestTenant>));
                });

            // Act
            using (TestServer server = new TestServer(builder))
            using (System.Net.Http.HttpClient client = server.CreateClient())
            {
                Task res() => Task.Run(async () =>
                {
                    System.Net.Http.HttpResponseMessage response = await client.GetAsync("/tenant-1-1").ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                });

                Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(res).ConfigureAwait(false);
                Assert.Equal(
                    "The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.",
                           ex.Message);
            }
        }
        */
    }
}
