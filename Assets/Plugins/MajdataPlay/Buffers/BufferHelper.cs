using System;
using System.Runtime.CompilerServices;

namespace MajdataPlay.Buffers
{
    public static class BufferHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckBufferLength<T>(in int length, in ReadOnlySpan<T> buffer)
        {
            return length > buffer.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EnsureBufferLength<T>(in int length, ref T[] buffer, float growRatio = 2)
        {
            if (length > buffer.Length)
            {
                var newBuffer = Pool<T>.RentArray((int)(buffer.Length * growRatio));
                Array.Clear(newBuffer, 0, newBuffer.Length);
                var s1 = buffer.AsSpan();
                var s2 = newBuffer.AsSpan();
                s1.CopyTo(s2);
                Pool<T>.ReturnArray(buffer);
                buffer = newBuffer;
            }
            return buffer.Length;
        }
    }
}
