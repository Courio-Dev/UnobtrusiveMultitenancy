using Microsoft.Extensions.Primitives;
using PuzzleCMS.Core.Multitenancy.Internal.Options;

namespace PuzzleCMS.Core.Multitenancy.Internal.Configurations
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
