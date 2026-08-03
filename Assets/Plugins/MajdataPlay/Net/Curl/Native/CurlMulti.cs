using MajdataPlay.Net.Curl.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Native
{
    internal class CurlMulti : CurlHandle
    {
        public CurlMulti() 
        {
            ThisHandle = LibCurl.curl_multi_init();
        }

        public override void Dispose()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                LibCurl.curl_multi_cleanup(handle);
                GC.SuppressFinalize(this);
            }
        }
    }
}
