using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Puzzle.Core.Multitenancy.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Puzzle.Core.Multitenancy.Internal.Middlewares
{
    internal class MultitenancyRequestServicesContainerMiddleware<TTenant>
    {
        private readonly RequestDelegate next;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy;

        public MultitenancyRequestServicesContainerMiddleware(RequestDelegate next, IHttpContextAccessor contextAccessor, IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            this.serviceFactoryForMultitenancy = serviceFactoryForMultitenancy ?? throw new ArgumentNullException(nameof(serviceFactoryForMultitenancy));
        }

        /// <summary>
        /// Invokes the middleware using the specified context.
        /// </summary>
        /// <param name="context">
        /// The request context to process through the middleware.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> to await for completion of the operation.
        /// </returns>
        public async Task Invoke(HttpContext httpContext)
        {
            Debug.Assert(httpContext != null);
            if (serviceFactoryForMultitenancy == null) throw new ArgumentNullException(nameof(serviceFactoryForMultitenancy));

            var tenantContext = httpContext.GetTenantContext<TTenant>();
            if (tenantContext != null)
            {
                var existingRequestServices = httpContext.RequestServices;

                using (var feature = new RequestServicesFeature(serviceFactoryForMultitenancy.Build(tenantContext).GetRequiredService<IServiceScopeFactory>()))
                {
                    try
                    {
                        // Replace the request IServiceProvider created by IServiceScopeFactory
                        httpContext.RequestServices = feature.RequestServices;
                        await this.next.Invoke(httpContext);
                    }
                    finally
                    {
                        //httpContext.RequestServices = feature.RequestServices;
                        //httpContext.RequestServices = existingRequestServices;
                    }
                }
            }
        }
    }

    /*
    internal class MultitenancyRequestServicesContainerMiddleware<TTenant>
    {
        private readonly RequestDelegate next;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy;

        public MultitenancyRequestServicesContainerMiddleware(RequestDelegate next, IHttpContextAccessor contextAccessor, IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            this.serviceFactoryForMultitenancy = serviceFactoryForMultitenancy ?? throw new ArgumentNullException(nameof(serviceFactoryForMultitenancy));
        }

        /// <summary>
        /// Invokes the middleware using the specified context.
        /// </summary>
        /// <param name="context">
        /// The request context to process through the middleware.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> to await for completion of the operation.
        /// </returns>
        public async Task Invoke(HttpContext httpContext)
        {
            Debug.Assert(httpContext != null);

            //// local cache for virtual disptach result
            //var features = httpContext.Features;
            //var existingFeature = features.Get<IServiceProvidersFeature>();

            // All done if RequestServices is set
            // If there isn't already an HttpContext set on the context
            // accessor for this async/thread operation, set it. This allows
            // tenant identification to use it.
            if (contextAccessor.HttpContext == null)
            {
                contextAccessor.HttpContext = httpContext;
            }

            //// All done if RequestServices is set
            //if (existingFeature?.RequestServices != null)
            //{
            //    await next.Invoke(httpContext);
            //    return;
            //}

            if (serviceFactoryForMultitenancy == null) throw new ArgumentNullException(nameof(serviceFactoryForMultitenancy));

            var tenantContext = httpContext.GetTenantContext<TTenant>();
            if (tenantContext != null)
            {
                using (var feature = new RequestServicesFeature(serviceFactoryForMultitenancy.Build(tenantContext).GetRequiredService<IServiceScopeFactory>()))
                {
                    // local cache for virtual disptach result
                    var features = httpContext.Features;
                    var existingFeature = features.Get<IServiceProvidersFeature>();
                    try
                    {
                        // Replace the request IServiceProvider created by IServiceScopeFactory
                        //context.RequestServices = requestContainer.GetInstance<IServiceProvider>();
                        httpContext.Features.Set<IServiceProvidersFeature>(feature);
                        await this.next.Invoke(httpContext);
                    }
                    finally
                    {
                        httpContext.Features.Set(existingFeature);
                    }
                }
            }
        }
    }
    */
}