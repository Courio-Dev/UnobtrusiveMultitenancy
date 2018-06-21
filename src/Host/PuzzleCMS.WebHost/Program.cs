namespace PuzzleCMS.WebHost
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Extensions;
    using Serilog;

    /// <summary>
    /// Program class.
    /// </summary>
    public sealed class Program
    {
        private const string BasePathName = "Configs";
        private const string HostingJsonFileName = "hosting.json";

        /// <summary>
        /// The entry point.
        /// </summary>
        public static int Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), BasePathName))
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Fatal)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");

                CreateWebHostBuilder(args)
                .Build()
                .Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
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
                  .UseSerilog()
                  .ConfigureAppConfiguration((context, configBuilder) =>
                   {
                       ConfigureConfigurationBuilder(context, configBuilder, args);
                   })
                  .ConfigureLogging((context, logging) =>
                  {
                      // clear all previously registered providers
                      logging.ClearProviders();
                  })
                  .UseIISIntegration()
                  .UseUnobtrusiveMulitenancyStartupWithDefaultConvention<Startup>()
                  .UseDefaultServiceProvider(options =>options.ValidateScopes = false)
                  ;
        }

        private static void ConfigureConfigurationBuilder(WebHostBuilderContext ctx, IConfigurationBuilder config, string[] args)
        {
            IHostingEnvironment env = ctx.HostingEnvironment;

            config
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configs"))
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddJsonFile(HostingJsonFileName, optional: false, reloadOnChange: true)
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .AddXmlFile("settings.xml", optional: true, reloadOnChange: true)
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)

                .AddJsonFile("MultitenancyOptions.json", optional: false, reloadOnChange: true)

                // This reads the configuration keys from the secret store. This allows you to store connection strings
                // and other sensitive settings, so you don't have to check them into your source control provider.
                // Only use this in Development, it is not intended for Production use. See
                // http://docs.asp.net/en/latest/security/app-secrets.html
                .AddUserSecrets<Startup>()
                .AddApplicationInsightsSettings(developerMode: env.IsProduction())
                .Build();
        }

        private static void ConfigureLogger(WebHostBuilderContext ctx, ILoggingBuilder logging)
        {
            logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
        }

        private static int? BuildSslPort(WebHostBuilderContext ctx)
        {
            int? sslPort = null;
            if (ctx.HostingEnvironment.IsDevelopment())
            {
                IConfigurationRoot launchConfiguration = new ConfigurationBuilder()
                    .SetBasePath(ctx.HostingEnvironment.ContentRootPath)
                    .AddJsonFile(@"Properties\launchSettings.json")
                    .Build();
                sslPort = launchConfiguration.GetValue<int>("iisSettings:iisExpress:sslPort");
            }

            return sslPort;
        }
    }
}
