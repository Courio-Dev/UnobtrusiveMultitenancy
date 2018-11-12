using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
using PuzzleCMS.Core.Multitenancy.Internal.Options;

namespace PuzzleCMS.Core.Multitenancy.Internal.Configurations
{
    /// <summary>
    /// MultitenancyOptions provider.<see cref="MultitenancyOptionsProvider{TTenant}" />.<see cref="MultitenancyOptions{TTenant}" />.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    public interface IMultitenancyOptionsProvider<TTenant>
    {
        /// <summary>
        /// Propagates notifications that a change has occurred..
        /// </summary>
        IChangeToken ChangeTokenConfiguration { get; }

        /// <summary>
        /// Options multitenants.
        /// </summary>
        MultitenancyOptions<TTenant> MultitenancyOptions { get; }

        /// <summary>
        /// List additionnal services to specific tenants.
        /// </summary>
        IList<Action<IServiceCollection, TTenant>> ConfigureServicesTenantList { get; }

        /// <summary>
        /// Specific tenant log provider.
        /// </summary>
        Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> TenantLogProvider { get; }

        /// <summary>
        /// Tell if has found tenants collection in config.
        /// </summary>
        bool HasTenants { get; }

        /// <summary>
        /// Reload config file and set options to null, to be reloaded.
        /// </summary>
        void Reload();
    }
}
