using System;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class MathExtensions
    {
        public static T Clamp<T>(this T source, in T min, in T max) where T : IComparable<T>
        {
            if (source.CompareTo(min) < 0)
                return min;
            else if (source.CompareTo(max) > 0)
                return max;
            else
                return source;
        }
        /// <summary>
        /// such like [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>if in range return true,else false</returns>
        public static bool InRange<T>(this T source, in T min, in T max) where T : IComparable<T>
        {
            return !(source.CompareTo(min) < 0 || source.CompareTo(max) > 0);
        }
    }
}