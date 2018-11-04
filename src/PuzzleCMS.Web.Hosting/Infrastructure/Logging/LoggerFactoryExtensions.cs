namespace PuzzleCMS.WebHost.Infrastructure.Logging
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddTenantLogger<TTenant>(
            this ILoggerFactory factory,
            TTenant tenant,
            IServiceProvider serviceProvider,
            Microsoft.Extensions.Logging.LogLevel minLevel)
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
            Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter = null)
        {
            return factory;
        }
    }
}
