namespace PuzzleCMS.WebHost
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal;
    using PuzzleCMS.WebHost.Infrastructure.Logging;
    using Serilog;
    using Serilog.Extensions.Logging;

    /// <summary>
    /// Program class.
    /// </summary>
    public class Program
    {
        private const string BasePathName = "Configs";
        private const string HostingJsonFileName = "hosting.json";

        protected static IConfigurationRoot Configuration => new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), BasePathName))
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .Build();

        protected static Serilog.ILogger SeriLogger => new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
                .CreateLogger();


        protected static SerilogLoggerProvider SerilogLoggerProvider => new SerilogLoggerProvider(SeriLogger, dispose: true);

        /// <summary>
        /// The entry point.
        /// </summary>
        public static int Main(string[] args)
        {

            Log.Logger = SeriLogger;
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
                  .ConfigureAppConfiguration((context, configBuilder) => ConfigureConfigurationBuilder(context, configBuilder, args))
                  .ConfigureLogging((context, logging) => logging.ClearProviders())
                  .UseIISIntegration()
                  .UseUnobtrusiveMulitenancyStartupWithDefaultConvention<Startup>(actionConfiguration:(action)=> {
                      action.UseLogProvider(new SeriLogProvider(SerilogLoggerProvider));
                      action.UseConfigureServicesTenant((sc, tenant) => { });
                      action.UseCustomServicesTenant((IServiceCollection sc,AppTenant tenant,IConfiguration tentantConfiguration) =>
                      {
                          try
                          {
                              Serilog.ILogger tenantLogger = new LoggerConfiguration().ReadFrom.Configuration(tentantConfiguration)
                                                                   .CreateLogger();
                              return new SeriLogProvider(new SerilogLoggerProvider(tenantLogger, dispose: true));
                          }
                          catch
                          {
                              string fileName =$"App_Tenants/{tenant.Name}/Logs/log.txt";
                              Serilog.Core.Logger serilogger = new LoggerConfiguration()
                               .Enrich.FromLogContext()
                               .MinimumLevel.Verbose()
                               .WriteTo.File(fileName, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}", flushToDiskInterval: TimeSpan.FromSeconds(1), shared: true)
                               .CreateLogger();
                              return new SeriLogProvider(new SerilogLoggerProvider(serilogger, dispose: true));
                          }
                      });
                  });
        }

        private static void ConfigureConfigurationBuilder(WebHostBuilderContext ctx, IConfigurationBuilder config, string[] args)
        {
            IHostingEnvironment env = ctx.HostingEnvironment;

            config
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configs"))
                
                .AddJsonFile(HostingJsonFileName, optional: false, reloadOnChange: true)
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .AddXmlFile("settings.xml", optional: true, reloadOnChange: true)
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("MultitenancyOptions.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
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
