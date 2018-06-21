namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using System.ComponentModel;
    using Puzzle.Core.Multitenancy.Internal.Configurations;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;

    internal static class MultiTenancyConfigExtensions
    {
        public static MultiTenancyConfig<TTenant> UseLogProvider<TTenant,TLogProvider>(this MultiTenancyConfig<TTenant> configuration, TLogProvider provider)
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

       
       

        public static MultiTenancyConfig<TTenant> UseColoredConsoleLogProvider<TTenant>(this MultiTenancyConfig<TTenant> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new ColoredConsoleLogProvider());
        }

        /*
       public static MultiTenancyConfig UseLog4NetLogProvider(this MultiTenancyConfig configuration)
       {
           if (configuration == null)
           {
               throw new ArgumentNullException(nameof(configuration));
           }

           return configuration.UseLogProvider(new Log4NetLogProvider());
       }
       */


        /*

        public static MultiTenancyConfig UseSerilogLogProvider(this MultiTenancyConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.UseLogProvider(new SerilogLogProvider());
        }
        */

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static MultiTenancyConfig<TTenant> Use<TTenant,T>(this MultiTenancyConfig<TTenant> configuration, T entry, Action<T> entryAction)
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
