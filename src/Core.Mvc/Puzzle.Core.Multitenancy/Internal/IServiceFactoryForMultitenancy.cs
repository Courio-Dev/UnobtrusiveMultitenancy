namespace Puzzle.Core.Multitenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal interface IServiceFactoryForMultitenancy<TTenant>
    {
        IServiceProvider Build(TenantContext<TTenant> tenantContext);
    }

    internal class ServiceFactoryForMultitenancy<TTenant> : IServiceFactoryForMultitenancy<TTenant>
    {
        private static LazyConcurrentDictionary<string, IServiceProvider> Cache { get; } = new LazyConcurrentDictionary<string, IServiceProvider>();

        private readonly IOptionsMonitor<MultitenancyOptions> optionsMonitor;

        public IServiceCollection Services { get; }

        public Action<IServiceCollection, TTenant> ConfigurePerTenantServicesDelegate { get; }

        public ServiceFactoryForMultitenancy(
            IServiceCollection services, Action<IServiceCollection, TTenant> configurePerTenantServicesDelegate, IOptionsMonitor<MultitenancyOptions> optionsMonitor)
            : this()
        {  
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            this.optionsMonitor.OnChange(vals =>
            {
                Cache?.Clear();
            });
            Services = services;
            ConfigurePerTenantServicesDelegate = configurePerTenantServicesDelegate;
        }

        private ServiceFactoryForMultitenancy()
        {
        }

        public IServiceProvider Build(TenantContext<TTenant> tenantContext)
        {
            string key = tenantContext.Id;

            IServiceProvider value = Cache.GetOrAdd(key, (k) =>
            {
                IServiceCollection serviceCollection = Services.Clone();
                ConfigurePerTenantServicesDelegate(serviceCollection, tenantContext.Tenant);
                return GetProviderFromFactory(serviceCollection, tenantContext);
            });

            return value;
        }

        private IServiceProvider GetProviderFromFactory(IServiceCollection collectionClone, TenantContext<TTenant> tenantContext)
        {
            ServiceProvider provider = collectionClone.BuildServiceProvider();
            IServiceProviderFactory<IServiceCollection> factory = provider.GetService<IServiceProviderFactory<IServiceCollection>>();

            if (factory != null)
            {
                using (provider)
                {
                    return factory.CreateServiceProvider(factory.CreateBuilder(collectionClone));
                }
            }

            return provider;
        }

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
