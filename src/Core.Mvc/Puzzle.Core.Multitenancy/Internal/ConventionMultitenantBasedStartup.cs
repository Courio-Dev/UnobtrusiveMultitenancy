﻿namespace Puzzle.Core.Multitenancy.Internal
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal class ConventionMultitenantBasedStartup<TTenant> : IStartup
    {
        private readonly StartupMethodsMultitenant<TTenant> methods;

        public ConventionMultitenantBasedStartup(StartupMethodsMultitenant<TTenant> methods)
        {
            Debug.Assert(methods != null, nameof(methods));

            this.methods = methods;
        }

        public void Configure(IApplicationBuilder app)
        {
            try
            {
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
                    IOptionsMonitor<MultitenancyOptions> optionsMonitor = provider.GetRequiredService<IOptionsMonitor<MultitenancyOptions>>();
                    services.AddSingleton<IServiceFactoryForMultitenancy<TTenant>>(_ =>
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
