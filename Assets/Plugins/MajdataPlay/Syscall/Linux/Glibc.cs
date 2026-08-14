using System;
using System.Runtime.InteropServices;

namespace MajdataPlay.Syscall.Linux
{
    public static class Glibc
    {
        [DllImport("libc", SetLastError = true)]
        public static extern int gettid();
    }
}
