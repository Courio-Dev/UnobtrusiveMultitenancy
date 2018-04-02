﻿namespace Puzzle.Core.Multitenancy.Internal.StartupFilters
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal.Middlewares;
    using Puzzle.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// MultitenantRequestStartupFilter
    /// <typeparamref name="TStartup" />
    /// <typeparamref name="TTenant"/>
    /// </summary>
    internal sealed class MultitenantRequestStartupFilter<TStartup, TTenant> : IStartupFilter
         where TStartup : class
         where TTenant : class
    {
        private readonly IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy;

        public MultitenantRequestStartupFilter(IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy)
            : this()
        {
            this.serviceFactoryForMultitenancy = serviceFactoryForMultitenancy;
        }

        private MultitenantRequestStartupFilter()
        {
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMultitenancy<TTenant>();

                // builder.UseMiddleware<TenantUnresolvedRedirectMiddleware<AppTenant>>("", false);
                builder.UseMiddleware<MultitenancyRequestServicesContainerMiddleware<TTenant>>();

                // builder.UsePerTenant<TStartup, TTenant>((ctx, innerBuilder) => { });
                next(builder);
            };
        }

        /*
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> nextFilter)
    {
        return builder =>
        {
            ConfigureRequestScoping(builder);

            nextFilter(builder);
        };
    }

    private void ConfigureRequestScoping(IApplicationBuilder builder)
    {
        builder.Use(async (context, next) =>
        {
            using (var scope = this.requestScopeProvider())
            {
                await next();
            }
        });
    }
    */
    }
}
