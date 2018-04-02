namespace Puzzle.Core.Multitenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy.Internal.Options;

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection Clone(this IServiceCollection serviceCollection)
        {
            IServiceCollection clone = new ServiceCollection();
            foreach (ServiceDescriptor service in serviceCollection)
            {
                clone.Add(service);
            }

            return clone;
        }
    }
}
