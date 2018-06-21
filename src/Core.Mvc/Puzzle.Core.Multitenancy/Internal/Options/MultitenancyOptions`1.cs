namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Options for multitenancy.
    /// </summary>
    /// <typeparam name="TTenant">Tenant object.</typeparam>
    public class MultitenancyOptions<TTenant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyOptions{TTenant}"/> class.
        /// </summary>
        public MultitenancyOptions()
        {
            OtherTokens = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets tenant's folder.
        /// </summary>
        public string AppTenantFolder { get; set; }

        /// <summary>
        /// Gets or sets tokens replacement.
        /// </summary>
        public IDictionary<string, string> OtherTokens { get; set; }

        /// <summary>
        /// Gets or sets list of tenant.
        /// </summary>
        public virtual Collection<TTenant> Tenants { get; set; }
    }
}
