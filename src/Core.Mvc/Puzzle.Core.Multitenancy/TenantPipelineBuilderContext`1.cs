namespace Puzzle.Core.Multitenancy
{
    using System;

    public class TenantPipelineBuilderContext<TTenant>
    {
        private readonly TenantContext<TTenant> tenantContext;

        public TenantPipelineBuilderContext(TenantContext<TTenant> tenantContext)
        {
            this.tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public TTenant Tenant => tenantContext.Tenant;
    }
}
