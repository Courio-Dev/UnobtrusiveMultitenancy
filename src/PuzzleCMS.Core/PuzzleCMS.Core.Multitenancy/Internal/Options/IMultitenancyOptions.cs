using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMultitenancyOptions
    {
        /// <summary>
        /// 
        /// </summary>
        string TenantFolder { get; set; }


        /// <summary>
        /// 
        /// </summary>
        IEnumerable<IConfigurationSection> TenantsConfigurations { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IDictionary<string, string> Tokens { get; set; }
    }
}
