using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> ToEnumerable(this Transform source)
        {
            for (var i = 0; i < source.childCount; i++)
                yield return source.GetChild(i);
        }
        public static T? GetComponentInChildren<T>(this Transform source, int index)
        {
            if (index >= source.childCount)
                throw new IndexOutOfRangeException("Cannot get child at this object,because the index is out of range");
            return source.GetChild(index).GetComponent<T?>();
        }
        public static Transform[] GetChildren(this Transform parent)
        {
            List<Transform> children = new();
            foreach (Transform child in parent)
            {
                children.Add(child);
                children.AddRange(child.GetChildren());
            }
            return children.ToArray();
        }
    }
}