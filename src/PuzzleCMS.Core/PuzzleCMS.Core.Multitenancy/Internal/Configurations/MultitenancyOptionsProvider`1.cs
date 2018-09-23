namespace PuzzleCMS.Core.Multitenancy.Internal.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    internal class MultitenancyOptionsProvider<TTenant> : IMultitenancyOptionsProvider<TTenant>
    {
        private readonly MultiTenancyConfig<TTenant> multiTenancyConfig;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiTenancyConfig"></param>
        public MultitenancyOptionsProvider(MultiTenancyConfig<TTenant> multiTenancyConfig)
        {
            this.multiTenancyConfig = multiTenancyConfig;

            ChangeTokenConfiguration?.RegisterChangeCallback(OnReloadConfig, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public MultitenancyOptions<TTenant> MultitenancyOptions => multiTenancyConfig.CurrentMultiTenacyOptionsValue;

        /// <summary>
        /// 
        /// </summary>
        public IChangeToken ChangeTokenConfiguration => multiTenancyConfig.ChangeTokenConfiguration;

        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {
            OnReloadConfig(null);
        }

        private void OnReloadConfig(object state)
        {
            Console.WriteLine($" {ChangeTokenConfiguration} Configuration changed. ");
            (MultitenancyOptions<TTenant> options, IChangeToken token) = multiTenancyConfig.ReloadConfiguration();

            // The token will change each time it reloads, so we need to register again.
            token?.RegisterChangeCallback(OnReloadConfig, null);
        }
    }
}
