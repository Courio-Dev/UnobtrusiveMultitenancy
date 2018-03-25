using System.Collections.ObjectModel;

namespace Puzzle.Core.Multitenancy.Internal.Options
{
    public class MultitenancyOptions
    {
        public Collection<AppTenant> Tenants { get; set; }
    }
}