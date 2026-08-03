using AOT;
using MajdataPlay.Net.Curl.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Core
{
    internal class CurlResponse
    {
        public CurlCode? ResultCode { get; set; }
        public CurlRequest Request { get; }
        public HttpResponseMessage Message { get; }


        readonly CurlResponseStream _responseStream;
        readonly CurlReadOrWriteCallback _onWriteCallback; // Download

        public CurlResponse(CurlRequest request)
        {
            Request = request;
             _responseStream = new CurlResponseStream();
            _onWriteCallback = OnWriteCallback;

            Message = new();
            Message.Content = new StreamContent(_responseStream);
            
            Request.SetOption(CurlOption.WriteFunction, _onWriteCallback);
        }

        public void Complete()
        {
            _responseStream.CompleteWriting();
        }
        public void Abort()
        {
            Abort(new OperationCanceledException("Request was aborted."));
        }
        public void Abort(Exception abortException)
        {
            _responseStream.Abort(abortException);
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
