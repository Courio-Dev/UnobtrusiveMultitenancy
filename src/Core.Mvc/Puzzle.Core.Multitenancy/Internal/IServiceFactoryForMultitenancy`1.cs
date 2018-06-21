namespace Puzzle.Core.Multitenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

    internal interface IServiceFactoryForMultitenancy<TTenant>
    {
        IServiceProvider Build(TenantContext<TTenant> tenantContext);

        void RemoveAll();
    }

    internal class ServiceFactoryForMultitenancy<TTenant> : IServiceFactoryForMultitenancy<TTenant>
    {
        private readonly IOptionsMonitor<MultitenancyOptions<TTenant>> optionsMonitor;

        public ServiceFactoryForMultitenancy(
            IServiceCollection services, Action<IServiceCollection, TTenant> configurePerTenantServicesDelegate, IOptionsMonitor<MultitenancyOptions<TTenant>> optionsMonitor)
            : this()
        {
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            Services = services;
            ConfigurePerTenantServicesDelegate = configurePerTenantServicesDelegate;
        }

        private ServiceFactoryForMultitenancy()
        {
        }

        public IServiceCollection Services { get; }

        public Action<IServiceCollection, TTenant> ConfigurePerTenantServicesDelegate { get; }

        private static LazyConcurrentDictionary<string, IServiceProvider> Cache { get; } = new LazyConcurrentDictionary<string, IServiceProvider>();

        public IServiceProvider Build(TenantContext<TTenant> tenantContext)
        {
            string key = tenantContext.Id;

            IServiceProvider value = Cache.GetOrAdd(key, (k) =>
            {
                IServiceCollection serviceCollection = Services.Clone();

                // Add plugin tenant services to servicecollection.
                BuildAddTenantServiceCollection(serviceCollection, tenantContext.Tenant);

                // Add specific tenant services to servicecollection.
                ConfigurePerTenantServicesDelegate(serviceCollection, tenantContext.Tenant);
                return GetProviderFromFactory(serviceCollection, tenantContext);
            });

            return value;
        }

        public void RemoveAll() => Cache.Clear();

        private static void CreateFolderIfNotExist(string path)
        {
            string dir = path;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private void BuildAddTenantServiceCollection(IServiceCollection collectionClone, TTenant tenant)
        {
            using (ServiceProvider provider = collectionClone.BuildServiceProvider())
            {
                OverrideHostingEnvironnementForTenant(collectionClone, provider);
                OverrideLoggerFactoryForTenant(collectionClone, provider, tenant);
            }
        }

        private IHostingEnvironment OverrideHostingEnvironnementForTenant(IServiceCollection collectionClone, ServiceProvider provider)
        {
            IHostingEnvironment env = provider.GetRequiredService<IHostingEnvironment>();

            // Replace IHostingEnvironment.
            string tenantContentRootFolderPath = Path.Combine(env.ContentRootPath, "App_Tenants");
            string tenantWebRootPath = Path.Combine(env.WebRootPath, "App_Tenants");
            CreateFolderIfNotExist(tenantContentRootFolderPath);
            CreateFolderIfNotExist(tenantWebRootPath);

            PhysicalFileProvider tenantContentRootFileProvider = new PhysicalFileProvider(tenantContentRootFolderPath);
            PhysicalFileProvider tenantWebRootFileProvider = new PhysicalFileProvider(tenantWebRootPath);
            env.ContentRootFileProvider = new CompositeFileProvider(tenantContentRootFileProvider, env.ContentRootFileProvider);
            env.WebRootFileProvider = new CompositeFileProvider(tenantWebRootFileProvider, env.ContentRootFileProvider);

            Replace<IHostingEnvironment>(collectionClone, () => env, ServiceLifetime.Singleton);
            return env;
        }

        private void OverrideLoggerFactoryForTenant(IServiceCollection collectionClone, ServiceProvider provider, TTenant tenant)
        {
            /*ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddTenantLogger(tenant, provider, LogLevel.Trace);
            collectionClone.AddSingleton(loggerFactory);*/
        }

        private IServiceProvider GetProviderFromFactory(IServiceCollection collectionClone, TenantContext<TTenant> tenantContext)
        {
            ServiceProvider provider = collectionClone.BuildServiceProvider();
            IServiceProviderFactory<IServiceCollection> factory = provider.GetService<IServiceProviderFactory<IServiceCollection>>();

            using (provider)
            {
                return factory.CreateServiceProvider(factory.CreateBuilder(collectionClone));
            }
        }

        // private IServiceCollection Remove<TService>(IServiceCollection services)
        //    where TService : class
        // {
        //    IEnumerable<ServiceDescriptor> descriptorListToRemove = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        //    foreach (ServiceDescriptor descriptorToRemove in descriptorListToRemove)
        //    {
        //        services.Remove(descriptorToRemove);
        //    }
        //    return services;
        // }
        private IServiceCollection Replace<TService>(IServiceCollection services, Func<TService> factory, ServiceLifetime lifetime)
            where TService : class
        {
            ICollection<ServiceDescriptor> descriptorListToRemove = services.Where(d => d.ServiceType == typeof(TService)).ToList();
            if (descriptorListToRemove?.Any() ?? false)
            {
                TService instance = factory();

                foreach (ServiceDescriptor descriptorToRemove in descriptorListToRemove)
                {
                    ServiceDescriptor descriptorToAdd = (lifetime == ServiceLifetime.Singleton) ?
                                                        new ServiceDescriptor(typeof(TService), instance) :
                                                        new ServiceDescriptor(typeof(TService), sp => instance, lifetime: lifetime);

                    int index = services.IndexOf(descriptorToRemove);
                    services.Insert(index, descriptorToAdd);
                    services.Remove(descriptorToRemove);
                }
            }

            return services;
        }

        /*private IServiceCollection Replace<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            ServiceDescriptor descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
            services.Remove(descriptorToRemove);
            ServiceDescriptor descriptorToAdd = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
            services.Add(descriptorToAdd);
            return services;
        }*/

        private class LazyConcurrentDictionary<TKey, TValue>
        {
            private readonly ConcurrentDictionary<TKey, Lazy<TValue>> concurrentDictionary;

            public LazyConcurrentDictionary()
            {
                concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            }

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                Lazy<TValue> lazyResult = concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));

                return lazyResult.Value;
            }

            public void Clear()
            {
                concurrentDictionary?.Clear();
            }
        }
    }
}
