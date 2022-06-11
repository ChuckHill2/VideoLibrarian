using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoLibrarian
{
    /// <summary>
    /// Generic dictionary-like list for small sets of items (generally less than 20 items).
    /// Duplicate keys will not be added, however the existing value will be updated.
    /// Does not throw exceptions. Invalid data is just ignored.
    /// </summary>
    /// <typeparam name="K">Type of key</typeparam>
    /// <typeparam name="V">Type of value</typeparam>
    public class KeyValueList<K, V> : IDictionary<K, V>
    {
        //Property is initialized only upon demand
        private List<KeyValueItem<K, V>> _Items = null;
        private List<KeyValueItem<K, V>> Items
        {
            get
            {
                if (_Items==null) _Items = new List<KeyValueItem<K, V>>();
                return _Items;
            }
        }

        public ICollection<K> Keys => Items.Select(m => m.Key).ToArray();
        public ICollection<V> Values => Items.Select(m => m.Value).ToArray();
        public int Count => Items.Count;
        public bool IsReadOnly => false;

        public KeyValueList() { }

        /// <summary>
        /// Get or set new or existing key/value pair.
        /// </summary>
        /// <param name="key">Key to search for</param>
        /// <returns>Associated value.</returns>
        public V this[K key]
        {
            get
            {
                if (_Items == null) return default(V);
                var kv = Items.FirstOrDefault(m => m.Key.Equals(key));
                return kv == null ? default(V) : kv.Value;
            }
            set
            {
                if (((object)key) == null) return; //disallow null keys.
                var kv = Items.FirstOrDefault(m => m.Key.Equals(key));
                if (kv == null) Items.Add(new KeyValueItem<K, V>(key, value));
                else kv.Value = value;
            }
        }

        /// <summary>
        /// Remove key/value pair from list.
        /// </summary>
        /// <param name="key">Key to find. If not found then this does nothing.</param>
        public bool Remove(K key)
        {
            int i = Items.IndexOf(m => m.Key.Equals(key));
            if (i == -1) return false;
            Items.RemoveAt(i);
            return true;
        }

        /// <summary>
        /// Convert the content of this into a string useful for deserializing. 
        /// </summary>
        /// <returns>Stringized content.</returns>
        public string Serialize()
        {
            var sb = new StringBuilder();
            var delimiter = string.Empty;
            foreach (var kv in Items)
            {
                sb.Append(delimiter);
                sb.Append(kv.Key);
                sb.Append("=");
                sb.Append(kv.Value);
                delimiter = ";";
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert key/value string into an initialized KeyValueList object.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Initialized KeyValueList object</returns>
        public static KeyValueList<K, V> Deserialize(string input)
        {
            var kvItems = input.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var kvList = new KeyValueList<K, V>();

            foreach (string kv in kvItems)
            {
                var kvItem = kv.Split('='); //Key may be string.Empty so "...;=value" is valid.
                if (kvItem.Length != 2) continue;
                K key;
                V value;
                try
                {
                    key = (K)Convert.ChangeType(kvItem[0].Trim(), typeof(K));
                    value = (V)Convert.ChangeType(kvItem[1].Trim(), typeof(V));
                }
                catch (Exception) { continue; }
                var exists = kvList.Items.FirstOrDefault(m => m.Key.Equals(key)) != null;
                if (exists) continue;
                kvList[key] = value;
            }

            return kvList;
        }

        public override string ToString() => $"Count={Items.Count}";

        public bool ContainsKey(K key) => Items.FirstOrDefault(m => m.Key.Equals(key)) != null;

        public void Add(K key, V value)
        {
            if (((object)key) == null) return; //disallow null keys.
            var kv = Items.FirstOrDefault(m => m.Key.Equals(key));
            if (kv == null) Items.Add(new KeyValueItem<K, V>(key, value));
            else kv.Value = value;
        }

        public bool TryGetValue(K key, out V value)
        {
            value = default(V);
            if (_Items == null) return false;
            var kv = Items.FirstOrDefault(m => m.Key.Equals(key));
            if (kv == null) return false;
            value = kv.Value;
            return true;
        }

        public void Add(KeyValuePair<K, V> item)
        {
            if (((object)item.Key) == null) return; //disallow null keys.
            var kv = Items.FirstOrDefault(m => m.Key.Equals(item.Key));
            if (kv == null) Items.Add(new KeyValueItem<K, V>(item.Key, item.Value));
            else kv.Value = item.Value;
        }

        public void Clear() => Items.Clear();

        public bool Contains(KeyValuePair<K, V> item)
        {
            var kv = Items.FirstOrDefault(m => m.Key.Equals(item.Key));
            if (kv == null) return false;
            return kv.Value.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<K, V> item) => throw new NotImplementedException();
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        private class KeyValueItem<KK, VV>
        {
            public KK Key { get; private set; }
            public VV Value { get; set; }
            public KeyValueItem(KK key, VV value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
