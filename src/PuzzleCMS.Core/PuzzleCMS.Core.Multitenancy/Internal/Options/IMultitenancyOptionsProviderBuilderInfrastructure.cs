namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;


    /// <summary>
    ///     <para>
    ///         Explicitly implemented by <see cref="MultitenancyOptionsProviderBuilder{TTenant}" />.
    ///     </para>
    ///      <para>
    ///         This interface is typically internally.
    ///     </para>
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant.</typeparam>
    internal interface IMultitenancyOptionsProviderBuilderInfrastructure<TTenant>
    {
        /// <summary>
        /// Adds the given extension to the builder.
        /// </summary>
        /// <param name="func"></param>
        void AddOrUpdateExtension(Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> func);

        /// <summary>
        /// Adds the given extension to the builder.
        /// </summary>
        /// <param name="action"></param>
        void AddOrUpdateServicesTenant(Action<IServiceCollection, TTenant> action);
    }

}
