namespace PuzzleCMS.Core.Multitenancy.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PuzzleCMS.Core.Multitenancy.Internal.Data;

    internal class ConfigureMultitenantServicesBuilder<TTenant, IConfiguration>
    {
        public ConfigureMultitenantServicesBuilder(MethodInfo configure)
        {
            MethodInfo = configure;
        }

        public MethodInfo MethodInfo { get; }

        public Action<IServiceCollection, TTenant, Microsoft.Extensions.Configuration.IConfiguration> Build(object instance) => 
            (services, tenant, tenantConfiguration) => _=Invoke(instance, services, tenant,tenantConfiguration);

        private IServiceCollection Invoke(object instance, 
            IServiceCollection services, 
            TTenant tenant, 
            Microsoft.Extensions.Configuration.IConfiguration tenantConfiguration)
        {
            InvokeCore(instance, services, tenant,tenantConfiguration);
            return services;
        }

        private void InvokeCore(object instance, 
            IServiceCollection services,
            in TTenant tenant, 
            Microsoft.Extensions.Configuration.IConfiguration tenantConfiguration)
        {
            if (MethodInfo == null)
            {
                return;
            }

            string[] typesIServiceCollection = new string[] { $"{typeof(IServiceCollection).FullName}", $"{typeof(IServiceCollection).FullName}&" };
            string[] typesTenant = new string[] { $"{typeof(TTenant).FullName}", $"{typeof(TTenant).FullName}&" };
            string[] typesTenantConfiguration = new string[] { $"{typeof(IConfiguration).FullName}", $"{typeof(IConfiguration).FullName}&" };
            string[] typesConnectionStrings = new string[] { $"{typeof(ConnectionStringSettingsCollection).FullName}", $"{typeof(ConnectionStringSettingsCollection).FullName}&" };

            // Only support IServiceCollection,TTenant and IConfiguration and ConnectionStringSettingsCollection  parameters
            ParameterInfo[] parameters = MethodInfo.GetParameters();
            if (parameters.Length > 4 || parameters.Any(p =>{
                     return !typesIServiceCollection.Contains(p.ParameterType.FullName)
                     && !typesTenant.Contains(p.ParameterType.FullName)
                     && !typesTenantConfiguration.Contains(p.ParameterType.FullName)
                     && !typesConnectionStrings.Contains(p.ParameterType.FullName);
            }))
            {
                throw new InvalidOperationException(@"The ConfigurePerTenantServices method must take only parameters 
                                                      of type IServiceCollection 
                                                      or one of type TTenant
                                                      or one of type IConfiguration.
                                                      or one of type ConnectionStringSettingsCollection.");
            }

            ////object[] arguments = new object[MethodInfo.GetParameters().Length];
            IDictionary<int, object> arguments = new Dictionary<int, object>(MethodInfo.GetParameters().Length);

            if (parameters.Length > 0)
            {
                int index = -1;
                arguments[AutoIncrementIndex(ref index)] = ThrowIfNull(services,nameof(services));
                if (parameters.Length >= 2)
                {
                    arguments[AutoIncrementIndex(ref index)] = ThrowIfNull(tenant, nameof(tenant));
                }
                if (parameters.Length >= 3)
                {  
                    arguments[AutoIncrementIndex(ref index)] = ThrowIfNull(tenantConfiguration, nameof(tenantConfiguration));
                }
                if (parameters.Length >= 4)
                {
                    arguments[AutoIncrementIndex(ref index)] =ConnectionStringSettingsExtensions.ConnectionStrings(tenantConfiguration);
                }
            }

            MethodInfo.Invoke(instance, arguments.Values.ToArray());
        }

        private static T ThrowIfNull<T>(T argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            return argument;
        }

        /// <summary>
        /// Inrement and retunr new value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static int AutoIncrementIndex(ref int id)
        {
            id += 1;
            return id;
        }
    }
}
