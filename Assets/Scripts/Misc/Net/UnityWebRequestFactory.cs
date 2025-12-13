using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MajdataPlay.Net;
internal static class UnityWebRequestFactory
{
    #region Post
    public static UnityWebRequest Post(string uri, string postData, string contentType)
    {
        var req = UnityWebRequest.Post(uri, postData, contentType);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    public static UnityWebRequest Post(string uri, WWWForm formData)
    {
        var req = UnityWebRequest.Post(uri, formData);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    public static UnityWebRequest Post(Uri uri, string postData, string contentType)
    {
        var req = UnityWebRequest.Post(uri, postData, contentType);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    public static UnityWebRequest Post(Uri uri, WWWForm formData)
    {
        var req = UnityWebRequest.Post(uri, formData);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);
        
        return req;
    }
    #endregion
    #region Get
    public static UnityWebRequest Get(string uri)
    {
        var req = UnityWebRequest.Get(uri);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    public static UnityWebRequest Get(Uri uri)
    {
        var req = UnityWebRequest.Get(uri);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    #endregion
    #region Head
    public static UnityWebRequest Head(string uri)
    {
        var req = UnityWebRequest.Head(uri);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    public static UnityWebRequest Head(Uri uri)
    {
        var req = UnityWebRequest.Head(uri);
        req.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
        req.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);

        return req;
    }
    #endregion
}
