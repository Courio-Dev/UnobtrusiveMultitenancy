namespace Puzzle.Core.Multitenancy.Extensions
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Puzzle.Core.Multitenancy.Constants;

    /// <summary>
    /// Multitenant extensions for <see cref="HttpContext"/>.
    /// </summary>
    internal static class MultitenancyHttpContextExtensions
    {
        private const string TenantContextKey = MultitenancyConstants.MultiTenantContextKey;

        internal static void SetTenantContext<TTenant>(this HttpContext context, TenantContext<TTenant> tenantContext)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
            }

            context.Items[TenantContextKey] = tenantContext ?? throw new ArgumentNullException($"Argument {nameof(tenantContext)} must not be null");
        }

        internal static TenantContext<TTenant> GetTenantContext<TTenant>(this HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
            }

            if (context.Items.TryGetValue(TenantContextKey, out object tenantContext))
            {
                return tenantContext as TenantContext<TTenant>;
            }

            return null;
        }

        internal static TTenant GetTenant<TTenant>(this HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Argument {nameof(context)} must not be null");
            }

            TenantContext<TTenant> tenantContext = GetTenantContext<TTenant>(context);

            if (tenantContext != null)
            {
                return tenantContext.Tenant;
            }

            return default(TTenant);
        }
    }
}
