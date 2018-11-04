namespace PuzzleCMS.Core.Multitenancy.Internal.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Next is the collection of ConnectionStringSettings. 
    /// This is only necessary because the Name of the connection string is the key in the JSON notation. 
    /// In order to keep that name consistently attached we need to override Dictionary's Add method (but you can't do that because its not virtual). 
    /// So all we are REALLY doing is just wrapping a Dictionary internally with that extra bit in our own Add implementation. 
    /// Again this looks like a lot of code, but you'll see it's very monotonous boring stuff.
    /// https://stackoverflow.com/questions/40845542/how-to-read-a-connectionstring-with-provider-in-net-core
    /// </summary>
    public class ConnectionStringSettingsCollection : IDictionary<string, ConnectionStringSettings>
    {
        private readonly Dictionary<string, ConnectionStringSettings> connectionStrings;

        /// <summary>
        /// 
        /// </summary>
        public ConnectionStringSettingsCollection()
        {
            connectionStrings = new Dictionary<string, ConnectionStringSettings>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        public ConnectionStringSettingsCollection(int capacity)
        {
            connectionStrings = new Dictionary<string, ConnectionStringSettings>(capacity);
        }

        #region IEnumerable methods
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)connectionStrings).GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumeratorGetEnumerator()
        {
            return ((IEnumerable)connectionStrings).GetEnumerator();
        }

        #endregion

        #region IEnumerable<> methods
        IEnumerator<KeyValuePair<string, ConnectionStringSettings>> IEnumerable<KeyValuePair<string, ConnectionStringSettings>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).GetEnumerator();
        }
        #endregion

        #region ICollection<> methods
        void ICollection<KeyValuePair<string, ConnectionStringSettings>>.Add(KeyValuePair<string, ConnectionStringSettings> item)
        {
            ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).Add(item);
        }

        void ICollection<KeyValuePair<string, ConnectionStringSettings>>.Clear()
        {
            ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).Clear();
        }

        bool ICollection<KeyValuePair<string, ConnectionStringSettings>>.Contains(KeyValuePair<string, ConnectionStringSettings> item)
        {
            return ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).Contains(item);
        }

        void ICollection<KeyValuePair<string, ConnectionStringSettings>>.CopyTo(KeyValuePair<string, ConnectionStringSettings>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, ConnectionStringSettings>>.Remove(KeyValuePair<string, ConnectionStringSettings> item)
        {
            return ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count => ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).Count;

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly => ((ICollection<KeyValuePair<string, ConnectionStringSettings>>)connectionStrings).IsReadOnly;
        #endregion

        #region IDictionary<> methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, ConnectionStringSettings value)
        {
            // NOTE only slight modification, we add back in the Name of connectionString here (since it is the key)
            value.Name = key;
            connectionStrings[key]= value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return connectionStrings.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return connectionStrings.Remove(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out ConnectionStringSettings value)
        {
            return connectionStrings.TryGetValue(key, out value);
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ConnectionStringSettings this[string key]
        {
            get => connectionStrings[key];
            set => Add(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<string> Keys => connectionStrings.Keys;

        /// <summary>
        /// 
        /// </summary>
        public ICollection<ConnectionStringSettings> Values => connectionStrings.Values;
        #endregion
    }
}
