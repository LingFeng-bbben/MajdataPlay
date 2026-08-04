using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.UnsafeKit;
public static unsafe class UnsafeHelper
{
    public static T* Alloc<T>(long size) where T : unmanaged
    {
        return (T*)Marshal.AllocHGlobal((IntPtr)(size * (long)sizeof(T)));
    }
    public static IntPtr Alloc(IntPtr size)
    {
        return Marshal.AllocHGlobal(size);
    }
    public static void Free<T>(T* ptr) where T : unmanaged
    {
        Marshal.FreeHGlobal((IntPtr)ptr);
    }
    public static void Free(IntPtr ptr)
    {
        Marshal.FreeHGlobal(ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* AddByteOffset<T>(T* ptr, long offset) where T : unmanaged
    {
        var size = sizeof(T);
        return ptr + size * offset;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* AddOffset<T>(T* ptr, long elementOffset) where T : unmanaged
    {
        return ptr + elementOffset;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetElement<T>(T* ptr, long elementOffset) where T : unmanaged
    {
        ptr = AddOffset(ptr, elementOffset);

        return ref *ptr;
    }
}
