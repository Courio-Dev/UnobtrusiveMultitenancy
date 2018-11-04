namespace PuzzleCMS.Core.Multitenancy.Internal.Middlewares
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using PuzzleCMS.Core.Multitenancy.Extensions;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Options;

    internal class TenantPipelineMiddleware<TTenant>
    {
        private readonly ILog<TenantPipelineMiddleware<TTenant>> logger;
        private readonly RequestDelegate next;
        private readonly IApplicationBuilder rootApp;

        private readonly IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy;

        private readonly Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration;

        private readonly ConcurrentDictionary<TTenant, Lazy<RequestDelegate>> pipelines
            = new ConcurrentDictionary<TTenant, Lazy<RequestDelegate>>();

        private readonly ConcurrentDictionary<TTenant, Lazy<Func<HttpContext, bool>>> pipelinesBranchBuilder
            = new ConcurrentDictionary<TTenant, Lazy<Func<HttpContext, bool>>>();

        public TenantPipelineMiddleware(
            RequestDelegate next,
            IApplicationBuilder rootApp,
            Action<TenantPipelineBuilderContext<TTenant>,
            IApplicationBuilder> configuration, IOptionsMonitor<MultitenancyOptions<TTenant>> optionsMonitor,
            ILog<TenantPipelineMiddleware<TTenant>> logger,
            IServiceFactoryForMultitenancy<TTenant> serviceFactoryForMultitenancy)
        {
            this.next = next ?? throw new ArgumentNullException($"Argument {nameof(next)} must not be null");
            this.rootApp = rootApp ?? throw new ArgumentNullException($"Argument {nameof(rootApp)} must not be null");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceFactoryForMultitenancy = serviceFactoryForMultitenancy ?? throw new ArgumentNullException(nameof(serviceFactoryForMultitenancy));

            this.configuration = configuration ?? throw new ArgumentNullException($"Argument {nameof(configuration)} must not be null");
            IOptionsMonitor<MultitenancyOptions<TTenant>> opt = optionsMonitor ?? throw new ArgumentNullException($"Argument {nameof(optionsMonitor)} must not be null");
            opt.OnChange(vals =>
            {
                pipelinesBranchBuilder.Clear();

                // log change.
                this.logger.Debug($"Config changed: {string.Join(", ", vals)}");
            });
        }

        public async Task Invoke(HttpContext httpContext)
        {
            ILog<TenantPipelineMiddleware<TTenant>> log = httpContext.RequestServices.GetService<ILog<TenantPipelineMiddleware<TTenant>>>() ?? logger;
            TenantContext<TTenant> tenantContext = httpContext.GetTenantContext<TTenant>();
            if (tenantContext != null)
            {
                Lazy<RequestDelegate> tenantPipeline = pipelines.GetOrAdd(
                    tenantContext.Tenant,
                    new Lazy<RequestDelegate>(() => BuildTenantPipeline(httpContext, tenantContext, log)));
                await tenantPipeline.Value(httpContext).ConfigureAwait(false);
            }
        }

        private RequestDelegate BuildTenantPipeline(HttpContext httpContext, TenantContext<TTenant> tenantContext, ILog<TenantPipelineMiddleware<TTenant>> loggerPipeline)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            loggerPipeline.Info($" Building TenantPipeline for Tenant: {tenantContext.Id}");
            IApplicationBuilder branchBuilder = rootApp.New();
            TenantPipelineBuilderContext<TTenant> builderContext = new TenantPipelineBuilderContext<TTenant>(tenantContext);

            IServiceProvider provider = serviceFactoryForMultitenancy.Build(tenantContext);
            branchBuilder.Use(async (context, next) =>
            {
                IServiceScopeFactory factory = provider.GetRequiredService<IServiceScopeFactory>();
                using (IServiceScope scope = factory.CreateScope())
                {
                    context.RequestServices = scope.ServiceProvider;
                    await next().ConfigureAwait(false);
                }
            });

            branchBuilder.ApplicationServices = provider;
            configuration(builderContext, branchBuilder);

            // register root pipeline at the end of the tenant branch
            branchBuilder.Run(next);

            RequestDelegate branchDelegate = branchBuilder.Build();
            loggerPipeline.Info($" Builded TenantPipeline for Tenant: {tenantContext.Id}");
            return branchDelegate;
        }
    }
}
