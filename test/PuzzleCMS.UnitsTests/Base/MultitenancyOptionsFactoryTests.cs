namespace PuzzleCMS.UnitsTests.Base
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Options;

    /// <summary>
    /// Factory for MultitenancyOptions test.
    /// </summary>
    public class MultitenancyOptionsAppTenantFactoryTests : IOptionsFactory<MultitenancyOptions<AppTenant>>
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
        public MultitenancyOptions<AppTenant> Create(string name)
        {
            return new MultitenancyOptions<AppTenant>()
            {
                Tenants = new Collection<AppTenant>(tenantList),
            };
        }
    }

    public class MultitenancyOptionsTestTenantFactoryTests : IOptionsFactory<MultitenancyOptions<TestTenant>>
    {
        private readonly List<TestTenant> tenantList = new List<TestTenant>()
        {
              new TestTenant()
              {
                   Name = "Tenant 1", Hostnames = new string[]
                   {
                      "/tenant-1-1",
                      "/tenant-1-2",
                      "/tenant-1-3",
                  },
               },
              new TestTenant()
              {
                   Name = "Tenant 2", Hostnames = new string[]
                   {
                      "/tenant-2-1",
                      "/tenant-2-1",
                      "/tenant-2-1",
                  },
               },
              new TestTenant()
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
        public MultitenancyOptions<TestTenant> Create(string name)
        {
            return new MultitenancyOptions<TestTenant>()
            {
                Tenants = new Collection<TestTenant>(tenantList),
            };
        }
    }
}
