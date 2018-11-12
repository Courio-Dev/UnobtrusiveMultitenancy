using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PuzzleCMS.Core.Multitenancy.Extensions;
using PuzzleCMS.Core.Multitenancy.Internal;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.AspNetCore;

namespace SampleMvcWithDatabase
{
    /// <summary>
    /// Program class.
    /// </summary>
    public static class Program
    {
        private const string BasePathName = "Configs";
        private const string HostingJsonFileName = "hosting.json";

        /// <summary>
        /// The entry point.
        /// </summary>
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting web host");

                CreateWebHostBuilder(args)
                            .Build()
                            .Run();

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Build the IWebHostBuilder.
        /// </summary>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), BasePathName))
                   .AddJsonFile(HostingJsonFileName, optional: true)
                   .AddEnvironmentVariables()
                   .AddCommandLine(args)
                   .Build();

            return Microsoft.AspNetCore.WebHost
                  .CreateDefaultBuilder()
                  .UseConfiguration(config)
                  .UseIISIntegration()
                  .UseUnobtrusiveMulitenancyStartupWithDefaultConvention<Startup>()
                  ;
        }
    }
}
