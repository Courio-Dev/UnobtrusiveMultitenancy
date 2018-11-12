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

    internal class ServiceFactoryForMultitenancyBase
    {
        private static readonly LazyConcurrentDictionary<string, IServiceProvider> cache = new LazyConcurrentDictionary<string, IServiceProvider>();

        protected ServiceFactoryForMultitenancyBase() { }

        protected static LazyConcurrentDictionary<string, IServiceProvider> GetCache()
        {
            return cache;
        }

    }

    internal class ServiceFactoryForMultitenancy<TTenant> : ServiceFactoryForMultitenancyBase, IServiceFactoryForMultitenancy<TTenant>
    {
        public ServiceFactoryForMultitenancy(
            IServiceProvider hostServiceprovider,
            IServiceCollection hostServices,
            Action<IServiceCollection, TTenant, IConfiguration> configurePerTenantServicesDelegate)
            : this()
        {
            this.hostServiceprovider = hostServiceprovider ?? throw new ArgumentNullException($"Argument {nameof(hostServiceprovider)} must not be null");
            this.hostServices = hostServices ?? throw new ArgumentNullException($"Argument {nameof(hostServices)} must not be null");
            ConfigurePerTenantServicesDelegate = configurePerTenantServicesDelegate ?? throw new ArgumentNullException($"Argument {nameof(configurePerTenantServicesDelegate)} must not be null");
        }

        protected ServiceFactoryForMultitenancy() : base() { }

        protected IServiceProvider hostServiceprovider { get; }

        protected IServiceCollection hostServices { get; }

        protected Action<IServiceCollection, TTenant, IConfiguration> ConfigurePerTenantServicesDelegate { get; }

        protected IMultitenancyOptionsProvider<TTenant> multiTenancyOptionsProvider
        {
            get
            {
                return hostServiceprovider.GetRequiredService<IMultitenancyOptionsProvider<TTenant>>();
            }
        }


        protected Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> additionnalServicesTenant
        {
            get
            {
                return multiTenancyOptionsProvider.TenantLogProvider;
            }
        }


        public IServiceProvider Build(TenantContext<TTenant> tenantContext)
        {
            string key = tenantContext.Id;

            IServiceProvider value = GetCache().GetOrAdd(key, (k) =>
            {
                int position = tenantContext.Position;
                IConfiguration tenantConfiguration = multiTenancyOptionsProvider
                                                     ?.MultitenancyOptions
                                                     ?.TenantsConfigurations
                                                     ?.FirstOrDefault(x => string.Equals(x.Key, position.ToString(), StringComparison.OrdinalIgnoreCase));

                IServiceCollection serviceCollection = hostServices.Clone();

                // Add specific tenant services to servicecollection.
                ConfigurePerTenantServicesDelegate(serviceCollection, tenantContext.Tenant, tenantConfiguration);

                // Add tenant services to servicecollection.
                if (tenantConfiguration != null)
                {
                    BuildAddTenantServiceCollection(serviceCollection, tenantContext.Tenant, tenantConfiguration);
                }

                return GetProviderFromFactory(serviceCollection);
            });

            return value;
        }

        public void RemoveAll() => GetCache().Clear();

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
                OverrideLoggerFactoryForTenant(collectionClone, tenant, tenantConfiguration);
            }
        }

        private void OverrideHostingEnvironnementForTenant(IServiceCollection collectionClone, ServiceProvider provider)
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
        }

        private void OverrideLoggerFactoryForTenant(IServiceCollection collectionClone, TTenant tenant, IConfiguration tenantConfiguration)
        {
            ILogProvider logProvider = additionnalServicesTenant?.Invoke(collectionClone, tenant, tenantConfiguration);
            if (logProvider != null)
            {
                collectionClone.RemoveAll(typeof(ILogProvider));
                collectionClone.RemoveAll(typeof(ILog<>));

                collectionClone.TryAdd(ServiceDescriptor.Singleton<ILogProvider>(logProvider));
                collectionClone.TryAdd(ServiceDescriptor.Singleton(typeof(ILog<>), typeof(Log<>)));
            }
        }

        private IServiceProvider GetProviderFromFactory(IServiceCollection collectionClone)
        {
            ServiceProvider provider = collectionClone.BuildServiceProvider();
            IServiceProviderFactory<IServiceCollection> factory = provider.GetService<IServiceProviderFactory<IServiceCollection>>();

            using (provider)
            {
                return factory.CreateServiceProvider(factory.CreateBuilder(collectionClone));
            }
        }

        private void Replace<TService>(IServiceCollection services, Func<TService> factory, ServiceLifetime lifetime)
            where TService : class
        {
            ICollection<ServiceDescriptor> descriptorListToRemove = services.Where(d => d.ServiceType == typeof(TService)).ToList()
                                                                            ?? Enumerable.Empty<ServiceDescriptor>().ToList();

            if (descriptorListToRemove.Any())
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
        }
    }



    internal class LazyConcurrentDictionary<TKey, TValue>
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
