using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<(int, T)> WithIndex<T>(this IEnumerable<T> source)
        {
            int index = 0;
            foreach (var item in source)
                yield return (index++, item);
        }
        public static T? Find<T>(this IEnumerable<T> source,Predicate<T> matcher)
        {
            foreach(var item in source)
                if(matcher(item))
                    return item;
            return default;
        }
        public static T[] FindAll<T>(this IEnumerable<T> source, Predicate<T> matcher)
        {
            List<T> items = new();
            foreach (var item in source)
                if (matcher(item))
                    items.Add(item);
            return items.ToArray();
        }
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> matcher)
        {
            foreach(var (index,item) in source.WithIndex())
                if (matcher(item))
                    return index;
            return -1;
        }
    }
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> ToEnumerable(this Transform source)
        {
            for(int i = 0; i < source.childCount;i++)
                yield return source.GetChild(i);
        }
        public static IEnumerable<Transform> GetChildren(this Transform source) => source.ToEnumerable();
        public static T? GetComponentInChildren<T>(this Transform source,int index)
        {
            if (index >= source.childCount)
                throw new IndexOutOfRangeException("Cannot get child at this object,because the index is out of range");
            return source.GetChild(index).GetComponent<T?>();
        }
    }
    public static class SensorTypeExtensions
    {
        /// <summary>
        /// Gets a touch panel area with a specified difference from the current touch panel area
        /// </summary>
        /// <param name="source">current touch panel area</param>
        /// <param name="diff">specified difference</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static SensorType GetDiff(this SensorType source,int diff)
        {
            if(source > SensorType.E8)
                throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area");
            diff = diff % 8;
            if (diff == 0)
                return source;

            var isReverse = diff < 0;
            var result = source.GetIndex() + diff;
            var group = source.GetGroup();
            switch(group)
            {
                case SensorGroup.A:
                    if (isReverse)
                        result += 7;
                    return (SensorType)result;
                case SensorGroup.B:
                    if (isReverse)
                        result += 7 + 8;
                    else
                        result += 8;
                    return (SensorType)result;
                case SensorGroup.C:
                    return source;
                case SensorGroup.D:
                    if (isReverse)
                        result += 7 + 17;
                    else
                        result += 17;
                    return (SensorType)result;
                // SensorGroup.E
                default:
                    if (isReverse)
                        result += 7 + 25;
                    else
                        result += 25;
                    return (SensorType)result;
            }
        }
        /// <summary>
        /// Get the group where the touch panel area is located
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static SensorGroup GetGroup(this SensorType source)
        {
            if (source > SensorType.E8)
                throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area");
            var i = (int)source;
            if (i <= 7)
                return SensorGroup.A;
            else if (i <= 15)
                return SensorGroup.B;
            else if (i <= 16)
                return SensorGroup.C;
            else if (i <= 24)
                return SensorGroup.D;
            else
                return SensorGroup.E;
        }
        /// <summary>
        /// Get the index of the touch panel area within the group
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static int GetIndex(this SensorType source)
        {
            var group = source.GetGroup();
            return group switch
            {
                SensorGroup.A => (int)source + 1,
                SensorGroup.B => (int)source - 7,
                SensorGroup.C => 1,
                SensorGroup.D => (int)source - 16,
                SensorGroup.E => (int)source - 24,
                _ => throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area")
            };
        }
        public static SensorType Mirror(this SensorType source,SensorType baseLine)
        {
            if (source == SensorType.C || source.IsCollinearWith(baseLine))
                return source;
            
        }
        public static bool IsCollinearWith(this SensorType source, SensorType target)
        {
            var thisGroup = source.GetGroup();
            var targetGroup = target.GetGroup();
            if (thisGroup is SensorGroup.C || targetGroup is SensorGroup.C)
                return true;

            var thisIndex = source.GetIndex();
            var targetIndex = target.GetIndex();
            
            if (thisGroup is (SensorGroup.A or SensorGroup.B) && targetGroup is (SensorGroup.A or SensorGroup.B))
                return thisIndex == targetIndex || Math.Abs(thisIndex - targetIndex) == 4;
            else if (thisGroup is (SensorGroup.D or SensorGroup.E) && targetGroup is (SensorGroup.D or SensorGroup.E))
                return thisIndex == targetIndex || Math.Abs(thisIndex - targetIndex) == 4;
            else
                return false;
        }
        public static bool IsLeftOf(this SensorType source, SensorType target)
        {
            if (source == SensorType.C || target == SensorType.C)
                throw new InvalidOperationException("cnm");
            else if (source.IsCollinearWith(target))
                return false;

            var opposite = target.GetDiff(4);
            var thisIndex = source.GetIndex();
            var aIndex = target.GetIndex();
            var bIndex = opposite.GetIndex();
            var min = Math.Min(aIndex, bIndex);
            var max = Math.Max(aIndex, bIndex);

            var thisGroup = source.GetGroup();
            var targetGroup = target.GetGroup();

            var AorB = thisGroup is SensorGroup.A or SensorGroup.B && targetGroup is SensorGroup.A or SensorGroup.B;
            var DorE = thisGroup is SensorGroup.D or SensorGroup.E && targetGroup is SensorGroup.D or SensorGroup.E;
            if (AorB || DorE)
            {
                if (thisIndex > min && thisIndex < max)
                    return false;
                else
                    return true;
            }
            else
            {
                if(targetGroup is SensorGroup.A or SensorGroup.B)
                {
                    if (thisIndex > min && thisIndex <= max)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (thisIndex >= min && thisIndex < max)
                        return false;
                    else
                        return true;
                }
            }
        }
        public static bool IsRightOf(this SensorType source, SensorType target)
        {
            if (source == SensorType.C || target == SensorType.C)
                throw new InvalidOperationException("cnm");
            else if (source.IsCollinearWith(target))
                return false;
            else
                return !source.IsLeftOf(target);
        }
    }
}
