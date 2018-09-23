namespace PuzzleCMS.Core.Multitenancy.Internal.Resolvers
{
    /// <summary>
    /// Configuration options for <see cref="MemoryCacheTenantResolver{TTenant}"/>.
    /// </summary>
    public class MemoryCacheTenantResolverOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheTenantResolverOptions"/> class.
        /// Creates a new <see cref="MemoryCacheTenantResolverOptions"/> instance.
        /// </summary>
        public MemoryCacheTenantResolverOptions()
        {
            EvictAllEntriesOnExpiry = true;
            DisposeOnEviction = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a setting that determines whether all cache entries for a <see cref="TenantContext{TTenant}"/>
        /// instance should be evicted when any of the entries expire. Default: True.
        /// </summary>
        /// <value>
        /// A value indicating whether gets or sets a setting that determines whether all cache entries for a <see cref="TenantContext{TTenant}"/>
        /// instance should be evicted when any of the entries expire. Default: True.
        /// </value>
        public bool EvictAllEntriesOnExpiry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a setting that determines whether cached tenant context instances should be disposed
        /// when upon eviction from the cache. Default: True.
        /// </summary>
        /// <value>
        /// A value indicating whether gets or sets a setting that determines whether cached tenant context instances should be disposed
        /// when upon eviction from the cache. Default: True.
        /// </value>
        public bool DisposeOnEviction { get; set; }
    }
}
