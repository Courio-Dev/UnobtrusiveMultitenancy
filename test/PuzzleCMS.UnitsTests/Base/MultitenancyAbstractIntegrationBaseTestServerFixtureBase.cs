namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base class for multitenancy test.
    /// </summary>
    public class MultitenancyAbstractIntegrationBaseTestServerFixtureBase : MultitenancyBaseFixture
    {
        /// <summary>
        /// The name of the settings file json.
        /// </summary>
        protected const string Appsettings = "appsettings";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyAbstractIntegrationBaseTestServerFixtureBase"/> class.
        /// </summary>
        public MultitenancyAbstractIntegrationBaseTestServerFixtureBase()
            : base()
        {
        }

        /// <summary>
        /// Gets object TestServer.
        /// </summary>
        protected internal TestServer Server { get; } = new TestServer(CreateWebHostBuilder<TestTransientStartup, TestTenant, TestTenantMemoryCacheResolver>());

        /// <summary>
        /// Gets test base HttpClient for transient registry.
        /// </summary>
        protected internal HttpClient ClientTransient { get; } = new TestServer(CreateWebHostBuilder<TestTransientStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        /// <summary>
        /// Gets test base HttpClient for singleton registry.
        /// </summary>
        protected internal HttpClient ClientSingleton { get; } = new TestServer(CreateWebHostBuilder<TestSingletonStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        /// <summary>
        /// Gets test base HttpClient for scope registry.
        /// </summary>
        protected internal HttpClient ClientScoped { get; } = new TestServer(CreateWebHostBuilder<TestScopedStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        /// <summary>
        /// Gets test base HttpClient for override singleton registry.
        /// </summary>
        protected internal HttpClient ClientOverrideSingleton { get; } = new TestServer(CreateWebHostBuilder<TestOverrideSingletonPerTenantStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        /// <summary>
        /// Gets test base HttpClient for override transient registry.
        /// </summary>
        protected internal HttpClient ClientOverrideTransient { get; } = new TestServer(CreateWebHostBuilder<TestOverrideTransientPerTenantStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

        /// <summary>
        /// Do the dispose.
        /// </summary>
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
        /// 
        /// </summary>
        protected internal void SetConfig(Dictionary<string, string> additionnal)
        {
            UpdateConfiguration(additionnal);
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
