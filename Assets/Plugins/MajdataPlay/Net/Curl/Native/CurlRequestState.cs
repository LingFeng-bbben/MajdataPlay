using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Native
{
    internal enum CurlRequestState
    {
        Created,
        Submitted,
        Completed,
        Cancelled,
        Disposed,
    }
}
