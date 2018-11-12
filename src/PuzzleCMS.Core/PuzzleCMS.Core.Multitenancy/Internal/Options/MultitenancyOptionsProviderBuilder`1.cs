using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;

namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring <see cref="MultitenancyOptionsProvider{TTenant}" />.
    ///     </para>
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant.</typeparam>
    public class MultitenancyOptionsProviderBuilder<TTenant>: IMultitenancyOptionsProviderBuilderInfrastructure<TTenant>
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultitenancyOptionsProvider{TTenant}" /> class to further configure.
        /// </summary>
        /// <param name="services">The services collections</param>
        /// <param name="multitenancyOptionsProvider"></param>
        public MultitenancyOptionsProviderBuilder(IServiceCollection services, MultitenancyOptionsProvider<TTenant> multitenancyOptionsProvider)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            MultitenancyOptionsProvider = multitenancyOptionsProvider ?? throw new ArgumentNullException(nameof(multitenancyOptionsProvider));
        }

        /// <summary>
        /// 
        /// </summary>
        protected MultitenancyOptionsProvider<TTenant> MultitenancyOptionsProvider { get; }


        /////// <summary>
        /////// Get or set MultitenancyOptionsProvider;
        /////// </summary>
        ////public MultitenancyOptionsProvider<TTenant> Provider { get; }

        /// <summary>
        /// 
        /// </summary>
        public IServiceCollection Services { get; }

        /// <inheritdoc/>
        public void AddOrUpdateExtension(Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> func)
        {
            MultitenancyOptionsProvider?.SetTenantLogProvider(func);
        }

        /// <inheritdoc/>
        public void AddOrUpdateServicesTenant(Action<IServiceCollection, TTenant> action)
        {
            MultitenancyOptionsProvider?.AddServicesTenant(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMultitenancyOptionsProvider<TTenant> BuildOptionsProviderBuilder()
        {
            return MultitenancyOptionsProvider;
        }


    }
}
