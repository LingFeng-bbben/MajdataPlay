using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Utils
{
    internal static class BufferHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckBufferLength<T>(in int length, in ReadOnlySpan<T> buffer)
        {
            return length > buffer.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EnsureBufferLength<T>(in int length, ref T[] buffer, float growRatio = 2)
        {
            var arrayPool = ArrayPool<T>.Shared;
            if (length > buffer.Length)
            {
                var newBuffer = arrayPool.Rent((int)(buffer.Length * growRatio));
                Array.Clear(newBuffer, 0, newBuffer.Length);
                var s1 = buffer.AsSpan();
                var s2 = newBuffer.AsSpan();
                s1.CopyTo(s2);
                arrayPool.Return(buffer);
                buffer = newBuffer;
            }
            return buffer.Length;
        }
    }
}
