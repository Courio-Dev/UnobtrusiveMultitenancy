namespace PuzzleCMS.Core.Multitenancy.Internal.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Primitives;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;

    internal abstract class MemoryCacheTenantResolver<TTenant> : ITenantResolver<TTenant>
    {
        private readonly IMemoryCache cache;
        private CancellationTokenSource resetCacheToken = new CancellationTokenSource();
        private readonly Logging.LibLog.ILog log;
        private readonly MemoryCacheTenantResolverOptions options;
        private readonly IMultitenancyOptionsProvider<TTenant> multitenancyOptionsProvider;

        protected MemoryCacheTenantResolver(IMultitenancyOptionsProvider<TTenant> multitenancyOptionsProvider,IMemoryCache cache, Logging.LibLog.ILog log)
            : this(multitenancyOptionsProvider,cache, log, new MemoryCacheTenantResolverOptions())
        {
        }

        protected MemoryCacheTenantResolver(
            IMultitenancyOptionsProvider<TTenant> multitenancyOptionsProvider,
            IMemoryCache cache, 
            Logging.LibLog.ILog log, 
            MemoryCacheTenantResolverOptions options)
        {
            this.log = log ?? throw new ArgumentNullException($"Argument {nameof(log)} must not be null");
            this.cache = cache ?? throw new ArgumentNullException($"Argument {nameof(cache)} must not be null");
            this.options = options ?? throw new ArgumentNullException($"Argument {nameof(options)} must not be null");
            this.multitenancyOptionsProvider = multitenancyOptionsProvider ?? throw new ArgumentNullException($"Argument {nameof(multitenancyOptionsProvider)} must not be null");
        }

        /// <inheritdoc />
        public virtual void Reset()
        {
            CancellationTokenSource previousToken = Interlocked.Exchange(ref resetCacheToken, new CancellationTokenSource());
            if (previousToken != null && !previousToken.IsCancellationRequested && previousToken.Token.CanBeCanceled)
            {
                previousToken?.Cancel();
                previousToken?.Dispose();
            }
            
            multitenancyOptionsProvider?.Reload();
        }

        Task<TenantContext<TTenant>> ITenantResolver<TTenant>.ResolveAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
            }

            return ResolveInternalAsync(context);
        }

        private async Task<TenantContext<TTenant>> ResolveInternalAsync(HttpContext context)
        {
            // Obtain the key used to identify cached tenants from the current request
            string cacheKey = GetContextIdentifier(context);
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return null;
            }

            TenantContext<TTenant> tenantContext = cache.Get(cacheKey) as TenantContext<TTenant>;

            if (tenantContext == null)
            {
                log.Debug($"TenantContext not present in cache with key \"{cacheKey}\". Attempting to resolve.");
                tenantContext = await ResolveAsync(context).ConfigureAwait(false);

                if (tenantContext != null)
                {
                    IEnumerable<string> tenantIdentifiers = GetTenantIdentifiers(tenantContext);

                    if (tenantIdentifiers != null)
                    {
                        MemoryCacheEntryOptions cacheEntryOptions = GetCacheEntryOptions();

                        log.Debug($"TenantContext:{tenantContext.Id} resolved. Caching with keys \"{tenantIdentifiers}\".");

                        foreach (string identifier in tenantIdentifiers)
                        {
                            _ = Set(cache, identifier, tenantContext, cacheEntryOptions, resetCacheToken);
                        }
                    }
                }
            }
            else
            {
                log.Debug($"TenantContext:{tenantContext.Id} retrieved from cache with key \"{cacheKey}\".");
            }

            return tenantContext;
        }

        protected virtual MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions().SetSlidingExpiration(new TimeSpan(1, 0, 0));
        }

        protected virtual void DisposeTenantContext(object cacheKey, TenantContext<TTenant> tenantContext)
        {
            if (tenantContext != null)
            {
                log.Debug($"Disposing TenantContext:{tenantContext.Id} instance with key \"{cacheKey}\".");
                tenantContext.Dispose();
            }
        }

        protected abstract string GetContextIdentifier(HttpContext context);

        protected virtual IEnumerable<TTenant> Tenants => multitenancyOptionsProvider?.MultitenancyOptions?.Tenants;

        protected abstract IEnumerable<string> GetTenantIdentifiers(TenantContext<TTenant> context);

        protected abstract Func<HttpContext,TTenant, bool> PredicateResolver();

        /// <inheritdoc />
        protected int GetTenantPositionWithPredicateResolver(HttpContext context)
        {
            int? index = Tenants.Select((x, p) => new { Item = x, Position = p })
                                    .FirstOrDefault(x => PredicateResolver().Invoke(context, x.Item))?.Position
                                    ;

            return index.GetValueOrDefault(-1);
        }

        protected abstract Task<TenantContext<TTenant>> ResolveAsync(HttpContext context);

        private MemoryCacheEntryOptions GetCacheEntryOptions()
        {
            MemoryCacheEntryOptions cacheEntryOptions = CreateCacheEntryOptions();

            if (options.EvictAllEntriesOnExpiry)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                cacheEntryOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    tokenSource.Cancel();
                })
                .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
            }

            if (options.DisposeOnEviction)
            {
                cacheEntryOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                     DisposeTenantContext(key, value as TenantContext<TTenant>);
                });
            }

            return cacheEntryOptions;
        }

        private TItem Set<TItem>(IMemoryCache cache, object key, TItem value, MemoryCacheEntryOptions options, CancellationTokenSource token)
        {
            MemoryCacheEntryOptions opt = options ?? new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal);
            opt.AddExpirationToken(new CancellationChangeToken(token.Token));

           return cache.Set(key, value, opt);
        }


    }
}
