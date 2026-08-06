using AOT;
using MajdataPlay.Net.Curl.Core.PInvoke;
using MajdataPlay.Net.Curl.Lifecycle;
using MajdataPlay.Net.Curl.Utils;
using System;
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
    internal class CurlRequest : CurlHandle
    {
        public Uri RequestUri { get; }
        public HttpContent Content { get; }
        public HttpMethod Method { get; }

        IntPtr _headersList = IntPtr.Zero;

        Stream? _contentStream;

        CurlReadOrWriteCallback _onReadCallback;  // Upload

        readonly HttpRequestMessage _rawRequest;

        public CurlRequest(HttpRequestMessage httpRequest, Stream? contentStream)
        {
            LibCurlLifecycle.Retain();
            ThisHandle = LibCurl.curl_easy_init();
            if (ThisHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize libcurl.");
            }
            if(httpRequest is null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }
            RequestUri = httpRequest.RequestUri ?? throw new ArgumentNullException(nameof(httpRequest.RequestUri));
            Content = httpRequest.Content ?? throw new ArgumentNullException(nameof(httpRequest.Content));
            Method = httpRequest.Method ?? throw new ArgumentNullException(nameof(httpRequest.Method));

            _rawRequest = httpRequest;
            _onReadCallback = OnReadCallback;

            SetOption(CurlOption.Url, RequestUri.OriginalString);
            SetOption(CurlOption.CustomRequest, Method.Method);
            SetOption(CurlOption.ReadFunction, _onReadCallback);
            SetHeaders(httpRequest.Headers, Content.Headers);

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
            var result = LibCurl.curl_easy_setopt(ThisHandle, option, value);
            CheckCurlCode(result);
        }
        public void SetOption(CurlOption option, long value)
        {
            ThrowIfDisposed();
            var result = LibCurl.curl_easy_setopt(ThisHandle, option, value);
            CheckCurlCode(result);
        }
        public void SetOption(CurlOption option, CurlReadOrWriteCallback callback)
        {
            ThrowIfDisposed();
            var result = LibCurl.curl_easy_setopt(ThisHandle, option, callback);
            CheckCurlCode(result);
        }
        public void SetOption(CurlOption option, IntPtr value)
        {
            ThrowIfDisposed();
            var result = LibCurl.curl_easy_setopt(ThisHandle, option, value);
            CheckCurlCode(result);
        }
        
        void SetHeaders(HttpHeaders headers, HttpContentHeaders contentHeaders)
        {
            if (_headersList != IntPtr.Zero)
            {
                LibCurl.curl_slist_free_all(_headersList);
                _headersList = IntPtr.Zero;
            }

            foreach (var header in headers.Concat(contentHeaders))
            {
                var headerString = $"{header.Key}: {string.Join(", ", header.Value)}";
                _headersList = LibCurl.curl_slist_append(_headersList, headerString);
            }
            SetOption(CurlOption.HttpHeader, _headersList);
        }

        [MonoPInvokeCallback(typeof(CurlReadOrWriteCallback))]
        unsafe UIntPtr OnReadCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            if (_contentStream is null)
            {
                return UIntPtr.Zero;
            }
            var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
            var buffer = new Span<byte>((void*)ptr, length);

            _contentStream.Read(buffer);

            return (UIntPtr)length;
        }

        public override void Dispose()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                LibCurl.curl_easy_cleanup(handle);
                LibCurlLifecycle.Release();
                GC.SuppressFinalize(this);
            }
        }
    }
}