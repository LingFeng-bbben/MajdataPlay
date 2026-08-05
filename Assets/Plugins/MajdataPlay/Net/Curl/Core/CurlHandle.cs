using MajdataPlay.Net.Curl.Core.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core
{
    internal abstract class CurlHandle : IDisposable
    {
        internal IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return ThisHandle;
            }

        }

        protected IntPtr ThisHandle;

        public abstract void Dispose();

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void CheckCurlCode(CurlCode result)
        {
            if (result != CurlCode.Ok)
            {
                IntPtr errorPtr = LibCurl.curl_easy_strerror(result);
                string errorMessage = Marshal.PtrToStringAnsi(errorPtr) ?? "Unknown error";
                throw new InvalidOperationException($"libcurl error: {errorMessage}");
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void ThrowIfDisposed()
        {
            if (ThisHandle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
        
    }
}
