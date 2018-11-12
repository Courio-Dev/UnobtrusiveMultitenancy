namespace PuzzleCMS.Web.Hosting
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using PuzzleCMS.Core.Multitenancy.Extensions;
    using PuzzleCMS.Core.Multitenancy.Internal;
    using PuzzleCMS.WebHost.Infrastructure.Logging;
    using Serilog;
    using Serilog.Extensions.Logging;
    using Serilog.AspNetCore;

    /// <summary>
    /// Program class.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// The entry point.
        /// </summary>
        public static int Main(string[] args)
        {

            Log.Logger = GetSeriLogger();
            try
            {
                Log.Information("Starting web host");

                CreateWebHostBuilder(args)
                .UseSerilog(Log.Logger)
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
                  .UseUnobtrusiveMulitenancyStartupWithDefaultConvention<Startup>(optionsAction:(p,b)=> {
                      b.UseLogProvider(new SeriLogProvider(GetSerilogLoggerProvider()));
                      b.UseConfigureServicesTenant((sc, tenant) => { });
                      b.UseCustomServicesTenant((IServiceCollection sc, AppTenant tenant, IConfiguration tentantConfiguration) =>
                      {
                          try
                          {
                              Serilog.ILogger tenantLogger = new LoggerConfiguration().ReadFrom.Configuration(tentantConfiguration).CreateLogger();
                              return new SeriLogProvider(new SerilogLoggerProvider(tenantLogger, dispose: true));
                          }
                          catch
                          {
                              string fileName = $"App_Tenants/{tenant.Name}/Logs/log.txt";
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

#pragma warning disable S1144 // Unused private types or members should be removed
        private static void ConfigureLogger(WebHostBuilderContext ctx, ILoggingBuilder logging)
        {
            logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
        }
#pragma warning restore S1144 // Unused private types or members should be removed

#pragma warning disable S1144 // Unused private types or members should be removed
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
#pragma warning restore S1144 // Unused private types or members should be removed
    }
}
