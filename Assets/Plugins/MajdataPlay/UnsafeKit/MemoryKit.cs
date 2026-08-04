using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

namespace MajdataPlay.UnsafeKit;
public static unsafe class MemoryKit
{
    public static void MemCpy<T>(this ReadOnlySpan<T> src, Span<T> dst) where T : unmanaged
    {
        if (dst.Length < src.Length)
        {
            throw new ArgumentException("Destination is too short", nameof(dst));
        }
        else if (src.IsEmpty)
        {
            return;
        }
        ref var srcRef = ref MemoryMarshal.GetReference(src);
        ref var dstRef = ref MemoryMarshal.GetReference(dst);

        fixed(void* srcPtr = &srcRef)
        {
            fixed (void* dstPtr = &dstRef)
            {
                var offset = (ulong)(((T*)dstPtr - (T*)srcPtr) * sizeof(T));
                var srcLen = (ulong)(src.Length * sizeof(T));
                var dstLen = (ulong)(dst.Length * sizeof(T));

                if (offset != 0 && (offset < srcLen || ulong.MinValue - offset < dstLen))
                {
                    throw new ArgumentException("The src and dest were overlapping but not referring to the same starting location");
                }

                UnsafeUtility.MemCpy(dstPtr, srcPtr, sizeof(T) * src.Length);
            }
        }
    }
    public static void MemSet(this Span<byte> src, byte value)
    {
        if (src.IsEmpty)
        {
            return;
        }
        ref var srcRef = ref MemoryMarshal.GetReference(src);

        fixed (void* srcPtr = &srcRef)
        {
            UnsafeUtility.MemSet(srcPtr, value, sizeof(byte) * src.Length);
        }
    }
    public static void MemClear<T>(this Span<T> src) where T: unmanaged
    {
        if (src.IsEmpty)
        {
            return;
        }
        ref var srcRef = ref MemoryMarshal.GetReference(src);

        fixed (void* srcPtr = &srcRef)
        {
            UnsafeUtility.MemClear(srcPtr, sizeof(T) * src.Length);
        }
    }
}
