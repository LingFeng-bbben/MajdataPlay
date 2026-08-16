using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MajdataPlay.Syscall.Darwin
{
    public static class DarwinApi
    {
#if UNITY_IOS
        const string DLL_NAME = "__Internal";
#else
        const string DLL_NAME = "libSystem.B.dylib";
#endif
        [DllImport(DLL_NAME, EntryPoint = "pthread_threadid_np_")]
        public static extern int pthread_threadid_np(IntPtr thread, out ulong threadId);
    }
}
