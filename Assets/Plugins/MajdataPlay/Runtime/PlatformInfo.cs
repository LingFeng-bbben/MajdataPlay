using System;
using System.Collections.Generic;
using System.Text;
using MajdataPlay.Syscall.Win32;
using MajdataPlay.Syscall.Linux;
using MajdataPlay.Syscall.Darwin;

namespace MajdataPlay.Runtime
{
    public static class PlatformInfo
    {
        public static ulong GetCurrentOSThreadId()
        {
#if UNITY_STANDALONE_WIN
            return Win32API.GetCurrentThreadId();
#elif UNITY_STANDALONE_LINUX
            return (ulong)Glibc.gettid();
#elif UNITY_ANDROID
            return (ulong)Bionic.gettid();
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            DarwinApi.pthread_threadid_np(IntPtr.Zero, out var tid);
            return tid;
#endif
        }
    }
}
