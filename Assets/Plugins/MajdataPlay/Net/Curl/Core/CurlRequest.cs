using AOT;
using MajdataPlay.Net.Curl.Core.PInvoke;
using MajdataPlay.Net.Curl.Lifecycle;
using MajdataPlay.Net.Curl.Utils;
using MajdataPlay.UnsafeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Core
{
    internal class CurlRequest
    {
        public Uri RequestUri
        {
            get => Message.RequestUri;
        }
        public HttpRequestMessage Message { get; }
        public HttpContent? Content
        {
            get => Message.Content;
        }
        public HttpMethod Method
        {
            get => Message.Method;
        }

        int _applyFlag = 0;

        Stream? _contentStream;

        GCHandle _handle;
        readonly static CurlReadOrWriteCallback _onReadCallback;  // Upload
        readonly static CurlSeekCallback _onSeekCallback;

        static CurlRequest()
        {
            _onReadCallback = OnReadCallback;
            _onSeekCallback = OnSeekCallback;
        }
        public CurlRequest(HttpRequestMessage httpRequest, Stream? contentStream)
        {
            if(httpRequest is null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }
            Message = httpRequest;
            _contentStream = contentStream;    
        }
        ~CurlRequest()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }
        public void ApplyTo(CurlEasy curlEasy)
        {
            if (Interlocked.CompareExchange(ref _applyFlag, 1 , 0) == 1)
            {
                throw new InvalidOperationException("The curl options have already been applied and cannot be applied again.");
            }
            curlEasy.SetOption(CurlOption.Url, RequestUri.OriginalString);
            curlEasy.SetOption(CurlOption.CustomRequest, Method.Method);
            curlEasy.SetHeaders(Message.Headers.Concat(Message.Content?.Headers?.AsEnumerable() ?? Array.Empty<KeyValuePair<string, IEnumerable<string>>>()));
            curlEasy.SetOption(CurlOption.ReadFunction, Marshal.GetFunctionPointerForDelegate(_onReadCallback));
            curlEasy.SetOption(CurlOption.SeekFunction, Marshal.GetFunctionPointerForDelegate(_onSeekCallback));

            _handle = GCHandle.Alloc(this, GCHandleType.Weak);
            var handlePtr = GCHandle.ToIntPtr(_handle);
            curlEasy.SetOption(CurlOption.ReadData, handlePtr);
            curlEasy.SetOption(CurlOption.SeekData, handlePtr);

            if (_contentStream is not null)
            {
                curlEasy.SetOption(CurlOption.Upload, 1);
                if (_contentStream.CanSeek)
                {
                    curlEasy.SetOption(CurlOption.InFileSizeLarge, _contentStream.Length);
                }
            }
            CurlUtility.ApplySystemCA(curlEasy);
        }

        [MonoPInvokeCallback(typeof(CurlReadOrWriteCallback))]
        static unsafe UIntPtr OnReadCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            if(!UnsafeHelper.TryGetInstanceFromGCHandle<CurlRequest>(userdata, out var curlReq))
            {
                return UIntPtr.Zero;
            }
            var contentStream = curlReq._contentStream;
            if (contentStream is null)
            {
                return UIntPtr.Zero;
            }
            var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
            var buffer = new Span<byte>((void*)ptr, length);

            contentStream.Read(buffer);

            return (UIntPtr)length;
        }
        [MonoPInvokeCallback(typeof(CurlSeekCallback))]
        static unsafe CurlSeekCallbackReturn OnSeekCallback(IntPtr userdata, long offset, SeekOrigin origin)
        {
            if (!UnsafeHelper.TryGetInstanceFromGCHandle<CurlRequest>(userdata, out var curlReq))
            {
                return CurlSeekCallbackReturn.Fail;
            }
            var contentStream = curlReq._contentStream;
            if (contentStream is null)
            {
                return CurlSeekCallbackReturn.Fail;
            }
            else if(!contentStream.CanSeek)
            {
                return CurlSeekCallbackReturn.CantSeek;
            }
            try
            {
                contentStream.Seek(offset, origin);
            }
            catch
            {
                return CurlSeekCallbackReturn.Fail;
            }
            return CurlSeekCallbackReturn.Ok;
        }

        public static async ValueTask<CurlRequest> CreateAsync(HttpRequestMessage requestMessage)
        {
            var contentStream = default(Stream?);
            var content = requestMessage.Content;
            if(content is not null)
            {
                contentStream = await content.ReadAsStreamAsync();
            }
            return new(requestMessage, contentStream);
        }
    }
}