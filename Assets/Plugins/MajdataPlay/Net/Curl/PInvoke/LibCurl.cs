using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MajdataPlay.Net.Curl.PInvoke
{
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


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr CurlWriteCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_global_init(long flags);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_global_cleanup();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr curl_easy_init();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_easy_cleanup(IntPtr handle);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_easy_perform(IntPtr handle);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr curl_easy_strerror(CurlCode errornum);

        // curl_easy_setopt 的各个重载 (应对 C 语言的可变参数)

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern CurlCode curl_easy_setopt(IntPtr handle, CurlOption option, string value);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_easy_setopt(IntPtr handle, CurlOption option, long value);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_easy_setopt(IntPtr handle, CurlOption option, CurlWriteCallback value);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_easy_setopt(IntPtr handle, CurlOption option, IntPtr value);

        // 处理自定义 HTTP 头 (curl_slist)
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr curl_slist_append(IntPtr list, string str);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_slist_free_all(IntPtr list);
    }
}
