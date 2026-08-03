using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core
{
    internal enum CurlRequestState
    {
        Created,
        Submitted,
        HeaderRead,
        Completed,
        Cancelled,
        Faulted,
    }
}
