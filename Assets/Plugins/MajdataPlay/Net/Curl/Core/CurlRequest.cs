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
    internal unsafe class CurlRequest : CurlHandle
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

        IntPtr _headersList = IntPtr.Zero;

        Stream? _contentStream;

        readonly static CurlReadOrWriteCallback _onReadCallback;  // Upload
        readonly static CurlSeekCallback _onSeekCallback;

        static CurlRequest()
        {
            _onReadCallback = OnReadCallback;
            _onSeekCallback = OnSeekCallback;
        }

        public CurlRequest(HttpRequestMessage httpRequest, Stream? contentStream)
        {
            LibCurlLifecycle.Retain();
            ThisHandle = LibCurl.Easy.Init();
            if (ThisHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize libcurl.");
            }
            if(httpRequest is null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }
            Message = httpRequest;

            SetOption(CurlOption.Url, RequestUri.OriginalString);
            SetOption(CurlOption.CustomRequest, Method.Method);
            SetOption(CurlOption.ReadFunction, Marshal.GetFunctionPointerForDelegate(_onReadCallback));
            SetOption(CurlOption.SeekFunction, Marshal.GetFunctionPointerForDelegate(_onSeekCallback));
            SetHeaders(httpRequest.Headers, Content?.Headers);

            if(contentStream is not null)
            {
                _contentStream = contentStream;
                SetOption(CurlOption.Upload, 1);
                if (_contentStream.CanSeek)
                {
                    SetOption(CurlOption.InFileSizeLarge, _contentStream.Length);
                }
            }

            CurlUtility.ApplySystemCA(this);            
        }
        ~CurlRequest()
        {
            Dispose();
        }

        public void SetOption(CurlOption option, string value)
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.SetOption(ThisHandle, option, value);
            CheckCurlCode(result);
        }
        public void SetOption(CurlOption option, long value)
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.SetOption(ThisHandle, option, value);
            CheckCurlCode(result);
        }
        public void SetOption(CurlOption option, IntPtr value)
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.SetOption(ThisHandle, option, value);
            CheckCurlCode(result);
        }
        
        void SetHeaders(HttpHeaders headers, HttpContentHeaders? contentHeaders)
        {
            if (_headersList != IntPtr.Zero)
            {
                LibCurl.SListFreeAll(_headersList);
                _headersList = IntPtr.Zero;
            }

            foreach (var header in headers.Concat(contentHeaders?.AsEnumerable() ?? Array.Empty<KeyValuePair<string, IEnumerable<string>>>()))
            {
                var headerString = $"{header.Key}: {string.Join(", ", header.Value)}";
                _headersList = LibCurl.SListAppend(_headersList, headerString);
            }
            SetOption(CurlOption.HttpHeader, _headersList);
        }

        [MonoPInvokeCallback(typeof(CurlReadOrWriteCallback))]
        static unsafe UIntPtr OnReadCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            if(!UnsafeHelper.TryGetInstanceFromGCHandle<CurlTask>(userdata, out var curlTask))
            {
                return UIntPtr.Zero;
            }
            var curlReq = curlTask.Request;
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

        static unsafe CurlSeekCallbackReturn OnSeekCallback(IntPtr userdata, long offset, SeekOrigin origin)
        {
            if (!UnsafeHelper.TryGetInstanceFromGCHandle<CurlTask>(userdata, out var curlTask))
            {
                return CurlSeekCallbackReturn.Fail;
            }
            var curlReq = curlTask.Request;
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

        public override void Dispose()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                LibCurl.Easy.CleanUp(handle);
                LibCurlLifecycle.Release();
                GC.SuppressFinalize(this);
            }
        }
    }
}