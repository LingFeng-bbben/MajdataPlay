using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net;
public enum HttpErrorCode
{
    NoError,
    Unreachable,
    InvalidRequest,
    NotSupported,
    Unsuccessful,
    Timeout,
    Canceled,
    IntegrityCheckFailed
}
