using MajdataPlay.Buffers.Pool;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MajdataPlay.Buffers
{
    public static class Pool<T>
    {
        const int MAX_ARRAY_LENGTH = 512 * 1024; // 0.5MB
        const int MAX_ARRAY_PER_BUCKET = 128;

        const int BYTE_MAX_ARRAY_LENGTH = 1 * 1024 * 1024; // 1MB
        const int BYTE_MAX_ARRAY_PER_BUCKET = 1024;

        internal readonly static SharedArrayPool<byte> ByteArrayPool = new(16, BYTE_MAX_ARRAY_LENGTH, BYTE_MAX_ARRAY_PER_BUCKET);
        internal readonly static SharedArrayPool<T> ArrayPool = new(8, MAX_ARRAY_LENGTH, MAX_ARRAY_PER_BUCKET);
        internal readonly static MemoryPool<T> MemoryPool = MemoryPool<T>.Shared;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RentArray(int length, bool clearArray = false)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Length must be greater than 0");
            }
            
            var array = GetCurrentArrayPool().Rent(length);
            if (clearArray)
            {
                Array.Clear(array, 0, array.Length);
            }
            return array;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMemoryOwner<T> RentMemory(int length)
        {
            if (length <= 0 || length > MemoryPool.MaxBufferSize)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Length must be between 1 and {MemoryPool.MaxBufferSize}.");
            }
            return MemoryPool.Rent(length);
        }
        public static void ReturnArray(T[] array, bool clearArray = false)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "Array cannot be null.");
            }

            GetCurrentArrayPool().Return(array, clearArray);
        }
        static ArrayPool<T> GetCurrentArrayPool()
        {
            if (typeof(T) == typeof(byte))
            {
                return Unsafe.As<ArrayPool<T>>(ByteArrayPool);
            }
            return ArrayPool;
        }
    }
}
