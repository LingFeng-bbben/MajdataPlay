using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MajdataPlay.Buffers
{
    public static class SpanExtensions
    {
        public static unsafe NativeArray<T> AsNativeArray<T>(this Span<T> span) where T : unmanaged
        {
            fixed (void* source = span)
            {
                var data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(source, span.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref data, AtomicSafetyHandle.Create());
#endif
                return data;
            }
        }
    }
}
