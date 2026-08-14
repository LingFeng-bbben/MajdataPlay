using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MajdataPlay.Syscall.Linux
{
    public static class Bionic
    {
        [DllImport("libc", SetLastError = true)]
        public static extern int gettid();
    }
}
