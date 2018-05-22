namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Options for multitenancy.
    /// </summary>
    public class MultitenancyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyOptions"/> class.
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
        public Collection<AppTenant> Tenants { get; set; }
    }
}
