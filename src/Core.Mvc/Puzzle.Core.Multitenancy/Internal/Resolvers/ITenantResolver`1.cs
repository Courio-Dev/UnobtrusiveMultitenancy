namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Resolver for tenant.
    /// </summary>
    /// <typeparam name="TTenant">Tenant object.</typeparam>
    public interface ITenantResolver<TTenant>
    {
        /// <summary>
        /// Resolve tenant within HttpContext.
        /// </summary>
        /// <param name="context">httpcontext.</param>
        /// <returns>Tenant's context.</returns>
        Task<TenantContext<TTenant>> ResolveAsync(HttpContext context);

        /// <summary>
        /// Clear data.
        /// </summary>
        void Reset();
    }
}
