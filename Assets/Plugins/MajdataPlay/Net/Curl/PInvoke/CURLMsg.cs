using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MajdataPlay.Net.Curl.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CURLMsg
    {
        public CURLMSG msg;
        public IntPtr easy_handle;
        public IntPtr data;
    }
}
