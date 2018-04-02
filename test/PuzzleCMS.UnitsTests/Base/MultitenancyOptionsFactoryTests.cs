using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Options;
using Puzzle.Core.Multitenancy.Internal;
using Puzzle.Core.Multitenancy.Internal.Options;

namespace PuzzleCMS.UnitsTests.Base
{
    public class MultitenancyOptionsFactoryTests : IOptionsFactory<MultitenancyOptions>
    {
        static readonly List<AppTenant> TenantList = new List<AppTenant>(){
                                                   new AppTenant(){
                                                        Name = "Tenant 1", Hostnames = new string[]
                                                        {
                                                           "/tenant-1-1",
                                                           "/tenant-1-2",
                                                           "/tenant-1-3"
                                                       }
                                                    },
                                                   new AppTenant(){
                                                        Name = "Tenant 2", Hostnames = new string[]
                                                        {
                                                           "/tenant-2-1",
                                                           "/tenant-2-1",
                                                           "/tenant-2-1"
                                                       }
                                                    }
                                                   ,
                                                   new AppTenant(){
                                                        Name = "Tenant 2", Hostnames = new string[]
                                                        {
                                                       }
                                                    }
        };

        public static MultitenancyOptions options = new MultitenancyOptions()
        {
            Tenants = new Collection<AppTenant>(TenantList)
        };

        public MultitenancyOptions Create(string name) => options;
    }
}
