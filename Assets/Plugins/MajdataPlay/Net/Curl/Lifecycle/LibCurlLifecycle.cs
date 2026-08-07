using MajdataPlay.Diagnostics;
using MajdataPlay.Net.Curl.Core;
using MajdataPlay.Net.Curl.Core.PInvoke;
using MajdataPlay.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Lifecycle
{
    internal static class LibCurlLifecycle
    {
        static int s_refCount = 0;
        static volatile bool s_isGlobalInited = false;
        static readonly object s_initLock = new object();

        static LibCurlLifecycle()
        {
            GCCallbackRegistration.RegisterGCCallback(OnGCCallback);
        }

        public static void Retain()
        {
            Interlocked.Increment(ref s_refCount);


            if (!s_isGlobalInited)
            {
                lock (s_initLock)
                {
                    if (!s_isGlobalInited)
                    {
                        var returnCode = LibCurl.Init(LibCurl.CURL_GLOBAL_DEFAULT);
                        if (returnCode != CurlCode.Ok)
                        {
                            throw new CurlException(returnCode);
                        }
                        var info = LibCurl.GetVersionInfo(0);
                        MajDebug.LogInfo($"""
                                        [libcurl]version info:
                                        Age: {info.Age}
                                        Version: {info.Version}
                                        VersionNum: 0x{info.VersionNum:X6} ({info.VersionNum})
                                        Host: {info.Host}
                                        Features: 0x{info.Features:X8} ({info.Features})
                                        SslVersion: {info.SslVersion}
                                        SslVersionNum: {info.SslVersionNum}
                                        LibzVersion: {info.LibzVersion}
                                        Protocols: {(info.Protocols == null ? "<null>" : string.Join(", ", info.Protocols))}
                                        """);
                        s_isGlobalInited = true;
                    }
                }
            }
        }

        public static void Release()
        {
            Interlocked.Decrement(ref s_refCount);
        }

        static bool OnGCCallback()
        {
            if(!s_isGlobalInited)
            {
                return true;
            }
            try
            {
                lock(s_initLock)
                {
                    if(!s_isGlobalInited || Volatile.Read(ref s_refCount) != 0)
                    {
                        return true;
                    }
                    LibCurl.CleanUp();
                    s_isGlobalInited = false;
                }
            }
            catch (Exception e)
            {

            }
            return true;
        }
    }
}
