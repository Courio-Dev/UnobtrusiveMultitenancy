using System;

namespace Puzzle.Core.Multitenancy
{
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