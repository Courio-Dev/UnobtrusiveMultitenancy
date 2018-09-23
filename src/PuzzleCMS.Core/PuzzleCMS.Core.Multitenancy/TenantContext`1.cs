namespace PuzzleCMS.Core.Multitenancy
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Context of the tenant.
    /// </summary>
    /// <typeparam name="TTenant">Tenant object.</typeparam>
    public class TenantContext<TTenant> : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantContext{TTenant}"/> class.
        /// </summary>
        /// <param name="tenant">Tenant object.</param>
        /// <param name="position">position</param>
        public TenantContext(TTenant tenant,int position)
        {
            if (tenant == null)
            {
                 throw new ArgumentNullException($"Argument {nameof(tenant)} must not be null");
            }
            if (position < 0)
            {
                throw new ArgumentException($"Argument {nameof(position)} cannot be negative.");
            }

            Position = position;
            Tenant = tenant;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets uniqueId that identify the tenant.
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets uniqueId that identify the tenant.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Gets tenant object.
        /// </summary>
        public TTenant Tenant { get; private set; }

        /// <summary>
        /// Gets additional store data for a tenant.
        /// </summary>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (KeyValuePair<string, object> prop in Properties)
                {
                    TryDisposeProperty(prop.Value as IDisposable);
                }

                TryDisposeProperty(Tenant as IDisposable);
            }

            disposed = true;
        }

        private void TryDisposeProperty(IDisposable obj)
        {
            if (obj == null)
            {
                return;
            }

            try
            {
                obj.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
