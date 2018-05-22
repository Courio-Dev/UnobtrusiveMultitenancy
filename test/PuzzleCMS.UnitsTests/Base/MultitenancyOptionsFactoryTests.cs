namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// Factory for MultitenancyOptions test.
    /// </summary>
    public class MultitenancyOptionsFactoryTests : IOptionsFactory<MultitenancyOptions>
    {
        private readonly List<AppTenant> tenantList = new List<AppTenant>()
        {
              new AppTenant()
              {
                   Name = "Tenant 1", Hostnames = new string[]
                   {
                      "/tenant-1-1",
                      "/tenant-1-2",
                      "/tenant-1-3",
                  },
               },
              new AppTenant()
              {
                   Name = "Tenant 2", Hostnames = new string[]
                   {
                      "/tenant-2-1",
                      "/tenant-2-1",
                      "/tenant-2-1",
                  },
               },
              new AppTenant()
              {
                   Name = "Tenant 2", Hostnames = new string[]
                   {
                  },
               },
        };

        /// <summary>
        /// Returns MultitenancyOptions for test.
        /// </summary>
        /// <returns>MultitenancyOptions.</returns>
        public MultitenancyOptions Create(string name)
        {
            return new MultitenancyOptions()
            {
                Tenants = new Collection<AppTenant>(tenantList),
            };
        }
    }
}
