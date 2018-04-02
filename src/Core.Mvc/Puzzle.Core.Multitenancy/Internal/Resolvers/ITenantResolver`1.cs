namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface ITenantResolver<TTenant>
    {
        Task<TenantContext<TTenant>> ResolveAsync(HttpContext context);
    }
}
