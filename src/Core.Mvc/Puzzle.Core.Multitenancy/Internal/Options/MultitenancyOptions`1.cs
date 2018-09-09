namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Extensions.Configuration;

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
            Tokens = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets tenant's folder.
        /// </summary>
        public string TenantFolder { get; set; }

        /// <summary>
        /// Gets or sets tokens replacement.
        /// </summary>
        public IDictionary<string, string> Tokens { get; set; }

        /// <summary>
        /// Gets or sets list of tenant.
        /// </summary>
        public virtual Collection<TTenant> Tenants { get; set; }

        /// <summary>
        /// Configuration of each tenant.
        /// </summary>
        public virtual IEnumerable<IConfigurationSection> TenantsConfigurations { get; set; } 
    }
}
