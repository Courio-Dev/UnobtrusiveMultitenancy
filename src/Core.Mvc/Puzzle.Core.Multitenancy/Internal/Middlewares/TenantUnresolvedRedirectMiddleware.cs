namespace Puzzle.Core.Multitenancy.Internal.Middlewares
{
    /*
    internal class TenantUnresolvedRedirectMiddleware<TTenant>
    {
        private readonly string redirectLocation;
        private readonly bool permanentRedirect;
        private readonly RequestDelegate next;

        public TenantUnresolvedRedirectMiddleware(
            RequestDelegate next,
            string redirectLocation,
            bool permanentRedirect)
        {
            this.next = next ?? throw new ArgumentNullException($"Argument {nameof(next)} must not be null");
            this.redirectLocation = redirectLocation ?? throw new ArgumentNullException($"Argument {nameof(redirectLocation)} must not be null");
            this.permanentRedirect = permanentRedirect;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
            var tenantContext = context.GetTenantContext<TTenant>();

            if (tenantContext == null)
            {
                Redirect(context, redirectLocation);
                return;
            }

            // otherwise continue processing
            await next(context);
        }

        private void Redirect(HttpContext context, string redirectLocation)
        {
            context.Response.Redirect(redirectLocation);
            context.Response.StatusCode = permanentRedirect ? StatusCodes.Status301MovedPermanently : StatusCodes.Status302Found;
        }
    }
    */
    /*
    internal class TenantUnresolved404Middleware
    {
        private readonly RequestDelegate _next;

        public TenantUnresolved404Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var tenantContext = context.GetTenantContext<apptenant>();

            if (tenantContext == null)
            {
                context.Response.StatusCode = 404;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }

    public static class TenantUnresolvedRedirectExtensions
    {
        public static IApplicationBuilder UseTenantUnresolvedMiddleware<TTenant>(this IApplicationBuilder app)
        {
            Ensure.Argument.NotNull(app, nameof(app));
            return app.UseMiddleware<TenantUnresolvedRedirectMiddleware<TTenant>>("http://localhost:51495/Home/Index", false);
        }
    }

    */
}