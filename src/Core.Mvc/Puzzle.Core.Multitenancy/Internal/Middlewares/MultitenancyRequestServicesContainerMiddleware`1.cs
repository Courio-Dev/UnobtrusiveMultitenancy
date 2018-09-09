namespace Puzzle.Core.Multitenancy.Internal.Middlewares
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal.Logging;

    internal class MultitenancyRequestServicesContainerMiddleware<TTenant>
    {
        private readonly RequestDelegate next;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy;

        public MultitenancyRequestServicesContainerMiddleware(
            RequestDelegate next, 
            IHttpContextAccessor contextAccessor, 
            IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            this.serviceFactoryForMultitenancy = serviceFactoryForMultitenancy ?? throw new ArgumentNullException(nameof(serviceFactoryForMultitenancy));
        }

        /// <summary>
        /// Invokes the middleware using the specified context.
        /// </summary>
        /// <param name="httpContext">
        /// The request context to process through the middleware.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> to await for completion of the operation.
        /// </returns>
        public async Task Invoke(HttpContext httpContext)
        {
            Debug.Assert(httpContext != null, nameof(httpContext));

            TenantContext<TTenant> tenantContext = httpContext.GetTenantContext<TTenant>();
            if (tenantContext != null)
            {
                IServiceProvidersFeature existingRequestServices = httpContext.Features.Get<IServiceProvidersFeature>();

                using (RequestServicesFeature feature = 
                       new RequestServicesFeature(httpContext, serviceFactoryForMultitenancy.Build(tenantContext).GetRequiredService<IServiceScopeFactory>()))
                {
                    // Replace the request IServiceProvider created by IServiceScopeFactory
                    httpContext.RequestServices = feature.RequestServices;

                    ILog<MultitenancyRequestServicesContainerMiddleware<TTenant>> log = 
                        httpContext.RequestServices.GetRequiredService<ILog<MultitenancyRequestServicesContainerMiddleware<TTenant>>>();
                    log.Log(Logging.LibLog.LogLevel.Info, () => $"IServiceProvider is successfully set for tenant {tenantContext.Id}");

                   await next.Invoke(httpContext).ConfigureAwait(false);

                }
            }
        }
    }
}
