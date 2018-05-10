namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

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
