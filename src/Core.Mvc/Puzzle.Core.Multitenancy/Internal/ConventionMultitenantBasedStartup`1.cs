namespace Puzzle.Core.Multitenancy.Internal
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;

    internal class ConventionMultitenantBasedStartup<TTenant> : IStartup
    {
        private readonly StartupMethodsMultitenant<TTenant> methods;

        public ConventionMultitenantBasedStartup(StartupMethodsMultitenant<TTenant> methods)
        {
            this.methods = methods ?? throw new ArgumentNullException($"Argument {nameof(methods)} must not be null");
        }

        public void Configure(IApplicationBuilder app)
        {
            try
            {
                app.ApplicationServices.GetService<IOptionsMonitor<MultitenancyOptions<TTenant>>>();
                methods.ConfigureDelegate(app);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                using (ServiceProvider provider = methods.ConfigureServicesDelegate(services) as ServiceProvider)
                {
                    IOptionsMonitor<MultitenancyOptions<TTenant>> optionsMonitor = provider.GetRequiredService<IOptionsMonitor<MultitenancyOptions<TTenant>>>();
                    services.AddScoped<IServiceFactoryForMultitenancy<TTenant>>(_ =>
                    {
                        return new ServiceFactoryForMultitenancy<TTenant>(services.Clone(), methods.ConfigurePerTenantServicesDelegate, optionsMonitor);
                    });
                }

                IServiceProvider serviceProvider = methods.ConfigureServicesDelegate(services);
                return serviceProvider;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }
    }
}
