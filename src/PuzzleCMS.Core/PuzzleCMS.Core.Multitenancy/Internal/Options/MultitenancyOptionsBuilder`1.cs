using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring <see cref="MultitenancyOptions{TTenant}" />.
    ///     </para>
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant.</typeparam>
    public class MultitenancyOptionsBuilder<TTenant>: MultitenancyOptionsBuilder
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultitenancyOptions{TTenant}" /> class with no options set.
        /// </summary>
        public MultitenancyOptionsBuilder(IServiceCollection services)
            : this(new MultitenancyOptions<TTenant>(),services)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultitenancyOptions{TTenant}" /> class to further configure.
        /// </summary>
        /// <param name="options"> The options to be configured. </param>
        /// <param name="services">The services collections</param>
        public MultitenancyOptionsBuilder(MultitenancyOptions<TTenant> options,IServiceCollection services)
            : base(options,services)
        {
        }
    }
}
