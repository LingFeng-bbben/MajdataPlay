using AOT;
using MajdataPlay.Net.Curl.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Native
{
    internal class CurlResponse
    {
        public CurlCode ResultCode { get; init; }
        public HttpStatusCode StatusCode
        {
            get
            {
                var @return = LibCurl.curl_easy_getinfo(Request.Handle, CurlInfo.ResponseCode, out long statusCode);
                if (@return != CurlCode.Ok)
                {
                    return default;
                }

                return (HttpStatusCode)statusCode;
            }
        }
        public CurlRequest Request { get; }
        
        public HttpContent Content { get; }

        readonly CurlResponseStream _responseStream;
        readonly CurlReadOrWriteCallback _onWriteCallback; // Download


        public CurlResponse(CurlRequest request)
        {
             Request = request;
             _responseStream = new CurlResponseStream();
            _onWriteCallback = OnWriteCallback;

            Content = new StreamContent(_responseStream);
            Request.SetOption(CurlOption.WriteFunction, _onWriteCallback);
        }

        [MonoPInvokeCallback(typeof(CurlReadOrWriteCallback))]
        unsafe UIntPtr OnWriteCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
            var buffer = new ReadOnlySpan<byte>((void*)ptr, length);

            _responseStream.Write(buffer);

            return (UIntPtr)length;
        }
    }
}
