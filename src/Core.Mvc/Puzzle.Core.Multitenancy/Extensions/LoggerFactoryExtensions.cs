namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Internal.Logging;

    internal static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddTenantLogger<TTenant>(
            this ILoggerFactory factory,
            TTenant tenant,
            IServiceProvider serviceProvider,
            LogLevel minLevel)
        {
            IHttpContextAccessor httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            AddTenantLogger(
                factory,
                tenant,
                httpContextAccessor,
                (_, logLevel) => logLevel >= minLevel);

            return factory;
        }

        public static ILoggerFactory AddTenantLogger<TTenant>(
            this ILoggerFactory factory,
            TTenant tenant,
            IHttpContextAccessor httpContextAccessor,
            Func<string, LogLevel, bool> filter = null)
        {
            factory.AddProvider(new MultitenantLoggerProvider<TTenant>(tenant, factory, filter, httpContextAccessor));
            return factory;
        }
    }
}
