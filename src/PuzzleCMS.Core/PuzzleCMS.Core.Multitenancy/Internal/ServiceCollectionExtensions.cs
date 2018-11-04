namespace PuzzleCMS.Core.Multitenancy.Internal
{
    using Microsoft.Extensions.DependencyInjection;

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
