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
    using System.Threading.Tasks;

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
                IOptionsMonitor<MultitenancyOptions<TTenant>> monitor = builder.ApplicationServices.GetService<IOptionsMonitor<MultitenancyOptions<TTenant>>>();
                if (monitor != null)
                {
                    monitor.OnChangeDelayed(x =>
                    {
                        Task.Delay(1000).ContinueWith((continuationAction) =>
                        {
                            GetService<ITenantResolver<TTenant>>()?.Reset();
                            GetService<IServiceFactoryForMultitenancy<TTenant>>()?.RemoveAll();
                            Console.WriteLine($" Configuration changed. ");
                        });

                        //Thread.Sleep(200);
                        //GetService<ITenantResolver<TTenant>>()?.Reset();
                        //GetService<IServiceFactoryForMultitenancy<TTenant>>()?.RemoveAll();
                        //Console.WriteLine($" Configuration changed. ");
                    });

                    T GetService<T>()
                    {
                        return builder.ApplicationServices.GetService<T>();
                    }
                }


                builder.UseMultitenancy<TTenant>();
                builder.UseMiddleware<MultitenancyRequestServicesContainerMiddleware<TTenant>>();
                //builder.UsePerTenant<TStartup>((ctx, innerBuilder) => { });

                next(builder);
            };
        }
    }
}
