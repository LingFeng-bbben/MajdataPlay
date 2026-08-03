using MajdataPlay.Net.Curl.PInvoke;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Utils
{
    internal static class CurlUtility
    {
        public static void EnsureSuccess(IntPtr easyHandle, CurlCode code)
        {
            var ex = GetEasyException(easyHandle, code);
            if (ex is not null)
            {
                throw ex;
            }
        }
        public static CurlException? GetEasyException(IntPtr easyHandle, CurlCode code)
        {
            if (code == CurlCode.Ok)
            {
                return default;
            }

            var curlMsg = GetCurlErrorMessage(code);
            var osErrno = 0L;
            var sysMsg = string.Empty;

            if (easyHandle != IntPtr.Zero && LibCurl.curl_easy_getinfo(easyHandle, CurlInfo.OsErrno, out osErrno) == CurlCode.Ok)
            {
                if (osErrno != 0)
                {
                    sysMsg = new Win32Exception((int)osErrno).Message;
                }
            }
            var exMsg = $"Curl Error {(int)code} ({code}): {curlMsg}";
            if (osErrno != 0)
            {
                exMsg += $"\n  └─ OS Error {osErrno}: {sysMsg}";
            }

            return new CurlException(code, exMsg);
        }

        static string GetCurlErrorMessage(CurlCode code)
        {
            var ptr = LibCurl.curl_easy_strerror(code);

            return ptr != IntPtr.Zero ? Marshal.PtrToStringUTF8(ptr) : "Unknown curl error";
        }
    }
}
