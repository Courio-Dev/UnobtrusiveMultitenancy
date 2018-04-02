namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System.Collections.ObjectModel;

    public class MultitenancyOptions
    {
        public Collection<AppTenant> Tenants { get; set; }
    }
}
