using System;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
#nullable enable
namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UIntPtr CurlReadOrWriteCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

    internal class LibCurl
    {
#if UNITY_IOS
        const string DLL_NAME = "__Internal";
#else
        const string DLL_NAME = "libcurl";
#endif
        // libcurl Init flags
        public const long CURL_GLOBAL_SSL = 1 << 0;
        public const long CURL_GLOBAL_WIN32 = 1 << 1;
        public const long CURL_GLOBAL_ALL = CURL_GLOBAL_SSL | CURL_GLOBAL_WIN32;
        public const long CURL_GLOBAL_DEFAULT = CURL_GLOBAL_ALL;

        public const uint CURL_WRITEFUNC_PAUSE = 0x10000001;

        /// <summary>
        /// 暂停接收
        /// </summary>
        public const int CURLPAUSE_RECV = 1 << 0;
        /// <summary>
        /// 恢复接收 (Continue)
        /// </summary>
        public const int CURLPAUSE_RECV_CONT = 0;
        /// <summary>
        /// 恢复所有 (收/发)
        /// </summary>
        public const int CURLPAUSE_CONT = 0;

        public const long CURLSSLOPT_NATIVE_CA = 1 << 4;

        public static class Easy
        {
            [DllImport(DLL_NAME, EntryPoint = "curl_easy_init", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Init();

            [DllImport(DLL_NAME,EntryPoint = "curl_easy_perform", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode Perform(IntPtr handle);
            public static string? GetErrorMessage(CurlCode errornum)
            {
                var ptr = curl_easy_strerror(errornum);

                if (ptr == IntPtr.Zero)
                {
                    return null;
                }
                else
                {
                    return Marshal.PtrToStringUTF8(ptr);
                }
            }
            public static CurlCode Pause(IntPtr handle, int bitmask)
            {
                return curl_easy_pause(handle, bitmask);
            }

            [DllImport(DLL_NAME, EntryPoint = "curl_unity_setopt_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode SetOption(IntPtr handle, CurlOption option, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
            [DllImport(DLL_NAME, EntryPoint = "curl_unity_setopt_long", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode SetOption(IntPtr handle, CurlOption option, long value);
            public static CurlCode SetOption(IntPtr handle, CurlOption option, CurlReadOrWriteCallback value)
            {
                if (option is CurlOption.ReadFunction)
                {
                    return curl_unity_setopt_read_function(handle, value);
                }
                else if (option is CurlOption.HeaderFunction)
                {
                    return curl_unity_setopt_header_function(handle, value);
                }
                else
                {
                    return curl_unity_setopt_write_function(handle, value);
                }
            }
            [DllImport(DLL_NAME, EntryPoint = "curl_unity_setopt_ptr", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode SetOption(IntPtr handle, CurlOption option, IntPtr value);



            [DllImport(DLL_NAME, EntryPoint = "curl_unity_getinfo_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode GetInfo(IntPtr curl, CurlInfo info, out double value);

            [DllImport(DLL_NAME, EntryPoint = "curl_unity_getinfo_long", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode GetInfo(IntPtr curl, CurlInfo info, out long value);
            [DllImport(DLL_NAME, EntryPoint = "curl_unity_getinfo_ptr", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlCode GetInfo(IntPtr curl, CurlInfo info, out IntPtr value);
            public static CurlCode GetInfo(IntPtr curl, CurlInfo info, out string? value)
            {
                var strPtr = IntPtr.Zero;
                var result = GetInfo(curl, info, out strPtr);
                if (strPtr != IntPtr.Zero)
                {
                    value = Marshal.PtrToStringUTF8(strPtr);
                }
                else
                {
                    value = default;
                }
                return result;
            }


            [DllImport(DLL_NAME, EntryPoint = "curl_easy_cleanup", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CleanUp(IntPtr handle);



            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            static extern IntPtr curl_easy_strerror(CurlCode errornum);
            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            static extern CurlCode curl_easy_pause(IntPtr handle, int bitmask);
            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            static extern CurlCode curl_unity_setopt_read_function(IntPtr handle, CurlReadOrWriteCallback value);
            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            static extern CurlCode curl_unity_setopt_write_function(IntPtr handle, CurlReadOrWriteCallback value);
            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            static extern CurlCode curl_unity_setopt_header_function(IntPtr handle, CurlReadOrWriteCallback value);
        }

        public static class Multi
        {
            [DllImport(DLL_NAME, EntryPoint = "curl_multi_init", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Init();


            [DllImport(DLL_NAME, EntryPoint = "curl_multi_add_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode AddEasyHandle(IntPtr multiHandle, IntPtr easyHandle);

            [DllImport(DLL_NAME, EntryPoint = "curl_multi_remove_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode RemoveEasyHandle(IntPtr multiHandle, IntPtr easyHandle);

            [DllImport(DLL_NAME, EntryPoint = "curl_multi_perform", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode Perform(IntPtr multiHandle, out int runningHandleCount);

            public static CurlMsg? GetMessage(IntPtr multiHandle, out int remainingMsgCount)
            {
                var msgHandle = curl_multi_info_read(multiHandle, out remainingMsgCount);
                if(msgHandle != IntPtr.Zero)
                {
                    return Marshal.PtrToStructure<CurlMsg>(msgHandle);
                }
                else
                {
                    return null;
                }
            }

            [DllImport(DLL_NAME, EntryPoint = "curl_multi_poll", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode Poll(IntPtr multiHandle, IntPtr extraFds, uint extraNfds, int timeoutMs, out int numFds);


            [DllImport(DLL_NAME, EntryPoint = "curl_multi_wakeup", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode Wakeup(IntPtr multiHandle);


            [DllImport(DLL_NAME, EntryPoint = "curl_unity_multi_setopt_long", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode SetOption(IntPtr multiHandle, CurlMOption option, long value);


            [DllImport(DLL_NAME, EntryPoint = "curl_unity_multi_setopt_ptr", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode SetOption(IntPtr multiHandle, CurlMOption option, IntPtr pointer);


            [DllImport(DLL_NAME, EntryPoint = "curl_multi_cleanup", CallingConvention = CallingConvention.Cdecl)]
            public static extern CurlMCode CleanUp(IntPtr multi_handle);


            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            static extern IntPtr curl_multi_info_read(IntPtr multiHandle, out int remainingMsgCount);
        }



        [DllImport(DLL_NAME, EntryPoint = "curl_global_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode Init(long flags);

        [DllImport(DLL_NAME, EntryPoint = "curl_global_cleanup", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CleanUp();



        [DllImport(DLL_NAME, EntryPoint = "curl_slist_append", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SlistAppend(IntPtr list, [MarshalAs(UnmanagedType.LPUTF8Str)] string str);

        [DllImport(DLL_NAME, EntryPoint = "curl_slist_free_all", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SlistFreeAll(IntPtr list);

        public static unsafe CurlVersionInfo GetVersionInfo(int age)
        {
            var ptr = curl_version_info(age);
            var info = Marshal.PtrToStructure<CurlVersionInfoRawData>(ptr);
            var protocols = Array.Empty<string>();
            var protocolsPtr = info.Protocols;
            var protocolCount = 0;

            while (*protocolsPtr != null)
            {
                protocolsPtr++;
                protocolCount++;
            }
            protocolsPtr = info.Protocols;
            if(protocolCount != 0)
            {
                protocols = new string[protocolCount];
            }
            for (var i = 0; i < protocolCount; i++)
            {
                var p = info.Protocols + i;
                var protocol = Marshal.PtrToStringUTF8((IntPtr)(*p));
                protocols[i] = protocol;
            }
            return new()
            {
                Age = info.Age,
                Version = Marshal.PtrToStringUTF8((IntPtr)info.Version),
                VersionNum = info.VersionNum,
                Host = Marshal.PtrToStringUTF8((IntPtr)info.Host),
                Features = info.Features,
                SslVersion = Marshal.PtrToStringUTF8((IntPtr)info.SslVersion),
                SslVersionNum = info.SslVersionNum,
                LibzVersion = Marshal.PtrToStringUTF8((IntPtr)info.LibzVersion),
                Protocols = protocols
            };
        }

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr curl_version_info(int age);


        [StructLayout(LayoutKind.Sequential)]
        unsafe struct CurlVersionInfoRawData
        {
            public int Age;

            public byte* Version;

            public uint VersionNum;

            public byte* Host;

            public int Features;

            public byte* SslVersion;

            public long SslVersionNum;

            public byte* LibzVersion;

            public byte** Protocols;
        }
    }
}
