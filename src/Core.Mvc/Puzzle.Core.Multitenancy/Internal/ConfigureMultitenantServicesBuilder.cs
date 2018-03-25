using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Puzzle.Core.Multitenancy.Internal
{
    internal class ConfigureMultitenantServicesBuilder<TTenant>
    {
        public MethodInfo MethodInfo { get; }

        public ConfigureMultitenantServicesBuilder(MethodInfo configure)
        {
            MethodInfo = configure;
        }

        public Action<IServiceCollection, TTenant> Build(object instance) => (services, tenant) => Invoke(instance, services, tenant);

        private IServiceCollection Invoke(object instance, IServiceCollection services, TTenant tenant)
        {
            InvokeCore(instance, services, tenant);
            return services;
        }

        private void InvokeCore(object instance, IServiceCollection services, TTenant tenant)
        {
            if (MethodInfo == null)
            {
                return;
            }

            // Only support IServiceCollection parameters
            var parameters = MethodInfo.GetParameters();
            if (parameters.Length > 2
                 || parameters.Any(p => (p.ParameterType != typeof(IServiceCollection)) && (p.ParameterType != typeof(TTenant)))
               )
            {
                throw new InvalidOperationException("The ConfigurePerTenantServices method must take only two parameter one of type IServiceCollection and one of type TTenant.");
            }

            var arguments = new object[MethodInfo.GetParameters().Length];

            if (parameters.Length > 0)
            {
                arguments[0] = services;
                arguments[1] = tenant;
            }

            MethodInfo.Invoke(instance, arguments);
        }
    }
}