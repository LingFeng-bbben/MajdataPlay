using MajdataPlay.Net.Curl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl
{
    public static class ClientUrl
    {
        public static CurlMulti CreateMulti()
        {
            return new CurlMulti();
        }
    }
}
