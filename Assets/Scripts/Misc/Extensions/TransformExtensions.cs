using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Extensions
{
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
}