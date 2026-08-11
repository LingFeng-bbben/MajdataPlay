using MajdataPlay.Net.Curl.Core.PInvoke;
using MajdataPlay.Net.Curl.Lifecycle;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core
{
    public class CurlEasy : CurlHandle
    {
        IntPtr _headersList = IntPtr.Zero;
        public CurlEasy()
        {
            LibCurlLifecycle.Retain();
            ThisHandle = LibCurl.Easy.Init();
            if (ThisHandle == IntPtr.Zero)
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
        public CurlCode Pause(CurlPauseAction action)
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.Pause(ThisHandle, action);

            return result;
        }
        public CurlCode Perform()
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.Perform(ThisHandle);

            return result;
        }
        public void SetHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            ThrowIfDisposed();
            ClearSList();
            foreach (var header in headers)
            {
                var headerString = $"{header.Key}: {string.Join(", ", header.Value)}";
                AppendSList(headerString);
            }
            SetOption(CurlOption.HttpHeader, _headersList);
        }
        public void AppendSList(string text)
        {
            ThrowIfDisposed();
            _headersList = LibCurl.SListAppend(_headersList, text);
        }
        public void ClearSList()
        {
            ThrowIfDisposed();
            if (_headersList != IntPtr.Zero)
            {
                LibCurl.SListFreeAll(_headersList);
                _headersList = IntPtr.Zero;
            }
        }

        public bool TrySetOption(CurlOption option, string value, [NotNullWhen(true)] out CurlCode? result)
        {
            if(!IsAllocated)
            {
                result = default;
                return false;
            }
            result = LibCurl.Easy.SetOption(ThisHandle, option, value);
            return result == CurlCode.Ok;
        }
        public bool TrySetOption(CurlOption option, long value, [NotNullWhen(true)] out CurlCode? result)
        {
            if (!IsAllocated)
            {
                result = default;
                return false;
            }
            result = LibCurl.Easy.SetOption(ThisHandle, option, value);
            return result == CurlCode.Ok;
        }
        public bool TrySetOption(CurlOption option, IntPtr value, [NotNullWhen(true)] out CurlCode? result)
        {
            if (!IsAllocated)
            {
                result = default;
                return false;
            }
            result = LibCurl.Easy.SetOption(ThisHandle, option, value);
            return result == CurlCode.Ok;
        }
        public bool TryPause(CurlPauseAction action, [NotNullWhen(true)] out CurlCode? result)
        {
            if (!IsAllocated)
            {
                result = default;
                return false;
            }
            result = LibCurl.Easy.Pause(ThisHandle, action);
            return result == CurlCode.Ok;
        }
        public bool TryPerform([NotNullWhen(true)] out CurlCode? result)
        {
            if (!IsAllocated)
            {
                result = default;
                return false;
            }
            result = LibCurl.Easy.Perform(ThisHandle);
            return result == CurlCode.Ok;
        }


        public override void Dispose()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                if (_headersList != IntPtr.Zero)
                {
                    LibCurl.SListFreeAll(_headersList);
                    _headersList = IntPtr.Zero;
                }
                LibCurl.Easy.CleanUp(handle);
                LibCurlLifecycle.Release();
                GC.SuppressFinalize(this);
            }
        }
    }
}
