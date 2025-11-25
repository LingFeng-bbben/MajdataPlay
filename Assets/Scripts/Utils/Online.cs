using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using MajdataPlay.Unsafe;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
#nullable enable
namespace MajdataPlay.Utils
{
    internal static class Online
    {
        readonly static HttpClient _client = MajEnv.SharedHttpClient;
        readonly static JsonSerializer DEFAULT_JSON_SERIALIZER = JsonSerializer.Create(DEFAULT_JSON_SERIALIZER_SETTINGS);
        readonly static JsonSerializerSettings DEFAULT_JSON_SERIALIZER_SETTINGS = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public const string API_GET_USER_INFO = "account/info";
        public const string API_GET_MAICHART_LIST = "maichart/list";
        public const string API_GET_MAICHART_INTERACT = "maichart/{0}/interact";
        public const string API_GET_MAICHART_SCORE = "maichart/{0}/score";

        public const string API_POST_USER_LOGIN = "account/login";
        public const string API_POST_MAICHART_INTERACT = "maichart/{0}/interact";
        public const string API_POST_MAICHART_SCORE = "maichart/{0}/score";

        public static async UniTask<bool> CheckLoginAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                using var req = UnityWebRequestFactory.Get(apiEndpoint.Url.Combine(API_GET_USER_INFO));
                var asyncOperation = req.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        req.Abort();
                        throw new HttpException(req.url, HttpErrorCode.Canceled);
                    }
                    await UniTask.Yield();
                }
                if(req.IsSuccessStatusCode())
                {
                    return true;
                }
                return false;
#else
                await UniTask.SwitchToThreadPool();
                var rsp = await _client.GetAsync(apiEndpoint.Url.Combine(API_GET_USER_INFO), token);
                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return false;
                }
                else
                {
                    return true;
                }
#endif
            }
        }
        public static UniTask LoginAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            return LoginAsync(apiEndpoint, apiEndpoint?.Username ?? string.Empty, apiEndpoint?.Password ?? string.Empty, token);
        }
        public static async UniTask LoginAsync(ApiEndpoint apiEndpoint, string username, string password, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                if (apiEndpoint == null)
                {
                    throw new ArgumentNullException(nameof(apiEndpoint));
                }
                if (username == "YourUsername" || password == "YourUsername")
                {
                    throw new Exception("Username or Password is unset");
                }
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("Username or Password is null");
                }
                if (await CheckLoginAsync(apiEndpoint))
                {
                    return;
                }
                var pwdHashStr = HashHelper.ToHexString(await HashHelper.ComputeHashAsync(Encoding.UTF8.GetBytes(password)));
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var form = new WWWForm();
                form.AddField("username", username);
                form.AddField("password", pwdHashStr.Replace("-", "").ToLower());
                using var req = UnityWebRequestFactory.Post(apiEndpoint.Url.Combine(API_POST_USER_LOGIN), form);
                var asyncOperation = req.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        req.Abort();
                        throw new HttpException(req.url, HttpErrorCode.Canceled);
                    }
                    await UniTask.Yield();
                }
                if(!req.IsSuccessStatusCode())
                {
                    throw new Exception("Login failed");
                }
#else
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(pwdHashStr.Replace("-", "").ToLower()), "password" }
                };

                var rsp = await _client.PostAsync(apiEndpoint.Url.Combine(API_POST_USER_LOGIN), formData, token);
                if (rsp.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Login failed");
                }
#endif
            }
        }
        public static async UniTask SendLikeAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await LoginAsync(song.ServerInfo, token);
                var interactUrl = BuildMaiChartUri(API_POST_MAICHART_INTERACT, song.Id);

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Get(interactUrl);
                var asyncOperation = getReq.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        getReq.Abort();
                        throw new HttpException(getReq.url, HttpErrorCode.Canceled);
                    }
                    await UniTask.Yield();
                }
                if(!getReq.IsSuccessStatusCode())
                {
                    throw new Exception("THUMBUP_FAILED".i18n());
                }
                var intlist = await Serializer.Json.DeserializeAsync<MajNetSongInteract>(getReq.downloadHandler.text, DEFAULT_JSON_SERIALIZER_SETTINGS);
                if (intlist.IsLiked)
                {
                    throw new Exception("THUMBUP_ALREADY".i18n());
                }

                var form = new WWWForm();
                form.AddField("type", "like");
                form.AddField("content", "...");

                var postReq = UnityWebRequestFactory.Post(interactUrl, form);
                var postAsyncOperation = postReq.SendWebRequest();

                while (!postAsyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        postReq.Abort();
                        throw new HttpException(postReq.url, HttpErrorCode.Canceled);
                    }
                    await UniTask.Yield();
                }
                if(!postReq.IsSuccessStatusCode())
                {
                    throw new Exception("THUMBUP_FAILED".i18n());
                }
