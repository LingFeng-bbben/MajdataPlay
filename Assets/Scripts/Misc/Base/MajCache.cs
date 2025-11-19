using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay
{
    internal static class MajCache<TKey,TValue>
    {
        readonly static ConcurrentDictionary<TKey, TValue> _storage = new(8, 1024);

        public static TValue GetOrAdd(TKey key, TValue value)
        {
            return _storage.GetOrAdd(key, value);
        }
        public static TValue GetOrAdd(in KeyValuePair<TKey,TValue> pair)
        {
            return GetOrAdd(pair.Key, pair.Value);
        }
        public static void Replace(TKey key, TValue value)
        {
            _storage[key] = value;
        }
        public static void Replace(in KeyValuePair<TKey, TValue> pair)
        {
            _storage[pair.Key] = pair.Value;
        }
        public static bool TryRemove(TKey key, out TValue value)
        {
            return _storage.TryRemove(key, out value);
        }
        public static bool TryGetValue(TKey key, out TValue result)
        {
            return _storage.TryGetValue(key, out result);
        }
        public static TValue GetValue(TKey key)
        {
            return _storage[key];
        }
    }
}
