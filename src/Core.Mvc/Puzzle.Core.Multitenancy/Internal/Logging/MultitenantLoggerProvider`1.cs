namespace Puzzle.Core.Multitenancy.Internal.Logging
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Extensions;
    using Serilog;
    using Serilog.Extensions.Logging;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    internal class MultitenantLoggerProvider<TTenant> : ILoggerProvider
    {
        private static readonly ConcurrentDictionary<string, MultiTenantLogger<TTenant>> Loggers = new ConcurrentDictionary<string, MultiTenantLogger<TTenant>>();

        private readonly ILoggerFactory factory;
        private readonly Func<string, LogLevel, bool> filter;
        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly SerilogLoggerProvider serilogLoggerProvider;

        private readonly string tenantId;

        public MultitenantLoggerProvider(
            TTenant tenant,
            ILoggerFactory factory,
            Func<string, LogLevel, bool> filter,
            IHttpContextAccessor httpContextAccessor)
        {
            this.factory = factory;
            this.filter = filter;
            this.httpContextAccessor = httpContextAccessor;

            tenantId = httpContextAccessor.HttpContext.GetTenantContext<TTenant>()?.Id;
            serilogLoggerProvider = ConfigureFileLoggingProvider($"App_Tenants/Logs/{tenantId}/log.txt");
        }

        public ILogger CreateLogger(string categoryName)
        {
            return Loggers.GetOrAdd(categoryName, name =>
            {
                ILogger logger = tenantId == null ? new NoopLogger() : serilogLoggerProvider.CreateLogger(categoryName);
                return new MultiTenantLogger<TTenant>(tenantId, name, logger, filter);
            });
        }

        public void Dispose()
        {
            serilogLoggerProvider?.Dispose();
        }

        private static SerilogLoggerProvider ConfigureFileLoggingProvider(string fileName)
        {
            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            Serilog.Core.Logger serilogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File(fileName, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}", flushToDiskInterval: TimeSpan.FromSeconds(1), shared: true)
                .CreateLogger();
            return new SerilogLoggerProvider(serilogger, dispose: true);
        }
    }
}
