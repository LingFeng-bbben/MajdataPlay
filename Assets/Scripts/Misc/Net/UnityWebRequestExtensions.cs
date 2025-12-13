using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace MajdataPlay.Net;
internal static class UnityWebRequestExtensions
{
    public static UnityWebRequest EnsureSuccessStatusCode(this UnityWebRequest request)
    {
        if(!request.isDone)
        {
            throw new InvalidOperationException("The request is not completed yet.");
        }
        var rspCode = request.responseCode is -1 ? null : (HttpStatusCode?)request.responseCode;
        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                if (request.error == "Request timeout")
                {
                    throw new HttpException(request.url, HttpErrorCode.Timeout, rspCode);
                }
                else
                {
                    throw new HttpException(request.url, HttpErrorCode.Unreachable, rspCode);
                }
            case UnityWebRequest.Result.DataProcessingError:
            case UnityWebRequest.Result.ProtocolError:
                throw new HttpException(request.url, HttpErrorCode.Unsuccessful, rspCode);
            case UnityWebRequest.Result.Success:
                break;
            default:
                if (request.error == "Request timeout")
                {
                    throw new HttpException(request.url, HttpErrorCode.Timeout, rspCode);
                }
                else
                {
                    throw new HttpException(request.url, HttpErrorCode.Unsuccessful, rspCode);
                }
        }
        if (rspCode is not HttpStatusCode.OK)
        {
            throw new HttpException(request.url, HttpErrorCode.Unsuccessful, rspCode);
        }

        return request;
    }
    public static bool IsSuccessStatusCode(this UnityWebRequest request)
    {
        if (!request.isDone)
        {
            return false;
        }
        return request.result == UnityWebRequest.Result.Success && request.responseCode == 200;
    }
}
