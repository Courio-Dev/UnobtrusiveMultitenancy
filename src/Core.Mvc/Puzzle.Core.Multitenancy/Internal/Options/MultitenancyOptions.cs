namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class MultitenancyOptions
    {
        public MultitenancyOptions()
        {
            OtherTokens = new Dictionary<string, string>();
        }

        public string AppTenantFolder { get; set; }

        public IDictionary<string, string> OtherTokens { get; set; }

        public Collection<AppTenant> Tenants { get; set; }
    }
}
