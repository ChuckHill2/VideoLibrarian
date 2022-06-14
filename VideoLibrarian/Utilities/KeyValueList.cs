//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="KeyValueList.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/VideoLibrarian</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VideoLibrarian
{
    /// <summary>
    /// Generic dictionary-like list.<br/>
    /// This may be treated as both a list and a dictionary.<br/>
    /// Duplicate keys cannot be added, however the existing value-part will be updated.<br/>
    /// Does not throw exceptions. Invalid data is just ignored.<br/>
    /// This may be XML serialized (unlike Dictionary objects).
    /// </summary>
    /// <typeparam name="K">Type of key</typeparam>
    /// <typeparam name="V">Type of value</typeparam>
    /// <remarks>
    /// This 'dictionary' is implemented as an array. For the first 20 items, the array is unsorted and a 
    /// linear search is performed. After 20 items, the array is sorted and a binary search is performed.<br/>
    /// <seealso cref="System.Collections.SortedList"/>
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class KeyValueList<K, V> : IList<KeyValueItem<K, V>>, IDictionary<K, V>
    {
        private const int MaxLinearSearch = 20;
        private bool IsSorted = false;
        private IComparer<KeyValueItem<K, V>> KeyComparer;
        private List<KeyValueItem<K, V>> _Items = null;  //Property is initialized only upon demand
        private List<KeyValueItem<K, V>> Items
        {
            get
            {
                if (_Items == null) _Items = new List<KeyValueItem<K, V>>();
                return _Items;
            }
        }

        public ICollection<K> Keys => Items.Select(m => m.Key).ToArray();
        public ICollection<V> Values => Items.Select(m => m.Value).ToArray();
        public int Count => Items.Count;
        public bool IsReadOnly => false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Lambda key comparison method or null for default comparer.</param>
        public KeyValueList(Comparison<K> comparer)
        {
            Comparer<K> keyComparer = comparer == null ? Comparer<K>.Default : Comparer<K>.Create(comparer);
            KeyComparer = new MyComparer(keyComparer);
        }

        /// <summary>
        /// Default Parameterless Constructor
        /// </summary>
        public KeyValueList() : this((Comparison<K>)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dictList">Initialize with String containing a semi-colon delimited array of equals-demimited key-value pairs.</param>
        /// <param name="comparer">Lambda key comparison method or null for default comparer.</param>
        /// <remarks>Warning: Keys are not validated for uniqueness.</remarks>
        public KeyValueList(string dictList, Comparison<K> comparer = null) : this(comparer)
        {
            var kvItems = dictList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

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
                this[key] = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Initialize with this collection.</param>
        /// <param name="comparer">Lambda key comparison method or null for default comparer.</param>
        public KeyValueList(IEnumerable<KeyValueItem<K, V>> collection, Comparison<K> comparer = null) : this(comparer)
        {
            Items.AddRange(collection.Where(kv => ((object)kv.Key) != null).GroupBy(kv => kv.Key).Select(g => g.First()).OrderBy(m => m.Key));
            IsSorted = Items.Count >= MaxLinearSearch;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Initialize with this collection.</param>
        /// <param name="comparer">Lambda key comparison method or null for default comparer.</param>
        public KeyValueList(IEnumerable<KeyValuePair<K, V>> collection, Comparison<K> comparer = null) : this(comparer)
        {
            Items.AddRange(collection.Where(kv => ((object)kv.Key) != null).GroupBy(kv => kv.Key).Select(g => (KeyValueItem<K, V>)g.First()).OrderBy(m => m.Key));
            IsSorted = Items.Count >= MaxLinearSearch;
        }

        public V this[K key]
        {
            get
            {
                if (_Items == null) return default(V);
                var i = FindIndex(key);
                return i < 0 ? default(V) : Items[i].Value;
            }
            set
            {
                if (((object)key) == null) return; //disallow null keys.
                var i = FindIndex(key);
                if (i < 0) Insert(~i, new KeyValueItem<K, V>(key, value));
                else Items[i].Value = value;
            }
        }
        public KeyValueItem<K, V> this[int index]
        {
            get => Items[index];
            set
            {
                if (Items.Count >= MaxLinearSearch)
                {
                    Items.Sort(KeyComparer);
                    IsSorted = true;
                }
                var i = FindIndex(value.Key);
                if (i < 0) Items.Insert(~i, value);
                else Items[i].Value = value.Value;
            }
        }

        /// <summary>
        /// Convert collection into string containing a semi-colon delimited array of equals-demimited key-value pairs. See constructor: KeyValueList(string)
        /// </summary>
        public override string ToString()
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

        public void Add(K key, V value) => this[key] = value;
        public void Add(KeyValueItem<K, V> item) => this[0] = item;
        public void Add(KeyValuePair<K, V> item) => this[0] = item;

        public bool TryGetValue(K key, out V value)
        {
            value = default(V);
            if (_Items == null) return false;
            var i = FindIndex(key);
            if (i < 0) return false;
            value = Items[i].Value;
            return true;
        }

        public void Clear() => Items.Clear();

        public bool ContainsKey(K key) => FindIndex(key) >= 0;
        public bool Contains(KeyValueItem<K, V> item) => Items.Contains(item);
        public bool Contains(KeyValuePair<K, V> item) => Items.Contains(item);

        public int IndexOf(KeyValueItem<K, V> item) => IndexOf(item.Key);
        public int IndexOf(K key)
        {
            var i = FindIndex(key);
            if (i < 0) i = -1;
            return i;
        }
        public void Insert(int index, KeyValueItem<K, V> item) => this[index] = item;

        public void RemoveAt(int index) => Items.RemoveAt(index);
        public bool Remove(K key)
        {
            var i = FindIndex(key);
            if (i < 0) return false;
            Items.RemoveAt(i);
            return true;
        }
        public bool Remove(KeyValueItem<K, V> item) => Items.Remove(item);
        public bool Remove(KeyValuePair<K, V> item) => Items.Remove(item);

        public void CopyTo(KeyValueItem<K, V>[] array, int arrayIndex = 0)
        {
            for (int i = arrayIndex; i < array.Length; i++) Add(array[i]);
        }
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex = 0)
        {
            for (int i = arrayIndex; i < array.Length; i++) Add(array[i]);
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => new KeyValuePairEnumerator(Items);
        public IEnumerator<KeyValueItem<K, V>> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        private int FindIndex(K key)
        {
            var kv = new KeyValueItem<K, V>(key, default(V));
            if (IsSorted) return Items.BinarySearch(kv, KeyComparer);

            int len = Items.Count;
            for (int index = 0; index < len; index++)
            {
                if (KeyComparer.Compare(Items[index], kv) == 0) return index;
            }
            return ~len;
        }

        private class MyComparer : IComparer<KeyValueItem<K, V>>
        {
            Comparer<K> keyComparer;
            public MyComparer(Comparer<K> comparer) => keyComparer = comparer;
            public int Compare(KeyValueItem<K, V> x, KeyValueItem<K, V> y) => keyComparer.Compare(x.Key, y.Key);
        }

        [Serializable]
        public struct KeyValuePairEnumerator : IEnumerator<KeyValuePair<K, V>>, IDisposable, IEnumerator
        {
            private List<KeyValueItem<K, V>> list;
            private int index;
            private KeyValuePair<K, V> current;

            internal KeyValuePairEnumerator(List<KeyValueItem<K, V>> list)
            {
                this.list = list;
                this.index = 0;
                this.current = default(KeyValuePair<K, V>);
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if ((uint)this.index >= (uint)list.Count)
                    return this.MoveNextRare();
                this.current = list[this.index];
                ++this.index;
                return true;
            }

            private bool MoveNextRare()
            {
                this.index = this.list.Count + 1;
                this.current = default(KeyValuePair<K, V>);
                return false;
            }

            public KeyValuePair<K, V> Current => this.current;

            object IEnumerator.Current => (object)this.Current;

            void IEnumerator.Reset()
            {
                this.index = 0;
                this.current = default(KeyValuePair<K, V>);
            }
        }
    }

    /// <summary>
    /// Same as System.Collections.Generic.KeyValuePair, EXCEPT this is a mutable Class and serializable as XML attributes<br/>
    /// It is also implicitly castable to/from KeyValuePair for easy conversions.<br/>
    /// </summary>
    /// <typeparam name="K">The type of Key</typeparam>
    /// <typeparam name="V">The type of Value</typeparam>
    [Serializable]
    public class KeyValueItem<K, V> : IXmlSerializable
    {
        // Once Key is initially set it becomes read-only. A modified Key would cause unexpected
        // results or at worst exceptions in a Dictionary. Only Value is modifiable.

        public K Key { get; private set; }
        public V Value { get; set; }

        public KeyValueItem() { } //parameterless constructor required for XmlSerializer

        public KeyValueItem(K key, V value)
        {
            Key = key;
            Value = value;
        }

        // Implicitly cast between KeyValueItem and System.Collections.Generic.KeyValuePair
        public static implicit operator KeyValueItem<K, V>(KeyValuePair<K, V> kv) => new KeyValueItem<K, V>(kv.Key, kv.Value);
        public static implicit operator KeyValuePair<K, V>(KeyValueItem<K, V> kv) => new KeyValuePair<K, V>(kv.Key, kv.Value);

        // This is identical to KeyValuePair<K,V>.ToString();
        public override string ToString() => $"[{Key?.ToString() ?? "null"}, {Value?.ToString() ?? "null"}]";

        // Need to manually serialize because XmlSerializer is old and does not handle private setters.
        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            try { Key = (K)Convert.ChangeType(reader.GetAttribute("Name"), typeof(K)); } catch { Key = default(K); }
            try { Value = (V)Convert.ChangeType(reader.GetAttribute("Value"), typeof(V)); } catch { Value = default(V); }
            reader.ReadElementContentAsString(); //"Read" the entire element to skip to the next.
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", this.Key.ToString());
            writer.WriteAttributeString("Value", this.Value.ToString());
        }
    }
}
