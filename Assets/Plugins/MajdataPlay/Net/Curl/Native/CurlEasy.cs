using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MajdataPlay.Net.Curl.PInvoke;

namespace MajdataPlay.Net.Curl.Native
{
    internal class CurlEasy : IDisposable
    {
        IntPtr _handle;

        public CurlEasy()
        {
            _handle = LibCurl.curl_easy_init();
            if (_handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize libcurl.");
            }
        }
        ~CurlEasy()
        {
            Dispose();
        }

        public void SetOption(CurlOption option, string value)
        {
            var result = LibCurl.curl_easy_setopt(_handle, option, value);
            CheckResult(result);
        }
        public void SetOption(CurlOption option, long value)
        {
            var result = LibCurl.curl_easy_setopt(_handle, option, value);
            CheckResult(result);
        }
        public void SetOption(CurlOption option, LibCurl.CurlWriteCallback callback)
        {
            var result = LibCurl.curl_easy_setopt(_handle, option, callback);
            CheckResult(result);
        }
        public void SetOption(CurlOption option, IntPtr value)
        {
            var result = LibCurl.curl_easy_setopt(_handle, option, value);
            CheckResult(result);
        }
        public void Perform()
        {
            var result = LibCurl.curl_easy_perform(_handle);
            CheckResult(result);
        }
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                LibCurl.curl_easy_cleanup(handle);
                GC.SuppressFinalize(this);
            }
        }

        void CheckResult(CurlCode result)
        {
            if (result != CurlCode.CURLE_OK)
            {
                IntPtr errorPtr = LibCurl.curl_easy_strerror(result);
                string errorMessage = Marshal.PtrToStringAnsi(errorPtr) ?? "Unknown error";
                throw new InvalidOperationException($"libcurl error: {errorMessage}");
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        void ThrowIfDisposed()
        {
            if (_handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(CurlEasy));
            }
        }
    }
}