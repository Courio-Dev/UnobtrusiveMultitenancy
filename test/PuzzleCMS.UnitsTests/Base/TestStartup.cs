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

    internal class TestStartup
    {
        public TestStartup()
        {
        }

        public void ConfigureTestServices(IServiceCollection services)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder application)
        {
            application.Run(async ctx =>
            {
                await ctx.Response.WriteAsync(": Test").ConfigureAwait(false);
            });
        }
    }
}
