using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.UnsafeKit;
public static unsafe class UnsafeHelper
{
    public static T* Alloc<T>(ulong size) where T : unmanaged
    {
        return (T*)Marshal.AllocHGlobal((IntPtr)(size * (ulong)sizeof(T)));
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
}
