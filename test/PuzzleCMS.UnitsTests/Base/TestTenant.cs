namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Threading;

    public class TestTenant : IDisposable
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
