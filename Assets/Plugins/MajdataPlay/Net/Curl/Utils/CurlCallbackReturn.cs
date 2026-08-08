using MajdataPlay.Net.Curl.Core.PInvoke;
using System;

namespace MajdataPlay.Net.Curl.Utils
{
    public static class CurlCallbackReturn
    {
        public static class Read
        {
            /// <summary>
            /// This is a return code for the read callback that, when returned, signals libcurl to pause sending data on the current transfer.
            /// </summary>
            public readonly static UIntPtr Pause = (UIntPtr)LibCurl.CURL_READFUNC_PAUSE;

            /// <summary>
            /// This is a return code for the read callback that, when returned, signals libcurl to immediately abort the current transfer.
            /// </summary>
            public readonly static UIntPtr Abort = (UIntPtr)LibCurl.CURL_READFUNC_ABORT;
        }
        public static class Write
        {
            /// <summary>
            /// This is a magic return code for the write callback that, when returned, signals libcurl to pause receiving on the current transfer.
            /// </summary>
            public readonly static UIntPtr Pause = (UIntPtr)LibCurl.CURL_WRITEFUNC_PAUSE;

            /// <summary>
            /// This is a magic return code for the write callback that, when returned, signals an error from the callback.
            /// </summary>
            public readonly static UIntPtr Error = (UIntPtr)LibCurl.CURL_WRITEFUNC_ERROR;
        }
    }
}
