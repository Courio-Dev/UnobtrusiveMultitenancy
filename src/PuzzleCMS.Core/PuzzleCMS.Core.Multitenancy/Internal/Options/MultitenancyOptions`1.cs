namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Options for multitenancy.
    /// </summary>
    /// <typeparam name="TTenant">Tenant object.</typeparam>
    public class MultitenancyOptions<TTenant> : MultitenancyOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyOptions{TTenant}"/> class.
        /// </summary>
        public MultitenancyOptions()
            : base(new Dictionary<Type, IMultitenancyOptionsExtension>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultitenancyOptions{TTenant}" /> class.
        /// </summary>
        /// <param name="extensions"> The extensions that store the configured options. </param>
        public MultitenancyOptions(IReadOnlyDictionary<Type, IMultitenancyOptionsExtension> extensions)
            : base(extensions)
        {
        }


        /// <summary>
        /// Gets or sets list of tenant.
        /// </summary>
        public virtual Collection<TTenant> Tenants { get; set; }

        /// <summary>
        ///     The type of context that these options are for (<typeparamref name="TTenant" />).
        /// </summary>
        public override Type TenantType => typeof(TTenant);
    }
}
