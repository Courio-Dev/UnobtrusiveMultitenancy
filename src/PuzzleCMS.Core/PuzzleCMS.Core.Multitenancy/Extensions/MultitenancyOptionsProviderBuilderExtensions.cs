
namespace PuzzleCMS.Core.Multitenancy.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// Extensions methos for add/or override configuration for MultitenancyOptionsProviderBuilder.
    /// </summary>
    public static class MultitenancyOptionsProviderBuilderExtensions
    {
        /// <summary>
        /// Set the internal defaultLogProvider.
        /// </summary>
        /// <returns></returns>
        internal static void SetDefaultLogProvider()
        {
            LogProvider.SetCurrentLogProvider(new ColoredConsoleLogProvider());
        }

        /// <summary>
        /// Activate ConsoleLog.
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static MultitenancyOptionsProviderBuilder<TTenant> UseColoredConsoleLogProvider<TTenant>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder)
        {
            return builder.UseLogProvider(new ColoredConsoleLogProvider());
        }



        /// <summary>
        /// Set current LogProvider.
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <typeparam name="TLogProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static MultitenancyOptionsProviderBuilder<TTenant> UseLogProvider<TTenant, TLogProvider>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder, TLogProvider provider)
            where TLogProvider : ILogProvider
        {
            return builder.Use(provider, x => LogProvider.SetCurrentLogProvider(x));
        }

        /// <summary>
        /// Add serilo ProviderLog.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static MultitenancyOptionsProviderBuilder<TTenant> UseSerilogLogProvider<TTenant>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder)
        {
            return builder.UseLogProvider(new SerilogLogProvider());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="builder"></param>
        /// <param name="withFunc"></param>
        /// <returns></returns>
        public static MultitenancyOptionsProviderBuilder<TTenant> UseCustomServicesTenant<TTenant>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder, Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> withFunc)
        {
            WithAddOrUpdateExtensions(builder, withFunc);
            return builder;
        }

        /// <summary>
        /// Add additionnal ConfigureServices for specific tenant.
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <param name="builder">Object represents MultiTenancyConfig.</param>
        /// <param name="withAction">Action to configure specific services for tenant.</param>
        /// <returns></returns>
        public static MultitenancyOptionsProviderBuilder<TTenant> UseConfigureServicesTenant<TTenant>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder, Action<IServiceCollection, TTenant> withAction)
        {
            WithAddOrUpdateServicesTenants(builder, withAction);
            return builder;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTenant"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="entry"></param>
        /// <param name="entryAction"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static MultitenancyOptionsProviderBuilder<TTenant> Use<TTenant, T>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder, T entry, Action<T> entryAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            entryAction?.Invoke(entry);
            return builder;
        }

        /// <summary>
        /// Adds the given extension to the builder..
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="withFunc"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static MultitenancyOptionsProviderBuilder<TTenant> WithAddOrUpdateExtensions<TTenant>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder,
            Func<IServiceCollection, TTenant, IConfiguration, ILogProvider> withFunc)
        {
            ((IMultitenancyOptionsProviderBuilderInfrastructure<TTenant>)builder)?.AddOrUpdateExtension(withFunc);

            return builder;
        }

        /// <summary>
        /// Adds the given extension to the builder..
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="withAction"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static MultitenancyOptionsProviderBuilder<TTenant> WithAddOrUpdateServicesTenants<TTenant>(
            this MultitenancyOptionsProviderBuilder<TTenant> builder,
            Action<IServiceCollection, TTenant> withAction)
        {
            ((IMultitenancyOptionsProviderBuilderInfrastructure<TTenant>)builder)?.AddOrUpdateServicesTenant(withAction);

            return builder;
        }
    }
}
