using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MultitenancyOptions: IMultitenancyOptions
    {
        
        private readonly IReadOnlyDictionary<Type, IMultitenancyOptionsExtension> extensionsList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultitenancyOptions" /> class.
        /// </summary>
        /// <param name="extensionsList"> The extensions that store the configured options. </param>
        protected MultitenancyOptions(IReadOnlyDictionary<Type, IMultitenancyOptionsExtension> extensionsList)
        {
            this.extensionsList = extensionsList ?? throw new ArgumentNullException(nameof(extensionsList));
            Tokens = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Gets the extensions that store the configured options.
        /// </summary>
        public virtual IEnumerable<IMultitenancyOptionsExtension> Extensions => extensionsList.Values;

        /// <summary>
        ///     Gets the extension of the specified type. Returns null if no extension of the specified type is configured.
        /// </summary>
        /// <typeparam name="TExtension"> The type of the extension to get. </typeparam>
        /// <returns> The extension, or null if none was found. </returns>
        public virtual TExtension FindExtension<TExtension>()
            where TExtension : class, IMultitenancyOptionsExtension
            => extensionsList.TryGetValue(typeof(TExtension), out IMultitenancyOptionsExtension extension) ? (TExtension)extension : null;

        /// <summary>
        ///     The type of tenant that these options are for.
        /// </summary>
        public abstract Type TenantType { get; }

        /// <summary>
        /// Gets or sets tenant's folder.
        /// </summary>
        public string TenantFolder { get; set; }

        /// <summary>
        /// Gets or sets tokens replacement.
        /// </summary>
        public IDictionary<string, string> Tokens { get; set; }


        /// <summary>
        /// Configuration of each tenant.
        /// </summary>
        public virtual IEnumerable<IConfigurationSection> TenantsConfigurations { get; set; }
    }
}
