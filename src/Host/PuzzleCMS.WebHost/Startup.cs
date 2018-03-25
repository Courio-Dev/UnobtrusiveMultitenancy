namespace PuzzleCMS.WebHost
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Internal;
    using System;
    using System.Net;

    /// <inheritdoc />
    public class Startup
    {
        #region Fields

        protected readonly TenantContext<AppTenant> tenantContext;
        protected readonly IConfiguration Configuration;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILoggerFactory loggerFactory;

        #endregion Fields

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            this.hostingEnvironment = hostingEnvironment ?? throw new ArgumentException(nameof(hostingEnvironment));
            this.Configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            this.loggerFactory = loggerFactory ?? throw new ArgumentException(nameof(loggerFactory));
        }

        #region Public Methods

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public void ConfigurePerTenantServices(IServiceCollection services, AppTenant tenant)
        {
            if (tenant.Id == "Tenant-1".ToLowerInvariant())
            {
                services.AddMvc();
            }
            else if (tenant.Id == "Tenant-2".ToLowerInvariant())
            {
            }
        }

        /// <inheritdoc />
        public void Configure(IApplicationBuilder application, IApplicationLifetime appLifetime)
        {
            if (hostingEnvironment.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
            }

            application.UseStaticFiles();

            application.UsePerTenant<AppTenant>((tenantContext, builder) =>
            {
                if (tenantContext.Tenant.Id == "Tenant-1".ToLowerInvariant())
                {
                    builder.UseMvcWithDefaultRoute();
                }
                else if (tenantContext.Tenant.Id == "Tenant-2".ToLowerInvariant())
                {
                    builder.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        await ctx.Response.WriteAsync(string.Format("{0} Without MVC", tenantContext.Tenant.Name));
                    });
                }
            });
        }

        #endregion Public Methods
    }
}