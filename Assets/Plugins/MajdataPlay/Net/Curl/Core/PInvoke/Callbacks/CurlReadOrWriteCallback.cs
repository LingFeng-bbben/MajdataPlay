using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UIntPtr CurlReadOrWriteCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata);
}
