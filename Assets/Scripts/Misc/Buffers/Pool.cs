using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.IL2CPP.CompilerServices;

namespace MajdataPlay.Buffers;
internal static class Pool<T>
{
    const int MAX_ARRAY_LENGTH = 2 * 1024 * 1024; // 2MB
    const int MAX_ARRAY_PER_BUCKET = 2048;

    public readonly static ArrayPool<T> ArrayPool = ArrayPool<T>.Create(MAX_ARRAY_LENGTH, MAX_ARRAY_PER_BUCKET);
    public readonly static MemoryPool<T> MemoryPool = MemoryPool<T>.Shared;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] RentArray(int length, bool clearArray = false)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"Length must be greater than 0");
        }
        else if(length > MAX_ARRAY_LENGTH)
        {
            return new T[length];
        }
        var array = ArrayPool.Rent(length);
        if(clearArray)
        {
            Array.Clear(array, 0, array.Length);
        }
        return array;
    }
    [Il2CppSetOption(Option.NullChecks, false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMemoryOwner<T> RentMemory(int length)
    {
        if (length <= 0 || length > MemoryPool.MaxBufferSize)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"Length must be between 1 and {MemoryPool.MaxBufferSize}.");
        }
        return MemoryPool.Rent(length);
    }
    [Il2CppSetOption(Option.NullChecks, false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReturnArray(T[] array, bool clearArray = false)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");
        }

        ArrayPool.Return(array, clearArray);
    }
}
