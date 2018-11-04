namespace PuzzleCMS.Web.Hosting
{
    using System;
    using System.Net;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal;

    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostingEnvironment HostingEnvironment { get; }
        private  ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="hostingEnvironment">The environment the application is running under. This can be Development,
        /// Staging or Production by default.</param>
        /// /// <param name="loggerFactory">The type used to configure the applications logging system.
        /// See http://docs.asp.net/en/latest/fundamentals/logging.html.</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            HostingEnvironment = hostingEnvironment ?? throw new ArgumentException(nameof(hostingEnvironment));
            Configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            LoggerFactory = loggerFactory ?? throw new ArgumentException(nameof(loggerFactory));
        }

        /// <summary>
        /// Configures the services to add to the ASP.NET Core Injection of Control (IoC) container. This method gets
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx.
        /// </summary>
        /// <param name="services">The services collection or IoC container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Configures the services for specific tenant to add to the ASP.NET Core Injection of Control (IoC) container. This method gets
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx.
        /// </summary>
        /// <param name="services">The services collection or IoC container.</param>
        /// <param name="tenant">The tenant object.</param>
        public void ConfigurePerTenantServices(IServiceCollection services,in AppTenant tenant,in IConfiguration tenantConfiguration)
        {
            if (tenant.Id.ToUpperInvariant().StartsWith("Tenant-1".ToUpperInvariant()))
            {
                services.AddMvc();
            }
        }

        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after ConfigureServices is
        /// called by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application, IApplicationLifetime appLifetime)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
            }

            application.UseStaticFiles();

            application.UsePerTenant<AppTenant>((tenantContext, builder) =>
            {
                if (tenantContext.Tenant.Id.ToUpperInvariant().StartsWith("Tenant-1".ToUpperInvariant()))
                {
                    builder.UseMvcWithDefaultRoute();
                }
                else if (tenantContext.Tenant.Id.ToUpperInvariant() == "Tenant-2".ToUpperInvariant())
                {
                    builder.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        await ctx.Response.WriteAsync(text: string.Format("{0} Without MVC", tenantContext.Tenant.Name)).ConfigureAwait(false);
                    });
                }
            });
        }
    }
}
