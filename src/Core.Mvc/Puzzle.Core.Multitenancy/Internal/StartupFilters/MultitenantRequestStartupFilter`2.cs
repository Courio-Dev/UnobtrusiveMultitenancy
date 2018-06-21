namespace Puzzle.Core.Multitenancy.Internal.StartupFilters
{
    using System;
    using System.Threading;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal.Middlewares;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The StartupFilter for multitenant.
    /// </summary>
    /// <typeparam  name="TStartup">The startup class.</typeparam >
    /// <typeparam  name="TTenant">The tenant object.</typeparam >
    internal sealed class MultitenantRequestStartupFilter<TStartup, TTenant> : IStartupFilter
         where TStartup : class
         where TTenant : class
    {
        public MultitenantRequestStartupFilter()
        {
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                IOptionsMonitor<MultitenancyOptions<TTenant>> t = builder.ApplicationServices.GetService<IOptionsMonitor<MultitenancyOptions<TTenant>>>();
                if (t != null)
                {
                    t.OnChangeDelayed(x => {

                        Thread.Sleep(200);

                        GetService<ITenantResolver<TTenant>>()?.Reset();
                        GetService<IServiceFactoryForMultitenancy<TTenant>>()?.RemoveAll();

                        Console.WriteLine($" Configuration changed. ");
                    });

                    T GetService<T>()
                    {
                        return builder.ApplicationServices.GetService<T>();
                    }
                }


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
