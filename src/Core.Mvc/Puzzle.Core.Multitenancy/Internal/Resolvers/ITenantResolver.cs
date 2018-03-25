using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Puzzle.Core.Multitenancy.Internal.Resolvers
{
    public interface ITenantResolver<TTenant>
    {
        Task<TenantContext<TTenant>> ResolveAsync(HttpContext context);
    }
}