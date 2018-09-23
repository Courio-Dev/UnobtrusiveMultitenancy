namespace PuzzleCMS.Core.Multitenancy
{
    using System;

    /// <summary>
    /// Class that store contect of TenantPipeline.
    /// </summary>
    /// <typeparam name="TTenant">Tenant object.</typeparam>
    public class TenantPipelineBuilderContext<TTenant>
    {
        private readonly TenantContext<TTenant> tenantContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantPipelineBuilderContext{TTenant}"/> class.
        /// </summary>
        public TenantPipelineBuilderContext(TenantContext<TTenant> tenantContext)
        {
            this.tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        /// <summary>
        /// Gets tenant object.
        /// </summary>
        public TTenant Tenant => tenantContext.Tenant;
    }
}
