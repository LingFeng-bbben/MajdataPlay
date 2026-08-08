using MajdataPlay.Net.Curl.Core.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core
{
    public abstract class CurlHandle : IDisposable
    {
        public bool IsAllocated
        {
            get
            {
                return Volatile.Read(ref ThisHandle) != IntPtr.Zero;
            }
        }
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
                var errorMessage = LibCurl.Easy.GetErrorMessage(result);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Unknown error";
                }
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
