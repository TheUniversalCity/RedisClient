using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TheUniversalCity.RedisClient.InMemory
{
    public class RedisClientInMemoryDictionary : ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IDictionary<string, object>, IReadOnlyCollection<KeyValuePair<string, object>>, IReadOnlyDictionary<string, object>, ICollection, IDictionary
    {
        private readonly ConcurrentDictionary<string, AutoResetEvent> syncRoots = new ConcurrentDictionary<string, AutoResetEvent>();
        private readonly ConcurrentDictionary<string, object> values = new ConcurrentDictionary<string, object>();

        public object this[string key] { get => values[key]; set => values.TryAdd(key, value); }
        public object this[object key] { get => ((IDictionary)values)[key]; set => values.TryAdd((string)key, value); }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public int Count => values.Count;

        public bool IsSynchronized => (values as ICollection).IsSynchronized;

        public object SyncRoot => (values as ICollection).SyncRoot;

        public ICollection Keys => (ICollection)values.Keys;

        public ICollection Values => (ICollection)(values.Values);

        ICollection<string> IDictionary<string, object>.Keys => values.Keys;

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => values.Keys;

        ICollection<object> IDictionary<string, object>.Values => values.Values;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => values.Values;

        public void Add(object key, object value)
        {
            values.TryAdd((string)key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            values.TryAdd(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            values.TryAdd(key, value);
        }

        public void Clear()
        {
            syncRoots.Clear();
            values.Clear();
        }

        public bool Contains(object key)
        {
            return ((IDictionary)values).Contains(key);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary)values).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)values).CopyTo(array, index);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection)values).CopyTo(array, arrayIndex);
        }

        public object GetOrAdd(string key, Func<string, object> factory)
        {
            if (values.TryGetValue(key, out var result))
            {
                return result;
            }

            lock (syncRoots.GetOrAdd(key, (_key) =>  new AutoResetEvent(true)))
            {
                return values.GetOrAdd(key, factory);
            }
        }

        public async Task<object> GetOrAddAsync(string key, Func<string, Task<object>> factory)
        {
            if (values.TryGetValue(key, out var result))
            {
                return result;
            }

            var mre = syncRoots.GetOrAdd(key, (_key) => new AutoResetEvent(true));

            try
            {
                mre.WaitOne();

                if (values.TryGetValue(key, out var _result))
                {
                    return _result;
                }

                return values.GetOrAdd(key, await factory.Invoke(key));
            }
            finally
            {
                mre.Set();
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return ((IDictionary)values).GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)values).Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)values).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, object>)values).Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return values.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)values).GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)values).GetEnumerator();
        }

        internal void TryRemove(string key, out object obj)
        {
            values.TryRemove(key, out obj);
        }
    }
}