#else
                var intStream = await _client.GetStreamAsync(interactUrl);
                var intlist = await Serializer.Json.DeserializeAsync<MajNetSongInteract>(intStream, DEFAULT_JSON_SERIALIZER);

                if (intlist.IsLiked)
                {
                    throw new Exception("THUMBUP_ALREADY".i18n());
                }

                var formData = new MultipartFormDataContent
                {
                    { new StringContent("like"), "type" },
                    { new StringContent("..."), "content" },
                };
                var rsp = await _client.PostAsync(interactUrl, formData, token);

                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("THUMBUP_FAILED".i18n());
                }
#endif
            }
        }
        public static async UniTask SendScoreAsync(OnlineSongDetail song, MaiScore score, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await LoginAsync(serverInfo);
                var scoreUrl = BuildMaiChartUri(API_POST_MAICHART_SCORE, song.Id);
                var json = await Serializer.Json.SerializeAsync(score, DEFAULT_JSON_SERIALIZER);

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var postReq = UnityWebRequestFactory.Post(scoreUrl, json, "application/json");
                var postAsyncOperation = postReq.SendWebRequest();
                while (!postAsyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        postReq.Abort();
                        throw new HttpException(postReq.url, HttpErrorCode.Canceled);
                    }
                    await UniTask.Yield();
                }
                if (!postReq.IsSuccessStatusCode())
                {
                    throw new Exception(postReq.downloadHandler.text);
                }
#else
                var rsp = await _client.PostAsync(scoreUrl, new StringContent(json, Encoding.UTF8, "application/json"), token);
                var rspContent = await rsp.Content.ReadAsStringAsync();

                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(rspContent);
                }
#endif
            }
        }
        public static async ValueTask<EndpointResponse> FetchChartListAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var url = apiEndpoint.Url.Combine(API_GET_MAICHART_LIST);
                var client = MajEnv.SharedHttpClient;
                var rspText = string.Empty;

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Get(url);
                try
                {
                    var asyncOperation = getReq.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            getReq.Abort();
                            throw new HttpException(url.OriginalString, HttpErrorCode.Canceled);
                        }
                        await UniTask.Yield();
                    }

                    getReq.EnsureSuccessStatusCode();
                    var nativeBuffer = getReq.downloadHandler.nativeData;
                    var buffer = new byte[nativeBuffer.Length];
                    nativeBuffer.CopyTo(buffer);

                    return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        ErrorCode = default,
                        StatusCode = (HttpStatusCode)getReq.responseCode,
                        Message = ""
                    };
                }
                catch (HttpException httpE)
                {
                    MajDebug.LogException(httpE);
                    return new EndpointResponse(Array.Empty<byte>(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        ErrorCode = httpE.ErrorCode,
                        StatusCode = httpE.StatusCode,
                        Message = httpE.Message
                    };
                }
                catch(Exception e)
                {
                    MajDebug.LogException(e);
                    return new EndpointResponse(Array.Empty<byte>(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        ErrorCode = HttpErrorCode.Unreachable,
                        StatusCode = null,
                        Message = e.ToString()
                    };
                }
#else
                try
                {
                    var rsp = await client.GetAsync(url);
                    if (rsp.StatusCode != HttpStatusCode.OK)
                    {
                        return new EndpointResponse(Array.Empty<byte>(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                        {
                            IsSuccessfully = false,
                            IsDeserializable = false,
                            ErrorCode = HttpErrorCode.Unsuccessful,
                            StatusCode = rsp.StatusCode,
                            Message = ""
                        };
                    }
                    var buffer = await rsp.Content.ReadAsByteArrayAsync();
                    return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = true,
                        IsDeserializable = true,
                        ErrorCode = HttpErrorCode.NoError,
                        StatusCode = rsp.StatusCode,
                        Message = "Ok"
                    };
                }
                catch (OperationCanceledException)
                {
                    var errorCode = HttpErrorCode.Timeout;
                    if (token.IsCancellationRequested)
                    {
                        errorCode = HttpErrorCode.Canceled;
                    }
                    return new EndpointResponse(Array.Empty<byte>(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        ErrorCode = errorCode,
                        StatusCode = null,
                        Message = ""
                    };
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                    return new EndpointResponse(Array.Empty<byte>(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        ErrorCode = HttpErrorCode.Unreachable,
                        StatusCode = null,
                        Message = e.ToString()
                    };
                }
#endif
            }
        }
        static Uri BuildMaiChartUri(string api, string chartId)
        {
            return BuildMaiChartUri(api, Guid.Parse(chartId));
        }
        static Uri BuildMaiChartUri(string api, Guid chartId)
        {
            using var sb = ZString.CreateStringBuilder(true);
            sb.AppendFormat(api, chartId);

            return new Uri(sb.ToString());
        }
    }

}