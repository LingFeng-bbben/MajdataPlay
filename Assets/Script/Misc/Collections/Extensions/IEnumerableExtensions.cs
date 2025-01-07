using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace MajdataPlay.Collections
{
    public static class IEnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source) => source.Count() == 0;
        public static IEnumerable<(int, T)> WithIndex<T>(this IEnumerable<T> source)
        {
            var index = 0;
            foreach (var item in source)
                yield return (index++, item);
        }
        public static T? Find<T>(this IEnumerable<T> source, in Predicate<T> matcher)
        {
            foreach (var item in source)
                if (matcher(item))
                    return item;
            return default;
        }
        public static T[] FindAll<T>(this IEnumerable<T> source, in Predicate<T> matcher)
        {
            List<T> items = new();
            foreach (var item in source)
                if (matcher(item))
                    items.Add(item);
            return items.ToArray();
        }
        public static int FindIndex<T>(this IEnumerable<T> source, in Predicate<T> matcher)
        {
            foreach (var (index, item) in source.WithIndex())
                if (matcher(item))
                    return index;
            return -1;
        }
    }
}