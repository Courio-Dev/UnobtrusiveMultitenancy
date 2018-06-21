namespace PuzzleCMS.UnitsTests.Base
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal class AddTenantNameMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string name;

        public AddTenantNameMiddleware(RequestDelegate next, string name)
        {
            this.next = next;
            this.name = name;
        }

        public async Task Invoke(HttpContext context)
        {
            await context.Response.WriteAsync($"{name}::").ConfigureAwait(false);
            await next(context).ConfigureAwait(false);
        }
    }
}
