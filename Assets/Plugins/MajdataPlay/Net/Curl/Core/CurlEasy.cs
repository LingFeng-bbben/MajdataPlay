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
        public void Pause(CurlPauseAction action)
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.Pause(ThisHandle, action);
        }
        public CurlCode Perform()
        {
            ThrowIfDisposed();
            var result = LibCurl.Easy.Perform(ThisHandle);
            CheckCurlCode(result);
            return result;
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
                LibCurl.Easy.CleanUp(handle);
                LibCurlLifecycle.Release();
                GC.SuppressFinalize(this);
            }
        }
    }
}
