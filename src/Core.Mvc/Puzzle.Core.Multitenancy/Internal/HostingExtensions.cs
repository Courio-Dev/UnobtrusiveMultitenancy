using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Diagnostics;

/// <summary>
/// Taken FROM https://github.com/aspnet-contrib/AspNet.Hosting.Extensions
/// Useful links :https://github.com/WebApiContrib/WebAPIContrib.Core/blob/master/src/WebApiContrib.Core/ParallelApplicationPipelinesExtensions.cs#L13
/// </summary>
namespace Puzzle.Core.Multitenancy.Internal
{
    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/>.
    /// </summary>
    internal static class HostingExtensions
    {
        /// <summary>
        /// If the request path starts with the given <paramref name="path"/>, execute the app configured via
        /// the configuration method of the <typeparamref name="TStartup"/> class instead of continuing to the next component
        /// in the pipeline. The new app will get an own newly created <see cref="ServiceCollection"/> and will not share
        /// the <see cref="ServiceCollection"/> of the originating app.
        /// </summary>
        /// <typeparam name="TStartup">The startup class used to configure the new app and the service collection.</typeparam>
        /// <param name="app">The application builder to register the isolated map with.</param>
        /// <param name="path">The path to match. Must not end with a '/'.</param>
        /// <returns>The new pipeline with the isolated middleware configured.</returns>
        internal static IApplicationBuilder IsolatedMap<TStartup>(this IApplicationBuilder app, PathString path)
            where TStartup : class
        {
            return app.IsolatedMap(typeof(TStartup), path);
        }

        internal static IApplicationBuilder IsolatedMapWhen<TStartup>(this IApplicationBuilder app, Func<HttpContext, bool> predicate)
        {
            return app.IsolatedMapWhen(typeof(TStartup), predicate);
        }

        internal static IApplicationBuilder IsolatedMap(this IApplicationBuilder app, Type startupType, PathString path)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            var environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var methods = Microsoft.AspNetCore.Hosting.Internal.StartupLoader.LoadMethods(app.ApplicationServices, startupType, environment.EnvironmentName);

            return app.IsolatedMap(path, methods.ConfigureDelegate, methods.ConfigureServicesDelegate);
        }

        internal static IApplicationBuilder IsolatedMapWhen(this IApplicationBuilder app, Type startupType, Func<HttpContext, bool> predicate)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            var environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var methods = Microsoft.AspNetCore.Hosting.Internal.StartupLoader.LoadMethods(app.ApplicationServices, startupType, environment.EnvironmentName);

