using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    [Flags]
    internal enum CurlAuth : long
    {
        Basic = 1 << 0,
        Digest = 1 << 1,
        GSSNEGOTIATE = 1 << 2,
        NTLM = 1 << 3,
        Any = ~0L
    }
}
