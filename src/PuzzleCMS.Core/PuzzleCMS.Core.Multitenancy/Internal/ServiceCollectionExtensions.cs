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
                // ServiceDescriptor clonedService = new ServiceDescriptor(
                //    service.ServiceType,
                //    service.ImplementationType,
                //    service.Lifetime);
                clone.Add(service);
            }

            return clone;
        }
    }
}
