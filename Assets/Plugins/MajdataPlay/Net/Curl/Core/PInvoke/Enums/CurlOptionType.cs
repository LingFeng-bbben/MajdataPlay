using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    internal enum CurlOptionType
    {
        /// <summary>
        /// long may be 32 or 64 bits in C, but we should never depend on anything else but 32
        /// </summary>
        CLong = 0,
        ObjectPointer = 10000,
        FunctionPointer = 20000,
        Off_T = 30000,
        Blob = 40000
    }
}
