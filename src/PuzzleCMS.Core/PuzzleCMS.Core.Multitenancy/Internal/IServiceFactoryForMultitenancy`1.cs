namespace PuzzleCMS.Core.Multitenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;
    using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

    internal interface IServiceFactoryForMultitenancy<TTenant>
    {
        IServiceProvider Build(TenantContext<TTenant> tenantContext);

        void RemoveAll();
    }

    internal class ServiceFactoryForMultitenancy<TTenant> : IServiceFactoryForMultitenancy<TTenant>
    {
        public ServiceFactoryForMultitenancy(
            IServiceCollection services, 
            Action<IServiceCollection, TTenant> configurePerTenantServicesDelegate,
            Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> additionnalServicesTenant,
            IMultitenancyOptionsProvider<TTenant> multitenancyProvider,
            IOptionsMonitor<MultitenancyOptions<TTenant>> optionsMonitor)
            : this()
        {

            OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            Services = services ?? throw new ArgumentNullException($"Argument {nameof(services)} must not be null");
            MultitenancyOptionsProvider = multitenancyProvider ?? throw new ArgumentNullException($"Argument {nameof(multitenancyProvider)} must not be null");
            ConfigurePerTenantServicesDelegate = configurePerTenantServicesDelegate ?? throw new ArgumentNullException($"Argument {nameof(configurePerTenantServicesDelegate)} must not be null");
            AdditionnalServicesTenant = additionnalServicesTenant ?? throw new ArgumentNullException($"Argument {nameof(additionnalServicesTenant)} must not be null");
        }

        private ServiceFactoryForMultitenancy() {}

        public IServiceCollection Services { get; }

        public IOptionsMonitor<MultitenancyOptions<TTenant>> OptionsMonitor { get; }

        public IMultitenancyOptionsProvider<TTenant> MultitenancyOptionsProvider { get; }

        public Action<IServiceCollection, TTenant> ConfigurePerTenantServicesDelegate { get; }

        public Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> AdditionnalServicesTenant { get; }

        private static LazyConcurrentDictionary<string, IServiceProvider> Cache { get; } = new LazyConcurrentDictionary<string, IServiceProvider>();

        public IServiceProvider Build(TenantContext<TTenant> tenantContext)
        {
            string key = tenantContext.Id;

            IServiceProvider value = Cache.GetOrAdd(key, (k) =>
            {
                int position = tenantContext.Position;
                IConfiguration tenantConfiguration = MultitenancyOptionsProvider
                                                     ?.MultitenancyOptions
                                                     ?.TenantsConfigurations
                                                     ?.FirstOrDefault(x => string.Equals(x.Key, position.ToString(), StringComparison.OrdinalIgnoreCase));

                IServiceCollection serviceCollection = Services.Clone();    

                // Add specific tenant services to servicecollection.
                ConfigurePerTenantServicesDelegate(serviceCollection, tenantContext.Tenant);

                // Add tenant services to servicecollection.
                if (tenantConfiguration != null)
                {
                    BuildAddTenantServiceCollection(serviceCollection, tenantContext.Tenant, tenantConfiguration);
                }

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

        private void BuildAddTenantServiceCollection(IServiceCollection collectionClone, TTenant tenant, IConfiguration tenantConfiguration)
        {
            using (ServiceProvider provider = collectionClone.BuildServiceProvider())
            {
                OverrideHostingEnvironnementForTenant(collectionClone, provider);
                OverrideLoggerFactoryForTenant(collectionClone, provider, tenant, tenantConfiguration);
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

        private ILogProvider OverrideLoggerFactoryForTenant(
            IServiceCollection collectionClone, 
            ServiceProvider provider, 
            TTenant tenant, 
            IConfiguration tenantConfiguration)
        {
            ILogProvider logProvider = AdditionnalServicesTenant?.Invoke(collectionClone, tenant, tenantConfiguration);
            if (logProvider != null)
            {
                collectionClone.RemoveAll(typeof(ILogProvider));
                collectionClone.RemoveAll(typeof(ILog<>));

                collectionClone.TryAdd(ServiceDescriptor.Singleton<ILogProvider>(logProvider));             
                collectionClone.TryAdd(ServiceDescriptor.Singleton(typeof(ILog<>), typeof(Log<>)));
            }

            return logProvider;
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
