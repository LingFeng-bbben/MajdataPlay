using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Buffers;
internal static class Pool<T>
{
    const int MAX_ARRAY_LENGTH = 256 * 1024 * 1024; // 256MB
    const int MAX_ARRAY_PER_BUCKET = 100;

    public readonly static ArrayPool<T> ArrayPool = ArrayPool<T>.Create(MAX_ARRAY_LENGTH, MAX_ARRAY_PER_BUCKET);
    public readonly static MemoryPool<T> MemoryPool = MemoryPool<T>.Shared;

    public static T[] RentArray(int length, bool clearArray = false)
    {
        if (length <= 0 || length > MAX_ARRAY_LENGTH)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"Length must be between 1 and {MAX_ARRAY_LENGTH}.");
        }
        var array = ArrayPool.Rent(length);
        if(clearArray)
        {
            Array.Clear(array, 0, array.Length);
        }
        return array;
    }
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

        ArrayPool.Return(array, clearArray);
    }
}
