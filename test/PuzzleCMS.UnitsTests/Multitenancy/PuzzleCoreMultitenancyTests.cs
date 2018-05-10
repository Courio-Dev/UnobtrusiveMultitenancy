namespace PuzzleCMS.UnitsTests.Multitenancy
{
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Puzzle.Core.Multitenancy.Constants;
    using Puzzle.Core.Multitenancy.Extensions;
    using PuzzleCMS.UnitsTests.Base;
    using Xunit;

    public class PuzzleCoreMultitenancyTests : IClassFixture<MultitenancyAbstractIntegrationBaseTestServerFixtureBase>
    {
        protected const string Appsettings = "appsettings";

        public PuzzleCoreMultitenancyTests(MultitenancyAbstractIntegrationBaseTestServerFixtureBase testServerFixture)
        {
            ClientTransient = testServerFixture.ClientTransient;
            ClientSingleton = testServerFixture.ClientSingleton;
            ClientScoped = testServerFixture.ClientScoped;

            Server = testServerFixture.Server;

            UrlTenant1 = testServerFixture.UrlTenant1;
            UrlTenant2 = testServerFixture.UrlTenant2;

            ClientOverrideTransient = testServerFixture.ClientOverrideTransient;
            ClientOverrideSingleton = testServerFixture.ClientOverrideSingleton;

            TestServerFixture = testServerFixture;
        }

        public MultitenancyAbstractIntegrationBaseTestServerFixtureBase TestServerFixture { get; }

        protected TestServer Server { get; }

        protected HttpClient ClientTransient { get; }

        protected HttpClient ClientSingleton { get; }

        protected HttpClient ClientScoped { get; }

        protected HttpClient ClientOverrideTransient { get; }

        protected HttpClient ClientOverrideSingleton { get; }

        protected string UrlTenant1 { get; }

        protected string UrlTenant2 { get; }

        [Fact]
        public async Task FallbackToDefaultUseStartupBehavior_WhenMultitenantOptionsIsNotProvided()
        {
            // Arrange
            WebHostBuilder CreateWebHostBuilder()
            {
                WebHostBuilder webHostBuilder = new WebHostBuilder();
                webHostBuilder.UseEnvironment("IntegrationTest");
                webHostBuilder.UseKestrel();
                webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                webHostBuilder.UseUnobtrusiveMulitenancyStartup<DefaultTestStartup>(null);

                return webHostBuilder;
            }

            // Act
            WebHostBuilder builder = CreateWebHostBuilder();
            using (TestServer server = new TestServer(builder))
            using (HttpClient client = server.CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync("/").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Assert
                Assert.Null(builder.GetSetting(MultitenancyConstants.UseUnobstrusiveMulitenancyStartupKey));
                Assert.Equal("Default", result);
                builder = null;
            }
        }

        [Fact]
        public async Task FallbackToDefaultUseStartupBehavior_WhenMultitenantOptionsDefaultConventionIsNotFound()
        {
            // Arrange
            WebHostBuilder CreateWebHostBuilder()
            {
                WebHostBuilder webHostBuilder = new WebHostBuilder();
                webHostBuilder.UseEnvironment("IntegrationTest");
                webHostBuilder.UseKestrel();
                webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                webHostBuilder.UseUnobtrusiveMulitenancyStartupWithDefaultConvention<DefaultTestStartup>();

                return webHostBuilder;
            }

            // Act
            WebHostBuilder builder = CreateWebHostBuilder();
            using (TestServer server = new TestServer(builder))
            using (HttpClient client = server.CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync("/").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Assert
                Assert.Null(builder.GetSetting(MultitenancyConstants.UseUnobstrusiveMulitenancyStartupKey));
                Assert.Equal("Default", result);
                builder = null;
            }
        }

        [Fact]
        public void MulitenancyStartupKeyIsSet_WhenMultitenantOptionsIsUsed()
        {
            // Arrange
            WebHostBuilder CreateWebHostBuilder()
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"{Appsettings}.json", optional: false, reloadOnChange: true)
                    .Build()
                    ;

                WebHostBuilder webHostBuilder = new WebHostBuilder();
                webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                webHostBuilder.UseUnobtrusiveMulitenancyStartup<DefaultTestStartup>(config);

                return webHostBuilder;
            }

            string startupAssemblyName = typeof(DefaultTestStartup).GetTypeInfo().Assembly.GetName().Name;

            // Act
            WebHostBuilder builder = CreateWebHostBuilder();
            using (TestServer server = new TestServer(builder))
            using (HttpClient client = server.CreateClient())
            {
                // Assert
                string setting = builder.GetSetting(MultitenancyConstants.UseUnobstrusiveMulitenancyStartupKey);
                Assert.NotNull(setting);
                Assert.Equal(setting, startupAssemblyName);
            }
        }

        [Fact]
        public async Task TransientServiceIsDefferentperTenant_WhenUseMultitenancy()
        {
            // Act
            string responseFirstTenant_11 = await ClientTransient.GetStringAsync(UrlTenant1).ConfigureAwait(false);
            string responseFirstTenant_12 = await ClientTransient.GetStringAsync(UrlTenant1).ConfigureAwait(false);

            string responseSecondTenant_21 = await ClientTransient.GetStringAsync(UrlTenant2).ConfigureAwait(false);
            string responseSecondTenant_22 = await ClientTransient.GetStringAsync(UrlTenant2).ConfigureAwait(false);

            // Assert
            string[] col = new[] { responseFirstTenant_11, responseFirstTenant_12, responseSecondTenant_21, responseSecondTenant_22 };
            Assert.All(col, x => Assert.NotNull(x));
            Assert.All(col, x => Assert.NotEqual(string.Empty, x));

            // Begin with tenant1.
            Assert.All(new[] { responseFirstTenant_11, responseFirstTenant_12 }, x => Assert.StartsWith("Tenant 1", x));

            // Begin with tenant2.
            Assert.All(new[] { responseSecondTenant_21, responseSecondTenant_22 }, x => Assert.StartsWith("Tenant 2", x));

            Assert.NotEqual(responseFirstTenant_11, responseFirstTenant_12);
            Assert.NotEqual(responseFirstTenant_11, responseSecondTenant_21);
        }

        [Fact]
        public async Task SingletonServiceIsDefferentPerTenant_WhenUseMultitenancy()
        {
            // Act
            string responseFirstTenant_11 = await ClientSingleton.GetStringAsync(UrlTenant1).ConfigureAwait(false);
            string responseFirstTenant_12 = await ClientSingleton.GetStringAsync(UrlTenant1).ConfigureAwait(false);

            string responseSecondTenant_21 = await ClientSingleton.GetStringAsync(UrlTenant2).ConfigureAwait(false);
            string responseSecondTenant_22 = await ClientSingleton.GetStringAsync(UrlTenant2).ConfigureAwait(false);

            // Assert
            string[] col = new[] { responseFirstTenant_11, responseFirstTenant_12, responseSecondTenant_21, responseSecondTenant_22 };
            Assert.All(col, x => Assert.NotNull(x));
            Assert.All(col, x => Assert.NotEqual(string.Empty, x));

            // Begin with tenant1.
            Assert.All(new[] { responseFirstTenant_11, responseFirstTenant_12 }, x => Assert.StartsWith("Tenant 1", x));

            // Begin with tenant2.
            Assert.All(new[] { responseSecondTenant_21, responseSecondTenant_22 }, x => Assert.StartsWith("Tenant 2", x));

            // singleton is same between same tenant,and different with different tenant
            Assert.Equal(responseFirstTenant_11, responseFirstTenant_12);
            Assert.Equal(responseSecondTenant_21, responseSecondTenant_22);

            Assert.NotEqual(responseFirstTenant_11, responseSecondTenant_21);
        }

        [Fact]
        public async Task ScopedServiceIsDefferentperTenant_WhenUseMultitenancy()
        {
            // Act
            string responseFirstTenant_11 = await ClientScoped.GetStringAsync(UrlTenant1).ConfigureAwait(false);
            string responseFirstTenant_12 = await ClientScoped.GetStringAsync(UrlTenant1).ConfigureAwait(false);

            string responseSecondTenant_21 = await ClientScoped.GetStringAsync(UrlTenant2).ConfigureAwait(false);
            string responseSecondTenant_22 = await ClientScoped.GetStringAsync(UrlTenant2).ConfigureAwait(false);

            // Assert
            string[] col = new[] { responseFirstTenant_11, responseFirstTenant_12, responseSecondTenant_21, responseSecondTenant_22 };
            Assert.All(col, x => Assert.NotNull(x));
            Assert.All(col, x => Assert.NotEqual(string.Empty, x));

            // Begin with tenant1.
            Assert.All(new[] { responseFirstTenant_11, responseFirstTenant_12 }, x => Assert.StartsWith("Tenant 1", x));

            // Begin with tenant2.
            Assert.All(new[] { responseSecondTenant_21, responseSecondTenant_22 }, x => Assert.StartsWith("Tenant 2", x));

            // singleton is same between same tenant,and different with different tenant
            Assert.NotEqual(responseFirstTenant_11, responseFirstTenant_12);
            Assert.NotEqual(responseSecondTenant_21, responseSecondTenant_22);
            Assert.NotEqual(responseFirstTenant_11, responseSecondTenant_21);
        }

        [Fact]
        public async Task CanRegisterAndOverrideTransientServiceIsPerTenant_WhenUseMultitenancy()
        {
            // Act
            string responseFirstTenant = await ClientOverrideTransient.GetStringAsync(UrlTenant1).ConfigureAwait(false);
            string responseSecondTenant = await ClientOverrideTransient.GetStringAsync(UrlTenant2).ConfigureAwait(false);

            // Assert
            string[] col = new[] { responseFirstTenant, responseFirstTenant };
            Assert.All(col, x => Assert.NotNull(x));
            Assert.All(col, x => Assert.NotEqual(string.Empty, x));

            // Begin with tenant1.
            Assert.StartsWith("Tenant 1", responseFirstTenant);

            // Begin with tenant2.
            Assert.StartsWith("Tenant 2", responseSecondTenant);

            Assert.Equal("Tenant 1::ValueTransientService_Override_Tenant1", responseFirstTenant);
            Assert.Equal("Tenant 2::ValueTransientService_Override_Tenant2", responseSecondTenant);
        }

        [Fact]
        public async Task CanRegisterAndOverrideSingletonServiceIsPerTenant_WhenUseMultitenancy()
        {
            // Act
            string responseFirstTenant = await ClientOverrideSingleton.GetStringAsync(UrlTenant1).ConfigureAwait(false);
            string responseSecondTenant = await ClientOverrideSingleton.GetStringAsync(UrlTenant2).ConfigureAwait(false);

            // Assert
            string[] col = new[] { responseFirstTenant, responseFirstTenant };
            Assert.All(col, x => Assert.NotNull(x));
            Assert.All(col, x => Assert.NotEqual(string.Empty, x));

            // Begin with tenant1.
            Assert.StartsWith("Tenant 1", responseFirstTenant);

            // Begin with tenant2.
            Assert.StartsWith("Tenant 2", responseSecondTenant);

            Assert.Equal("Tenant 1::ValueSingletonService_Override_Tenant1", responseFirstTenant);
            Assert.Equal("Tenant 2::ValueSingletonService_Override_Tenant2", responseSecondTenant);
        }

        private class DefaultTestStartup
        {
            public void Configure(IApplicationBuilder application)
            {
                application.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Default").ConfigureAwait(false);
                });
            }
        }
    }
}
