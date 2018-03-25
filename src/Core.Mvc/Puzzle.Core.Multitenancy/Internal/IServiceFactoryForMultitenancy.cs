using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Puzzle.Core.Multitenancy.Internal.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Puzzle.Core.Multitenancy.Internal
{
    public interface IServiceFactoryForMultitenancy<TTenant>
    {
        IServiceProvider Build(TenantContext<TTenant> tenantContext);
    }

    internal class ServiceFactoryForMultitenancy<TTenant> : IServiceFactoryForMultitenancy<TTenant>
    {
        public IServiceCollection Services { get; }
        private readonly IOptionsMonitor<MultitenancyOptions> optionsMonitor;

        public Action<IServiceCollection, TTenant> ConfigurePerTenantServicesDelegate { get; }

        private static readonly LazyConcurrentDictionary<string, IServiceProvider> cache = new LazyConcurrentDictionary<string, IServiceProvider>();

        public ServiceFactoryForMultitenancy
            (IServiceCollection services, Action<IServiceCollection, TTenant> configurePerTenantServicesDelegate, IOptionsMonitor<MultitenancyOptions> optionsMonitor)
            : this()
        {
            this.Services = services;
            this.ConfigurePerTenantServicesDelegate = configurePerTenantServicesDelegate;
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            this.optionsMonitor.OnChange(vals =>
            {
                cache?.Clear();
            });
        }

        private ServiceFactoryForMultitenancy()
        {
        }

        public IServiceProvider Build(TenantContext<TTenant> tenantContext)
        {
            var key = tenantContext.Id;

            var value = cache.GetOrAdd(key, (k) =>
            {
                var serviceCollection = Services.Clone();
                ConfigurePerTenantServicesDelegate(serviceCollection, tenantContext.Tenant);
                return GetProviderFromFactory(serviceCollection, tenantContext);
            });

            return value;
        }

        private IServiceProvider GetProviderFromFactory(IServiceCollection collectionClone, TenantContext<TTenant> tenantContext)
        {
            var provider = collectionClone.BuildServiceProvider();
            var factory = provider.GetService<IServiceProviderFactory<IServiceCollection>>();

            if (factory != null)
            {
                using (provider)
                {
                    return factory.CreateServiceProvider(factory.CreateBuilder(collectionClone));
                }
            }

            return provider;
        }
    }

    internal class LazyConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> concurrentDictionary;

        public LazyConcurrentDictionary()
        {
            this.concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var lazyResult = this.concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));

            return lazyResult.Value;
        }

        public void Clear()
        {
            concurrentDictionary?.Clear();
        }
    }

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection Clone(this IServiceCollection serviceCollection)
        {
            IServiceCollection clone = new ServiceCollection();
            foreach (var service in serviceCollection)
            {
                clone.Add(service);
            }
            return clone;
        }
    }
}