using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CurlMsg
    {
        public CurlMsgCode Code;
        public IntPtr EasyHandle;
        public CurlMsgData Data;
    }
}