            return app.IsolatedMapWhen(predicate, methods.ConfigureDelegate, methods.ConfigureServicesDelegate);
        }

        internal static IApplicationBuilder IsolatedMapWhen(
            this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> configuration, Func<IServiceCollection, IServiceProvider> registration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return app.Isolate(builder => builder.MapWhen(predicate, configuration), registration);
        }

        //public static IApplicationBuilder IsolatedMapWhen(this IApplicationBuilder app, Type startupType, PathString path)
        //{
        //    if (app == null) throw new ArgumentNullException(nameof(app));

        //    var environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
        //    var methods = Microsoft.AspNetCore.Hosting.Internal.StartupLoader.LoadMethods(app.ApplicationServices, startupType, environment.EnvironmentName);

        //    return app.IsolatedMap(path, methods.ConfigureDelegate, methods.ConfigureServicesDelegate);

        //    if (app == null) throw new ArgumentNullException(nameof(app));
        //    if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        //    if (registration == null) throw new ArgumentNullException(nameof(registration));

        //    if (path.HasValue && path.Value.EndsWith("/", StringComparison.Ordinal))
        //    {
        //        throw new ArgumentException("The path must not end with a '/'", nameof(path));
        //    }

        //    return app.Isolate(builder => builder.MapWhen(path, configuration), registration);
        //}

        /// <summary>
        /// If the request path starts with the given <paramref name="path"/>, execute the app configured via
        /// <paramref name="configuration"/> parameter instead of continuing to the next component in the pipeline.
        /// The new app will get an own newly created <see cref="ServiceCollection"/> and will not share the
        /// <see cref="ServiceCollection"/> from the originating app.
        /// </summary>
        /// <param name="app">The application builder to register the isolated map with.</param>
        /// <param name="path">The path to match. Must not end with a '/'.</param>
        /// <param name="configuration">The branch to take for positive path matches.</param>
        /// <param name="registration">A method to configure the newly created service collection.</param>
        /// <returns>The new pipeline with the isolated middleware configured.</returns>
        internal static IApplicationBuilder IsolatedMap(this IApplicationBuilder app, PathString path, Action<IApplicationBuilder> configuration, Action<IServiceCollection> registration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return app.IsolatedMap(path, configuration, services =>
            {
                registration(services);

                return services.BuildServiceProvider();
            });
        }

        /// <summary>
        /// If the request path starts with the given <paramref name="path"/>, execute the app configured via
        /// <paramref name="configuration"/> parameter instead of continuing to the next component in the pipeline.
        /// The new app will get an own newly created <see cref="ServiceCollection"/> and will not share the
        /// <see cref="ServiceCollection"/> from the originating app.
        /// </summary>
        /// <param name="app">The application builder to register the isolated map with.</param>
        /// <param name="path">The path to match. Must not end with a '/'.</param>
        /// <param name="configuration">The branch to take for positive path matches.</param>
        /// <param name="registration">A method to configure the newly created service collection.</param>
        /// <returns>The new pipeline with the isolated middleware configured.</returns>
        internal static IApplicationBuilder IsolatedMap(
            this IApplicationBuilder app, PathString path, Action<IApplicationBuilder> configuration, Func<IServiceCollection, IServiceProvider> registration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            if (path.HasValue && path.Value.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The path must not end with a '/'", nameof(path));
            }

            return app.Isolate(builder => builder.Map(path, configuration), registration);
        }

        /// <summary>
        /// Creates a new isolated application builder which gets its own <see cref="ServiceCollection"/>, which only
        /// has the default services registered. It will not share the <see cref="ServiceCollection"/> from the
        /// originating app. The isolated map will be configured using the configuration methods of the
        /// <typeparamref name="TStartup"/> class.
        /// </summary>
        /// <typeparam name="TStartup">The startup class used to configure the new app and the service collection.</typeparam>
        /// <param name="app">The application builder to create the isolated app from.</param>
        /// <returns>The new pipeline with the isolated application integrated.</returns>
        internal static IApplicationBuilder Isolate<TStartup>(this IApplicationBuilder app) where TStartup : class
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var methods = Microsoft.AspNetCore.Hosting.Internal.StartupLoader.LoadMethods(app.ApplicationServices, typeof(TStartup), environment.EnvironmentName);

            return app.Isolate(methods.ConfigureDelegate, methods.ConfigureServicesDelegate);
        }

        /// <summary>
        /// Creates a new isolated application builder which gets its own <see cref="ServiceCollection"/>, which only
        /// has the default services registered. It will not share the <see cref="ServiceCollection"/> from the
        /// originating app.
        /// </summary>
        /// <param name="app">The application builder to create the isolated app from.</param>
        /// <param name="configuration">The branch of the isolated app.</param>
        /// <returns>The new pipeline with the isolated application integrated.</returns>
        internal static IApplicationBuilder Isolate(this IApplicationBuilder app, Action<IApplicationBuilder> configuration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return app.Isolate(configuration, services => services.BuildServiceProvider());
        }

        /// <summary>
        /// Creates a new isolated application builder which gets its own <see cref="ServiceCollection"/>, which only
        /// has the default services registered. It will not share the <see cref="ServiceCollection"/> from the
        /// originating app.
        /// </summary>
        /// <param name="app">The application builder to create the isolated app from.</param>
        /// <param name="configuration">The branch of the isolated app.</param>
        /// <param name="registration">A method to configure the newly created service collection.</param>
        /// <returns>The new pipeline with the isolated application integrated.</returns>
        internal static IApplicationBuilder Isolate(this IApplicationBuilder app, Action<IApplicationBuilder> configuration, Action<IServiceCollection> registration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return app.Isolate(configuration, services =>
            {
                registration(services);

                return services.BuildServiceProvider();
            });
        }

        /// <summary>
        /// Creates a new isolated application builder which gets its own <see cref="ServiceCollection"/>, which only
        /// has the default services registered. It will not share the <see cref="ServiceCollection"/> from the
        /// originating app.
        /// </summary>
        /// <param name="app">The application builder to create the isolated app from.</param>
        /// <param name="configuration">The branch of the isolated app.</param>
        /// <param name="registration">A method to configure the newly created service collection.</param>
        /// <returns>The new pipeline with the isolated application integrated.</returns>
        internal static IApplicationBuilder Isolate(this IApplicationBuilder app, Action<IApplicationBuilder> configuration, Func<IServiceCollection, IServiceProvider> registration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var services = CreateDefaultServiceCollection(app.ApplicationServices);
            var provider = registration(services);

            var builder = new ApplicationBuilder(null);
            builder.ApplicationServices = provider;

            builder.Use(async (context, next) =>
            {
                var factory = provider.GetRequiredService<IServiceScopeFactory>();

                // Store the original request services in the current ASP.NET context.
                context.Items[typeof(IServiceProvider)] = context.RequestServices;

                try
                {
                    using (var scope = factory.CreateScope())
                    {
                        context.RequestServices = scope.ServiceProvider;

                        await next();
                    }
                }
                finally
                {
                    context.RequestServices = null;
                }
            });

            configuration(builder);

            return app.Use(next =>
            {
                // Run the rest of the pipeline in the original context,
                // with the services defined by the parent application builder.
                builder.Run(async context =>
                {
                    var factory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

                    try
                    {
                        using (var scope = factory.CreateScope())
                        {
                            context.RequestServices = scope.ServiceProvider;

                            await next(context);
                        }
                    }
                    finally
                    {
                        context.RequestServices = null;
                    }
                });

                var branch = builder.Build();

                return context => branch(context);
            });
        }

        /// <summary>
        /// This creates a new <see cref="ServiceCollection"/> with the same services registered as the
        /// <see cref="WebHostBuilder"/> does when creating a new <see cref="ServiceCollection"/>.
        /// </summary>
        /// <param name="provider">The service provider used to retrieve the default services.</param>
        /// <returns>A new <see cref="ServiceCollection"/> with the default services registered.</returns>
        private static ServiceCollection CreateDefaultServiceCollection(IServiceProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            var services = new ServiceCollection();

            // Copy the services added by the hosting layer (WebHostBuilder.BuildHostingServices).
            // See https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNetCore.Hosting/WebHostBuilder.cs.

            services.AddLogging();

            if (provider.GetService<IHttpContextAccessor>() != null)
            {
                services.AddSingleton(provider.GetService<IHttpContextAccessor>());
            }

            services.AddSingleton(provider.GetRequiredService<IHostingEnvironment>());
            services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());
            services.AddSingleton(provider.GetRequiredService<IApplicationLifetime>());
            services.AddSingleton(provider.GetRequiredService<IHttpContextFactory>());

            services.AddSingleton(provider.GetRequiredService<DiagnosticSource>());
            services.AddSingleton(provider.GetRequiredService<DiagnosticListener>());
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            return services;
        }
    }
}