using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PuzzleCMS.Core.Multitenancy.Internal.Data
{
    /// <summary>
    /// https://stackoverflow.com/questions/40845542/how-to-read-a-connectionstring-with-provider-in-net-core
    /// </summary>
    public static class ConnectionStringSettingsExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ConnectionStringSettingsCollection ConnectionStrings(this IConfigurationRoot configuration, string section = "ConnectionStrings")
        {
            ConnectionStringSettingsCollection connectionStringCollection = configuration.GetSection(section).Get<ConnectionStringSettingsCollection>();
            if (connectionStringCollection == null)
            {
                return new ConnectionStringSettingsCollection();
            }

            return connectionStringCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ConnectionStringSettings ConnectionString(this IConfigurationRoot configuration, string name, string section = "ConnectionStrings")
        {

            ConnectionStringSettingsCollection connectionStringCollection = configuration.GetSection(section).Get<ConnectionStringSettingsCollection>();
            if (connectionStringCollection == null ||
                !connectionStringCollection.TryGetValue(name, out ConnectionStringSettings connectionStringSettings))
            {
                return null;
            }

            return connectionStringSettings;
        }
    }
}
