using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PuzzleCMS.UnitsTests.Base
{
    public class MultitenancyAbstractIntegrationBaseTestServerFixtureBase : MultitenancyBaseFixture
    {
        private string[] urls = new string[] {
            "http://localhost:47887",
            "http://localhost:44301",
            "http://localhost:60000",
            "http://localhost:44302",
            "http://localhost:60001"
        };

        public string UrlTenant1 { get; } = "/tenant-1-1";
        public string UrlTenant2 { get; } = "/tenant-2-1";

        public TestServer Server { get; }
        public HttpClient ClientTransient { get; }
        public HttpClient ClientSingleton { get; }
        public HttpClient ClientScoped { get; }

        public HttpClient ClientOverrideTransient { get; }
        public HttpClient ClientOverrideSingleton { get; }

        private const string appsettingsForTest = "appsettings";

        public MultitenancyAbstractIntegrationBaseTestServerFixtureBase()
            : base()
        {
            // Arrange
            this.Server = new TestServer(CreateWebHostBuilder<TestTransientStartup, TestTenant, TestTenantMemoryCacheResolver>());
            this.ClientTransient = Server.CreateClient();

            this.ClientSingleton = new TestServer(CreateWebHostBuilder<TestSingletonStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();
            this.ClientScoped = new TestServer(CreateWebHostBuilder<TestScopedStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();

            /**/
            this.ClientOverrideTransient = new TestServer(CreateWebHostBuilder<TestOverrideTransientPerTenantStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();
            this.ClientOverrideSingleton = new TestServer(CreateWebHostBuilder<TestOverrideSingletonPerTenantStartup, TestTenant, TestTenantMemoryCacheResolver>()).CreateClient();
        }

        public void Dispose()
        {
            this.Server?.Dispose();
            //
            this.ClientTransient?.Dispose();
            this.ClientSingleton?.Dispose();
            this.ClientScoped?.Dispose();
        }

        /// <summary>
        /// Dummy service that is used in tests.
        /// </summary>
        private class ValueTransientService
        {
            public ValueTransientService(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        /// <summary>
        /// Dummy service that is used in tests.
        /// </summary>
        private class ValueSingletonService
        {
            public ValueSingletonService(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        /// <summary>
        /// Dummy service that is used in tests.
        /// </summary>
        private class ValueScopedService
        {
            public ValueScopedService(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        private class TestTransientStartup
        {
            public TestTransientStartup()
            {
            }

            /// <inheritdoc />
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient(typeof(ValueTransientService), (s) => new ValueTransientService(nameof(ValueTransientService) + Guid.NewGuid()));
            }

            /// <inheritdoc />
            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    var service = ctx.RequestServices.GetRequiredService<ValueTransientService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>");
                });
            }
        }

        private class TestOverrideTransientPerTenantStartup
        {
            public TestOverrideTransientPerTenantStartup()
            {
            }

            /// <inheritdoc />
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient(typeof(ValueTransientService), (s) => new ValueTransientService(nameof(ValueTransientService) + Guid.NewGuid()));
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    var service = ctx.RequestServices.GetRequiredService<ValueTransientService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>");
                });
            }
        }

        private class TestOverrideSingletonPerTenantStartup
        {
            public TestOverrideSingletonPerTenantStartup()
            {
            }

            /// <inheritdoc />
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(typeof(ValueTransientService), (s) => new ValueTransientService(nameof(ValueTransientService) + Guid.NewGuid()));
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    var service = ctx.RequestServices.GetRequiredService<ValueTransientService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>");
                });
            }
        }

        private class TestSingletonStartup
        {
            public TestSingletonStartup()
            {
            }

            /// <inheritdoc />
            public void ConfigureServices(IServiceCollection services)
            {
                var key = nameof(ValueSingletonService) + Guid.NewGuid();
                var instance = new ValueSingletonService(key);
                services.AddSingleton(typeof(ValueSingletonService), (s) => instance);
            }

            /// <inheritdoc />
            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    //ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    var service = ctx.RequestServices.GetRequiredService<ValueSingletonService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>");
                });
            }
        }

        private class TestScopedStartup
        {
            public TestScopedStartup()
            {
            }

            /// <inheritdoc />
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddScoped(typeof(ValueScopedService), (s) => new ValueScopedService(nameof(ValueScopedService) + Guid.NewGuid()));
            }

            /// <inheritdoc />
            public void ConfigurePerTenantServices(IServiceCollection services, TestTenant tenant)
            {
            }

            /// <inheritdoc />
            public void Configure(IApplicationBuilder application)
            {
                application.UsePerTenant<TestTenant>((context, builder) =>
                {
                    builder.UseMiddleware<AddTenantNameMiddleware>(context.Tenant.Name);
                });

                application.Run(async ctx =>
                {
                    var service = ctx.RequestServices.GetRequiredService<ValueScopedService>();
                    await ctx.Response.WriteAsync(service?.Value ?? "<null>");
                });
            }
        }
    }

    internal class TestStartup
    {
        #region Constructors

        /// <inheritdoc />
        public TestStartup()
        {
        }

        #endregion Constructors

        #region Public Methods

        public void ConfigureTestServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public void Configure(IApplicationBuilder application)
        {
            application.Run(async ctx =>
            {
                await ctx.Response.WriteAsync(": Test");
            });
        }

        #endregion Public Methods
    }

    internal class TestTenant : IDisposable
    {
        public string Name { get; set; }
        public string[] Hostnames { get; set; }
        public string Theme { get; set; }
        public string ConnectionString { get; set; }

        public bool Disposed { get; set; }

        public CancellationTokenSource Cts = new CancellationTokenSource();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Cts.Cancel();
            }

            Disposed = true;
        }
    }

    internal class AddTenantNameMiddleware
    {
        private RequestDelegate next;
        private string name;

        public AddTenantNameMiddleware(RequestDelegate next, string name)
        {
            this.next = next;
            this.name = name;
        }

        public async Task Invoke(HttpContext context)
        {
            await context.Response.WriteAsync($"{name}::");
            await next(context);
        }
    }
}