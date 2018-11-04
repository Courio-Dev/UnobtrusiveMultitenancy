

namespace PuzzleCMS.Core.Multitenancy.Internal.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// https://stackoverflow.com/questions/40845542/how-to-read-a-connectionstring-with-provider-in-net-core
    /// </summary>
    public sealed class ConnectionStringSettings : IEquatable<ConnectionStringSettings>
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ConnectionStringSettings()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        public ConnectionStringSettings(string name, string connectionString)
            : this(name, connectionString, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        public ConnectionStringSettings(string name, string connectionString, string providerName)
        {
            Name = name;
            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ConnectionStringSettings other)
        {
            return string.Equals(Name, other.Name) && string.Equals(ConnectionString, other.ConnectionString) && string.Equals(ProviderName, other.ProviderName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConnectionStringSettings)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConnectionString != null ? ConnectionString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProviderName != null ? ProviderName.GetHashCode() : 0);
                return hashCode;
            }
        }

        /////// <summary>
        /////// 
        /////// </summary>
        /////// <param name="left"></param>
        /////// <param name="right"></param>
        /////// <returns></returns>
        ////public static bool operator ==(ConnectionStringSettings left, ConnectionStringSettings right) => Equals(left, right);

        /////// <summary>
        /////// 
        /////// </summary>
        /////// <param name="left"></param>
        /////// <param name="right"></param>
        /////// <returns></returns>
        ////public static bool operator !=(ConnectionStringSettings left, ConnectionStringSettings right)
        ////{
        ////    return !Equals(left, right);
        ////}
    }
}
