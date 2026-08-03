using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Net.Curl.PInvoke;
#nullable enable
namespace MajdataPlay.Net.Curl.Native
{
    internal class CurlRequest : CurlHandle
    {
        public CurlRequestState State
        {
            get
            {
                return (CurlRequestState)Volatile.Read(ref _state);
            }
        }
        public string RequestUri { get; }
        public HttpContent Content { get; }
        public HttpMethod Method { get; }

        int _state = (int)CurlRequestState.Created;
        IntPtr _headersList = IntPtr.Zero;

        Stream? _contentStream;

        CurlReadOrWriteCallback _onWriteCallback; // Download
        CurlReadOrWriteCallback _onReadCallback;  // Upload

        public CurlRequest(string requestUri, HttpContent content, HttpMethod method)
        {
            ThisHandle = LibCurl.curl_easy_init();
            if (ThisHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize libcurl.");
            }
            RequestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Method = method ?? throw new ArgumentNullException(nameof(method));

            SetOption(CurlOption.CURLOPT_URL, RequestUri);
            SetOption(CurlOption.CURLOPT_CUSTOMREQUEST, Method.Method);

            _onReadCallback = OnReadCallback;
            _onWriteCallback = OnWriteCallback;
        }
        ~CurlRequest()
        {
            Dispose();
        }

        public bool TryEnterSubmittedState()
        {
            var lastState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Submitted, (int)CurlRequestState.Created);

            return lastState == (int)CurlRequestState.Created;
        }
        public bool TryEnterCompletedState()
        {
            var lastState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Completed, (int)CurlRequestState.Submitted);

            return lastState == (int)CurlRequestState.Submitted;
        }
        public bool TryEnterCancelledState()
        {
            var state = Volatile.Read(ref _state);

            switch ((CurlRequestState)state)
            {
                case CurlRequestState.Created:
                case CurlRequestState.Submitted:
                    break;

                default:
                    return false;
            }

            var oldState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Cancelled, state);

            return oldState == state;
        }
        public async Task ReadyToSubmitAsync()
        {
            var isUpload = Method == HttpMethod.Post || Method == HttpMethod.Put || Method == HttpMethod.Patch;
            _contentStream = await Content.ReadAsStreamAsync();
            if (isUpload)
            {
                SetOption(CurlOption.CURLOPT_UPLOAD, 1);
                if (_contentStream.CanSeek)
                {
                    SetOption(CurlOption.CURLOPT_INFILESIZE_LARGE, _contentStream.Length);
                }
                SetOption(CurlOption.CURLOPT_READFUNCTION, _onReadCallback);
            }
            else
            {
                SetOption(CurlOption.CURLOPT_WRITEFUNCTION, _onWriteCallback);
            }
        }
        public void ReadyToComplete()
        {
            if (_contentStream != null)
            {
                _contentStream.Dispose();
                _contentStream = null;
            }
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
        
        public void SetHeaders(HttpHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            if (_headersList != IntPtr.Zero)
            {
                LibCurl.curl_slist_free_all(_headersList);
                _headersList = IntPtr.Zero;
            }

            foreach (var header in headers)
            {
                var headerString = $"{header.Key}: {string.Join(", ", header.Value)}";
                _headersList = LibCurl.curl_slist_append(_headersList, headerString);
            }
            SetOption(CurlOption.CURLOPT_HTTPHEADER, _headersList);
        }

        unsafe UIntPtr OnWriteCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            if(_contentStream is null)
            {
                return UIntPtr.Zero;
            }
            var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
            var buffer = new ReadOnlySpan<byte>((void*)ptr, length);

            _contentStream.Write(buffer);

            return (UIntPtr)length;
        }
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
                GC.SuppressFinalize(this);
            }
        }
    }
}