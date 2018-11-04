using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring <see cref="MultitenancyOptions" />.
    ///     </para>
    /// </summary>
    public class MultitenancyOptionsBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MultitenancyOptionsBuilder" /> class to further configure
        ///     a given <see cref="MultitenancyOptions" />.
        /// </summary>
        /// <param name="options"> The options to be configured. </param>
        /// <param name="services">The service collection.</param>
        public MultitenancyOptionsBuilder(MultitenancyOptions options,IServiceCollection services)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Multitenancy Options.
        /// </summary>
        public MultitenancyOptions Options { get; }

        /// <summary>
        /// Service collection.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
