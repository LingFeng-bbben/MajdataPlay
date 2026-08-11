using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    internal enum CurlMCode
    {
        CallMultiPerform = -1,
        Ok = 0,
        BadHandle = 1,
        BadEasyHandle = 2,
        OutOfMemory = 3,
        InternalError = 4,
        BadSocket = 5,
        UnknownOption = 6,
        AddedAlready = 7,
        RecursiveApiCall = 8,
        WakeUpFailure = 9,
        BadFunctionArgument = 10,
        AbortedByCallback = 11,
        UnrecoverablePoll = 12
    }
}
