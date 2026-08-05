using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    internal enum CURLMcode
    {
        CURLM_CALL_MULTI_PERFORM = -1,
        CURLM_OK = 0,
        CURLM_BAD_HANDLE = 1,
        CURLM_BAD_EASY_HANDLE = 2,
        CURLM_OUT_OF_MEMORY = 3,
        CURLM_INTERNAL_ERROR = 4,
        CURLM_BAD_SOCKET = 5,
        CURLM_UNKNOWN_OPTION = 6,
        CURLM_ADDED_ALREADY = 7,
        CURLM_RECURSIVE_API_CALL = 8,
        CURLM_WAKEUP_FAILURE = 9,
        CURLM_BAD_FUNCTION_ARGUMENT = 10,
        CURLM_ABORTED_BY_CALLBACK = 11,
        CURLM_UNRECOVERABLE_POLL = 12
    }
}
