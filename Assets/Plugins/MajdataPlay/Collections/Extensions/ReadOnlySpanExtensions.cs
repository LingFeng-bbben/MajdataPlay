using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Collections
{
    public static partial class ReadOnlySpanExtensions
    {
        public static bool IsEmpty<T>(this ReadOnlySpan<T> source) => source.Length == 0;
        public static T Max<T>(this ReadOnlySpan<T> source) where T : IComparable<T>
        {
            if (source.Length == 0)
                throw new InvalidOperationException();
            else if (source.Length == 1)
                return source[0];
            var max = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value.CompareTo(max) > 0)
                    max = value;
            }
            return max;
        }
        public static T Min<T>(this ReadOnlySpan<T> source) where T : IComparable<T>
        {
            if (source.Length == 0)
                throw new InvalidOperationException();
            else if (source.Length == 1)
                return source[0];
            var min = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value.CompareTo(min) < 0)
                    min = value;
            }
            return min;
        }
        public static SpanWithIndexEnumerable<T> WithIndex<T>(this ReadOnlySpan<T> source)
        {
            return new SpanWithIndexEnumerable<T>(source);
        }
        public static T? Find<T>(this ReadOnlySpan<T> source, in Predicate<T> matcher)
        {
            foreach (var item in source)
                if (matcher(item))
                    return item;
            return default;
        }
        public static ReadOnlySpan<T> FindAll<T>(this ReadOnlySpan<T> source, in Predicate<T> matcher)
        {
            Span<T> results = new T[source.Length];
            var index = 0;
            foreach (var item in source)
                if (matcher(item))
                    results[index++] = item;
            return results.Slice(0, index);
        }
        public static int FindIndex<T>(this ReadOnlySpan<T> source, in Predicate<T> matcher)
        {
            foreach (var (index, item) in source.WithIndex())
                if (matcher(item))
                    return index;
            return -1;
        }
        public static bool Contains<T>(this ReadOnlySpan<T> source, T obj) where T : IEquatable<T>
        {
            foreach (var item in source)
            {
                if (item.Equals(obj))
                    return true;
            }
            return false;
        }
        public static bool Any<T>(this ReadOnlySpan<T> source, in Predicate<T> matcher)
        {
            foreach (var item in source)
            {
                if (matcher(item))
                    return true;
            }
            return false;
        }
    }
}
