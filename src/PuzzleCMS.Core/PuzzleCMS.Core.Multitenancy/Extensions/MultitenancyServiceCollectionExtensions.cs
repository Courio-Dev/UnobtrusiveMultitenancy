namespace PuzzleCMS.Core.Multitenancy.Extensions
{
    using System;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using PuzzleCMS.Core.Multitenancy.Internal;
    using PuzzleCMS.Core.Multitenancy.Internal.Resolvers;

    internal static class MultitenancyServiceCollectionExtensions
    {
        /// <summary>
        /// https://github.com/saaskit/saaskit/issues/68
        /// https://github.com/saaskit/saaskit/pull/69/files
        ///
        /// https://github.com/aspnet/HttpAbstractions/blob/ab0185a0b8d0b7a80a6169fd78a45f00a28e057d/src/Microsoft.AspNetCore.Http.Extensions/UriHelper.cs.
        /// 
        /// </summary>
        /// <typeparam name="TTenant">The Tenant class.</typeparam>
        /// <typeparam name="TResolver">The Resolver which resoles tenant.</typeparam>
        /// <param name="services">An IServiceCollection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddMultitenancy<TTenant, TResolver>(this IServiceCollection services)
            where TResolver : class, ITenantResolver<TTenant>
            where TTenant : class
        {
            if (services == null)
            {
                throw new ArgumentNullException($"Argument {nameof(services)} must not be null");
            }

            services.AddSingleton<ITenantResolver<TTenant>, TResolver>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Make Tenant and TenantContext injectable
            services.AddScoped(prov => prov.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>());

            // https://github.com/saaskit/saaskit/issues/86
            // https://github.com/saaskit/saaskit/issues/76
            // WARNING don't resolve TenantContext here, see https://github.com/saaskit/saaskit/issues/68
            // https://github.com/dazinator/saaskit/commit/735a507d980d9d2e0c5ec3961181f5873dade4e7
            services.AddScoped(prov =>
            {
                // WARNING don't resolve TenantContext here, see https://github.com/saaskit/saaskit/issues/68
                TenantContext<TTenant> context = prov.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext<TTenant>();
                TTenant tenant = context?.Tenant;
                return tenant;
            });

            // Make ITenant injectable for handling null injection, similar to IOptions
            services.AddScoped<ITenant<TTenant>>(prov => new TenantWrapper<TTenant>(prov.GetService<TTenant>()));

            // Ensure caching is available for caching resolvers
            Type resolverType = typeof(TResolver);
            if (typeof(MemoryCacheTenantResolver<TTenant>).IsAssignableFrom(resolverType))
            {
                services.AddMemoryCache();
            }

            return services;
        }
    }
}
