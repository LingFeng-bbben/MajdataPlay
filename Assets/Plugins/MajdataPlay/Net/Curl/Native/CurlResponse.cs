using MajdataPlay.Net.Curl.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Native
{
    internal class CurlResponse
    {
        public CurlCode ResultCode { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public CurlRequest Request { get; init; }
        public byte[] Body { get; init; }
        public byte[] RawHeaders { get; init; }
    }
}
