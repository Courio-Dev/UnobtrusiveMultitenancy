namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    internal abstract class MemoryCacheTenantResolver<TTenant> : ITenantResolver<TTenant>
    {
        private readonly IMemoryCache cache;
        private readonly ILogger log;
        private readonly MemoryCacheTenantResolverOptions options;

        public MemoryCacheTenantResolver(IMemoryCache cache, ILoggerFactory loggerFactory)
            : this(cache, loggerFactory, new MemoryCacheTenantResolverOptions())
        {
        }

        public MemoryCacheTenantResolver(IMemoryCache cache, ILoggerFactory loggerFactory, MemoryCacheTenantResolverOptions options)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException($"Argument {nameof(loggerFactory)} must not be null");
            }

            this.cache = cache ?? throw new ArgumentNullException($"Argument {nameof(cache)} must not be null");
            this.options = options ?? throw new ArgumentNullException($"Argument {nameof(options)} must not be null");

            log = loggerFactory.CreateLogger<MemoryCacheTenantResolver<TTenant>>();
        }

        async Task<TenantContext<TTenant>> ITenantResolver<TTenant>.ResolveAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
            }

            // Obtain the key used to identify cached tenants from the current request
            string cacheKey = GetContextIdentifier(context);

            // if (cacheKey == null)
            // {
            //    return null;
            // }
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return null;
            }

            TenantContext<TTenant> tenantContext = cache.Get(cacheKey) as TenantContext<TTenant>;

            if (tenantContext == null)
            {
                log.LogDebug("TenantContext not present in cache with key \"{cacheKey}\". Attempting to resolve.", cacheKey);
                tenantContext = await ResolveAsync(context).ConfigureAwait(false);

                if (tenantContext != null)
                {
                    IEnumerable<string> tenantIdentifiers = GetTenantIdentifiers(tenantContext);

                    if (tenantIdentifiers != null)
                    {
                        MemoryCacheEntryOptions cacheEntryOptions = GetCacheEntryOptions();

                        log.LogDebug("TenantContext:{id} resolved. Caching with keys \"{tenantIdentifiers}\".", tenantContext.Id, tenantIdentifiers);

                        foreach (string identifier in tenantIdentifiers)
                        {
                            cache.Set(identifier, tenantContext, cacheEntryOptions);
                        }
                    }
                }
            }
            else
            {
                log.LogDebug("TenantContext:{id} retrieved from cache with key \"{cacheKey}\".", tenantContext.Id, cacheKey);
            }

            return tenantContext;
        }

        protected virtual MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(new TimeSpan(1, 0, 0));
        }

        protected virtual void DisposeTenantContext(object cacheKey, TenantContext<TTenant> tenantContext)
        {
            if (tenantContext != null)
            {
                log.LogDebug("Disposing TenantContext:{id} instance with key \"{cacheKey}\".", tenantContext.Id, cacheKey);
                tenantContext.Dispose();
            }
        }

        protected abstract string GetContextIdentifier(HttpContext context);

        protected abstract IEnumerable<string> GetTenantIdentifiers(TenantContext<TTenant> context);

        protected abstract Task<TenantContext<TTenant>> ResolveAsync(HttpContext context);

        private MemoryCacheEntryOptions GetCacheEntryOptions()
        {
            MemoryCacheEntryOptions cacheEntryOptions = CreateCacheEntryOptions();

            if (options.EvictAllEntriesOnExpiry)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                cacheEntryOptions
                    .RegisterPostEvictionCallback(
                        (key, value, reason, state) =>
                        {
                            tokenSource.Cancel();
                        })
                    .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
            }

            if (options.DisposeOnEviction)
            {
                cacheEntryOptions
                    .RegisterPostEvictionCallback(
                        (key, value, reason, state) =>
                        {
                            DisposeTenantContext(key, value as TenantContext<TTenant>);
                        });
            }

            return cacheEntryOptions;
        }
    }
}
