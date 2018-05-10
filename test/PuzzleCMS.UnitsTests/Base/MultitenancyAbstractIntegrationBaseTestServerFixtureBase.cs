namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    public class MultitenancyAbstractIntegrationBaseTestServerFixtureBase : MultitenancyBaseFixture
    {
        protected const string Appsettings = "appsettings";

        // internal string UrlTenant1 { get; } = "/tenant-1-1";
        // internal string UrlTenant2 { get; } = "/tenant-2-1";
        public MultitenancyAbstractIntegrationBaseTestServerFixtureBase()
            : base()
        {
        }

        protected internal TestServer Server { get; } = new TestServer(CreateWebHostBuilder<TestTransientStartup, TestTenant, TestTenantMemoryCacheResolver>());

        protected internal HttpClient ClientTransient { get; } = new TestServer(CreateWebHostBuilder<TestTransientStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        protected internal HttpClient ClientSingleton { get; } = new TestServer(CreateWebHostBuilder<TestSingletonStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        protected internal HttpClient ClientScoped { get; } = new TestServer(CreateWebHostBuilder<TestScopedStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        protected internal HttpClient ClientOverrideSingleton { get; } = new TestServer(CreateWebHostBuilder<TestOverrideSingletonPerTenantStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        protected internal HttpClient ClientOverrideTransient { get; } = new TestServer(CreateWebHostBuilder<TestOverrideTransientPerTenantStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        public void Dispose()
        {
            Server?.Dispose();
            ClientTransient?.Dispose();
            ClientSingleton?.Dispose();
            ClientScoped?.Dispose();

            ClientOverrideTransient?.Dispose();
            ClientOverrideSingleton?.Dispose();
        }

        /// <summary>
        /// Dummy service that is used in tests.
        /// </summary>
        private class ValueTransientService
        {
            public ValueTransientService(string value) => Value = value;

            public string Value { get; }
        }

        /// <summary>
        /// Dummy service that is used in tests.
        /// </summary>
        private class ValueSingletonService
        {
            public ValueSingletonService(string value) => Value = value;

            public string Value { get; }
        }

        /// <summary>
        /// Dummy service that is used in tests.
        /// </summary>
        private class ValueScopedService
        {
            public ValueScopedService(string value) => Value = value;

            public string Value { get; }
        }

        private class TestTransientStartup
        {
            public TestTransientStartup()
            {
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient(typeof(ValueTransientService), (s) => new ValueTransientService(nameof(ValueTransientService) + Guid.NewGuid()));
            }

            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    ValueTransientService service = ctx.RequestServices.GetRequiredService<ValueTransientService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>").ConfigureAwait(false);
                });
            }
        }

        private class TestOverrideTransientPerTenantStartup
        {
            public TestOverrideTransientPerTenantStartup()
            {
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient(typeof(ValueTransientService), (s) => new ValueTransientService(nameof(ValueTransientService) + Guid.NewGuid()));
            }

            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
                if (tenant.Name == "Tenant 1")
                {
                    services.AddTransient(typeof(ValueTransientService), (s) => new ValueTransientService("ValueTransientService_Override_Tenant1"));
                }
                else if (tenant.Name == "Tenant 2")
                {
                    services.AddTransient(typeof(ValueTransientService), (s) => new ValueTransientService("ValueTransientService_Override_Tenant2"));
                }
            }

            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    ValueTransientService service = ctx.RequestServices.GetRequiredService<ValueTransientService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>").ConfigureAwait(false);
                });
            }
        }

        private class TestOverrideSingletonPerTenantStartup
        {
            public TestOverrideSingletonPerTenantStartup()
            {
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(typeof(ValueTransientService), (s) => new ValueTransientService(nameof(ValueTransientService) + Guid.NewGuid()));
            }

            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
                if (tenant.Name == "Tenant 1")
                {
                    services.AddSingleton(typeof(ValueTransientService), (s) => new ValueTransientService("ValueSingletonService_Override_Tenant1"));
                }
                else if (tenant.Name == "Tenant 2")
                {
                    services.AddSingleton(typeof(ValueTransientService), (s) => new ValueTransientService("ValueSingletonService_Override_Tenant2"));
                }
            }

            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    ValueTransientService service = ctx.RequestServices.GetRequiredService<ValueTransientService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>").ConfigureAwait(false);
                });
            }
        }

        private class TestSingletonStartup
        {
            public TestSingletonStartup()
            {
            }

            public void ConfigureServices(IServiceCollection services)
            {
                string key = nameof(ValueSingletonService) + Guid.NewGuid();
                ValueSingletonService instance = new ValueSingletonService(key);
                services.AddSingleton(typeof(ValueSingletonService), (s) => instance);
            }

            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    // ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    ValueSingletonService service = ctx.RequestServices.GetRequiredService<ValueSingletonService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>").ConfigureAwait(false);
                });
            }
        }

        private class TestScopedStartup
        {
            public TestScopedStartup()
            {
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddScoped(typeof(ValueScopedService), (s) => new ValueScopedService(nameof(ValueScopedService) + Guid.NewGuid()));
            }

            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
            }

            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    ValueScopedService service = ctx.RequestServices.GetRequiredService<ValueScopedService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>").ConfigureAwait(false);
                });
            }
        }
    }
}
