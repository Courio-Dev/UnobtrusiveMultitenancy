namespace Puzzle.Core.Multitenancy.Internal
{
    /// <summary>
    /// ITenant wrapper that returns the tenant instance.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    public class TenantWrapper<TTenant> : ITenant<TTenant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantWrapper{TTenant}"/> class.
        /// Intializes the wrapper with the tenant instance to return.
        /// </summary>
        /// <param name="tenant">The tenant instance to return.</param>
        public TenantWrapper(TTenant tenant)
        {
            Value = tenant;
        }

        /// <summary>
        /// Gets the tenant instance.
        /// </summary>
        /// <value>
        /// The tenant instance.
        /// </value>
        public TTenant Value { get; }
    }
}
