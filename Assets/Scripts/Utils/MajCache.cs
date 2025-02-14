using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Utils
{
    internal static class MajCache<TKey,TValue>
    {
        static Dictionary<TKey, TValue> _storage = new(4096);

        public static void Add(TKey key,in TValue value)
        {
            _storage.Add(key, value);
        }
        public static void Add(in KeyValuePair<TKey,TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }
        public static void Replace(TKey key,in TValue value)
        {
            _storage[key] = value;
        }
        public static void Replace(in KeyValuePair<TKey, TValue> pair)
        {
            Replace(pair.Key, pair.Value);
        }
        public static bool Remove(TKey key)
        {
            return _storage.Remove(key);
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
