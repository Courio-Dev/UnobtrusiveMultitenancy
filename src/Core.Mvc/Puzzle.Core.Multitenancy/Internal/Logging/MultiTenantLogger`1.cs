namespace Puzzle.Core.Multitenancy.Internal.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Extensions;
    using Serilog;
    using Serilog.Extensions.Logging;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    internal class MultiTenantLogger<TTenant> : ILogger
    {
        private readonly ILogger logger;
        private readonly string tenantId;
        private readonly string categoryName;
        private readonly Func<string, LogLevel, bool> filter;

        public MultiTenantLogger(string tenantId, string categoryName, ILogger logger, Func<string, LogLevel, bool> filter)
        {
            this.tenantId = tenantId;
            this.categoryName = categoryName;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.filter = filter;
        }

        /// <inheritdoc />
        IDisposable ILogger.BeginScope<TState>(TState state) => logger.BeginScope(state);

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return !string.IsNullOrEmpty(tenantId) && (filter == null || filter(categoryName, logLevel)) && logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (IsEnabled(logLevel))
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}
