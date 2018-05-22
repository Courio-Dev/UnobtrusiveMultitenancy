namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;

    internal static class MultiTenancyConfigExtensions
    {
        public static MultiTenancyConfig UseLogProvider<TLogProvider>(this MultiTenancyConfig configuration, TLogProvider provider)
            where TLogProvider : ILogProvider
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return configuration.Use(provider, x => LogProvider.SetCurrentLogProvider(x));
        }

        /*
        public static MultiTenancyConfig UseNLogLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new NLogLogProvider());
        }

        public static MultiTenancyConfig UseColoredConsoleLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new ColoredConsoleLogProvider());
        }

        public static MultiTenancyConfig UseLog4NetLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new Log4NetLogProvider());
        }
        */

        /*public static MultiTenancyConfig UseElmahLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new ElmahLogProvider());
        }

        public static MultiTenancyConfig UseElmahLogProvider(this MultiTenancyConfig configuration,LogLevel minLevel)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new ElmahLogProvider(minLevel));
        }*/

        /*
        public static MultiTenancyConfig UseEntLibLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new EntLibLogProvider());
        }

        public static MultiTenancyConfig UseSerilogLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new SerilogLogProvider());
        }

        public static MultiTenancyConfig UseLoupeLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new LoupeLogProvider());
        }
        */

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static MultiTenancyConfig Use<T>(this MultiTenancyConfig configuration, T entry, Action<T> entryAction)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            entryAction(entry);

            return configuration;
        }
    }
}
