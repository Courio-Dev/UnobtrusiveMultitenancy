namespace PuzzleCMS.WebHost
{
    using System;
    using System.Net;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Internal;

    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILoggerFactory loggerFactory;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            this.hostingEnvironment = hostingEnvironment ?? throw new ArgumentException(nameof(hostingEnvironment));
            this.configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            this.loggerFactory = loggerFactory ?? throw new ArgumentException(nameof(loggerFactory));
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void ConfigurePerTenantServices(IServiceCollection services, AppTenant tenant)
        {
            if (tenant.Id.ToUpperInvariant() == "Tenant-1".ToUpperInvariant())
            {
                services.AddMvc();
            }
            else if (tenant.Id.ToUpperInvariant() == "Tenant-2".ToUpperInvariant())
            {
            }
        }

        public void Configure(IApplicationBuilder application, IApplicationLifetime appLifetime)
        {
            if (hostingEnvironment.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
            }

            application.UseStaticFiles();

            application.UsePerTenant<AppTenant>((tenantContext, builder) =>
            {
                if (tenantContext.Tenant.Id.ToUpperInvariant() == "Tenant-1".ToUpperInvariant())
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
