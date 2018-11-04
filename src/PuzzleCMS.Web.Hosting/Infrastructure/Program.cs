using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Extensions.Logging;

namespace PuzzleCMS.Web.Hosting
{
    public static partial class Program
    {
        private const string BasePathName = "Configs";
        private const string HostingJsonFileName = "hosting.json";

        private static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                        .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), BasePathName))
                        .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                        .Build();
        }

        private static Serilog.ILogger GetSeriLogger()
        {
            return new LoggerConfiguration()
                        .ReadFrom.Configuration(GetConfiguration())
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
                        .CreateLogger();
        }

        private static SerilogLoggerProvider GetSerilogLoggerProvider()
        {
            return new SerilogLoggerProvider(GetSeriLogger(), dispose: true);
        }

    }
}
