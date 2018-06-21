using Microsoft.Extensions.Primitives;
using Puzzle.Core.Multitenancy.Internal.Options;

namespace Puzzle.Core.Multitenancy.Internal.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    internal interface IMultitenancyOptionsProvider<TTenant>
    {
        IChangeToken ChangeTokenConfiguration { get; }

        /// <summary>
        /// 
        /// </summary>
        MultitenancyOptions<TTenant> MultitenancyOptions { get; }

        void Reload();
    }
}
