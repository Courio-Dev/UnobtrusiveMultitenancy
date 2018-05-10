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

    internal class TestTenant : IDisposable
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        public string Name { get; set; }

        public string[] Hostnames { get; set; }

        public string Theme { get; set; }

        public string ConnectionString { get; set; }

        public bool Disposed { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                cts.Cancel();
            }

            Disposed = true;
        }
    }
}
