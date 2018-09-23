namespace PuzzleCMS.Core.Multitenancy.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PuzzleCMS.Core.Multitenancy.Internal.Configurations;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;

    /// <summary>
    /// Extensions methos for add/or override configuration for MultiTenancyConfig.
    /// </summary>
    public static class MultiTenancyConfigExtensions
    {
        

        /// <summary>
        /// Set current LogProvider.
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <typeparam name="TLogProvider"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static MultiTenancyConfig<TTenant> UseLogProvider<TTenant,TLogProvider>(this MultiTenancyConfig<TTenant> configuration, TLogProvider provider)
            where TLogProvider : ILogProvider
        {
            return configuration.Use(provider, x => LogProvider.SetCurrentLogProvider(x));
        }

        /// <summary>
        /// Activate ConsoleLog.
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static MultiTenancyConfig<TTenant> UseColoredConsoleLogProvider<TTenant>(this MultiTenancyConfig<TTenant> configuration)
        {
            return configuration.UseLogProvider(new ColoredConsoleLogProvider());
        }


        /// <summary>
        /// Add serilo ProviderLog.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static MultiTenancyConfig<TTenant> UseSerilogLogProvider<TTenant>(this MultiTenancyConfig<TTenant> configuration)
        {
            return configuration.UseLogProvider(new SerilogLogProvider());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <typeparam name="TLogProvider"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="tenant"></param>
        /// <param name="tenantConfiguration"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static MultiTenancyConfig<TTenant> UseLogCustomServicesTenant<TTenant, TLogProvider>(
            this MultiTenancyConfig<TTenant> configuration, TTenant tenant,IConfiguration tenantConfiguration, TLogProvider provider)
            where TLogProvider : ILogProvider
        {
            return configuration.Use(provider, x => LogProvider.SetCurrentLogProvider(x));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static MultiTenancyConfig<TTenant> UseCustomServicesTenant<TTenant>(
            this MultiTenancyConfig<TTenant> configuration, Func<IServiceCollection, TTenant,IConfiguration, ILogProvider> func)
        {
            configuration.SetTenantLogProvider(func);
            return configuration;
        }

        /// <summary>
        /// Add additionnal ConfigureServices for specific tenant.
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="configuration">Object represents MultiTenancyConfig.</param>
        /// <param name="action">Action to configure services.</param>
        /// <returns></returns>
        public static MultiTenancyConfig<TTenant> UseConfigureServicesTenant<TTenant>(
            this MultiTenancyConfig<TTenant> configuration, Action<IServiceCollection, TTenant> action)
        {
            return configuration.Use(action, x => configuration.SetConfigureServicesTenant(action));
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="entry"></param>
        /// <param name="entryAction"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static MultiTenancyConfig<TTenant> Use<TTenant,T>(this MultiTenancyConfig<TTenant> configuration, T entry, Action<T> entryAction)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            entryAction?.Invoke(entry);
            return configuration;
        }
    }
}
