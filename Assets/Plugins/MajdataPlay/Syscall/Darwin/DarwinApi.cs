using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MajdataPlay.Syscall.Darwin
{
    public static class DarwinApi
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string DLL_NAME = "libSystem.B.dylib";
#else
        const string DLL_NAME = "__Internal";
#endif
        [DllImport(DLL_NAME)]
        public static extern int pthread_threadid_np(IntPtr thread, out ulong threadId);
    }
}
