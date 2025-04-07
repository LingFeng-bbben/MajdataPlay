﻿using MajdataPlay.Collections;
using System;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static partial class SpanExtensions
    {
        public static bool IsEmpty<T>(this Span<T> source) => source.Length == 0;
        public static T Max<T>(this Span<T> source) where T: IComparable<T>
        {
            if (source.Length == 0)
                throw new InvalidOperationException();
            else if(source.Length == 1)
                return source[0];
            var max = source[0];
            for (int i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value.CompareTo(max) > 0)
                    max = value;
            }
            return max;
        }
        public static T Min<T>(this Span<T> source) where T : IComparable<T>
        {
            if (source.Length == 0)
                throw new InvalidOperationException();
            else if (source.Length == 1)
                return source[0];
            var min = source[0];
            for (int i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value.CompareTo(min) < 0)
                    min = value;
            }
            return min;
        }
        public static SpanWithIndexEnumerable<T> WithIndex<T>(this Span<T> source)
        {
            return new SpanWithIndexEnumerable<T>(source);
        }
        public static T? Find<T>(this Span<T> source,in Predicate<T> matcher)
        {
            foreach (var item in source)
                if (matcher(item))
                    return item;
            return default;
        }
        public static Span<T> FindAll<T>(this Span<T> source,in Predicate<T> matcher)
        {
            Span<T> results = new T[source.Length];
            int index = 0;
            foreach (var item in source)
                if (matcher(item))
                    results[index++] = item;
            return results.Slice(0, index);
        }
        public static int FindIndex<T>(this Span<T> source,in Predicate<T> matcher)
        {
            foreach (var (index, item) in source.WithIndex())
                if (matcher(item))
                    return index;
            return -1;
        }
        public static bool Contains<T>(this Span<T> source, T obj) where T : IEquatable<T>
        {
            foreach(var item in source)
            {
                if (item.Equals(obj))
                    return true;
            }
            return false;
        }
        public static bool Any<T>(this Span<T> source, in Predicate<T> matcher)
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