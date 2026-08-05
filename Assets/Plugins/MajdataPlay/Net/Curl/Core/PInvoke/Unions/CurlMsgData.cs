using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct CurlMsgData
    {
        /// <summary>
        /// Placeholder for the actual union data. Replace with actual fields as needed.
        /// </summary>
        [FieldOffset(0)]
        public readonly IntPtr Whatever;
        [FieldOffset(0)]
        public readonly CurlCode Result;
    }
}
