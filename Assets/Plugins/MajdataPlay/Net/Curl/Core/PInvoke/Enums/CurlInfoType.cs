using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    internal enum CurlInfoType
    {
        String = 0x100000,
        Long = 0x200000,
        Double = 0x300000,
        SList = 0x400000,
        Socket = 0x500000,
        OffT = 0x600000
    }
}
