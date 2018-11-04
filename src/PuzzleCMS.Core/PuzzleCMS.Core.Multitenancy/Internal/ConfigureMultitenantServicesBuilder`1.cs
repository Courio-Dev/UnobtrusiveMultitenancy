namespace PuzzleCMS.Core.Multitenancy.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class ConfigureMultitenantServicesBuilder<TTenant, IConfiguration>
    {
        public ConfigureMultitenantServicesBuilder(MethodInfo configure)
        {
            MethodInfo = configure;
        }

        public MethodInfo MethodInfo { get; }

        public Action<IServiceCollection, TTenant, IConfiguration> Build(object instance) => (services, tenant, tenantConfiguration) => _=Invoke(instance, services, tenant,tenantConfiguration);

        private IServiceCollection Invoke(object instance, IServiceCollection services, TTenant tenant, IConfiguration tenantConfiguration)
        {
            InvokeCore(instance, services, tenant,tenantConfiguration);
            return services;
        }

        private void InvokeCore(object instance, IServiceCollection services,in TTenant tenant, IConfiguration tenantConfiguration)
        {
            if (MethodInfo == null)
            {
                return;
            }

            string[] acceptedTypesIServiceCollection = new string[] { $"{typeof(IServiceCollection).FullName}", $"{typeof(IServiceCollection).FullName}&" };
            string[] acceptedTypesTenant = new string[] { $"{typeof(TTenant).FullName}", $"{typeof(TTenant).FullName}&" };
            string[] acceptedTypesTenantConfiguration = new string[] { $"{typeof(IConfiguration).FullName}", $"{typeof(IConfiguration).FullName}&" };

            // Only support IServiceCollection,TTenant and IConfiguration  parameters
            ParameterInfo[] parameters = MethodInfo.GetParameters();
            if (parameters.Length > 3 || parameters.Any(p =>{
                     return !acceptedTypesIServiceCollection.Contains(p.ParameterType.FullName)
                     && !acceptedTypesTenant.Contains(p.ParameterType.FullName)
                     && !acceptedTypesTenantConfiguration.Contains(p.ParameterType.FullName);
            }))
            {
                throw new InvalidOperationException("The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.");
            }

            object[] arguments = new object[MethodInfo.GetParameters().Length];

            if (parameters.Length > 0)
            {
                arguments[0] = ThrowIfNull(services,nameof(services));
                if (parameters.Length >= 2)
                {
                    arguments[1] = ThrowIfNull(tenant, nameof(tenant));
                }
                if (parameters.Length >= 3)
                {
                    arguments[2] = ThrowIfNull(tenantConfiguration, nameof(tenantConfiguration));
                }
            }

            MethodInfo.Invoke(instance, arguments);
        }

        private static T ThrowIfNull<T>(T argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            return argument;
        }
    }
}
