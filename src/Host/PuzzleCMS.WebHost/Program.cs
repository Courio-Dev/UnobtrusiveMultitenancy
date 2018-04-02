namespace PuzzleCMS.WebHost
{
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Puzzle.Core.Multitenancy.Extensions;

    public sealed class Program
    {
        private const string BasePathName = "Configs";
        private const string HostingJsonFileName = "hosting.json";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }

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
              .CaptureStartupErrors(false)
              .SuppressStatusMessages(false)
              .UseSetting(WebHostDefaults.DetailedErrorsKey, value: true.ToString().ToLower())
              .UseConfiguration(config)
              .ConfigureAppConfiguration((context, configBuilder) =>
               {
                   ConfigureConfigurationBuilder(context, configBuilder, args);
               })
              .UseDefaultServiceProvider((context, options) =>
              {
                  options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
              })
              .UseIISIntegration()
              .UseAzureAppServices()

              // .PreferHostingUrls(false)
              .UseUnobtrusiveMulitenancyStartupWithDefaultConvention<Startup>()
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
