using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate CurlSeekCallbackReturn CurlSeekCallback(IntPtr userdata, long offset, SeekOrigin origin);

    public enum CurlSeekCallbackReturn : int
    {
        Ok = 0,
        Fail = 1,
        CantSeek = 2
    }
}
