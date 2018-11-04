namespace PuzzleCMS.Core.Multitenancy.Internal
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
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;
    using PuzzleCMS.Core.Multitenancy.Internal.Resolvers;

    internal class ConventionMultitenantBasedStartup<TTenant> : IStartup
    {
        private readonly StartupMethodsMultitenant<TTenant> methods;
        private readonly Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> additionnalServicesTenant;

        public ConventionMultitenantBasedStartup(
            StartupMethodsMultitenant<TTenant> methods, 
            Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> additionnalServicesTenant)
        {
            this.methods = methods ?? throw new ArgumentNullException($"Argument {nameof(methods)} must not be null");
            this.additionnalServicesTenant = additionnalServicesTenant ?? throw new ArgumentNullException($"Argument {nameof(additionnalServicesTenant)} must not be null");
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                ServiceProvider hostServiceprovider = methods.ConfigureServicesDelegate(services) as ServiceProvider;
                {
                    services.AddScoped<IServiceFactoryForMultitenancy<TTenant>>(_ =>
                    {
                        return new ServiceFactoryForMultitenancy<TTenant>(hostServiceprovider,services.Clone(), 
                            methods.ConfigurePerTenantServicesDelegate,
                            additionnalServicesTenant);
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

    }
}
