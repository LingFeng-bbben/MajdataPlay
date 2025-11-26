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
using UnityEditor.PackageManager;
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
                await UniTask.SwitchToThreadPool();
                var uri = apiEndpoint.Url.Combine(API_GET_USER_INFO);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                var rsp = await GetAsync(uri, token);

                return rsp.IsSuccessfully;
#else
                var rsp = await GetAsync(uri, token);

                return rsp.IsSuccessfully;
#endif
            }
        }
        public static UniTask<EndpointResponse> LoginAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            return LoginAsync(apiEndpoint, apiEndpoint?.Username ?? string.Empty, apiEndpoint?.Password ?? string.Empty, token);
        }
        public static async UniTask<EndpointResponse> LoginAsync(ApiEndpoint apiEndpoint, string username, string password, CancellationToken token = default)
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
                    return new()
                    {
                        IsSuccessfully = true,
                        IsDeserializable = false,
                        StatusCode = HttpStatusCode.OK,
                        ErrorCode = HttpErrorCode.NoError,
                        Message = string.Empty
                    };
                }
                var pwdHashStr = HashHelper.ToHexString(await HashHelper.ComputeHashAsync(Encoding.UTF8.GetBytes(password)));
                var uri = apiEndpoint.Url.Combine(API_POST_USER_LOGIN);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var form = new WWWForm();
                form.AddField("username", username);
                form.AddField("password", pwdHashStr.Replace("-", "").ToLower());
                var rsp = await PostAsync(uri, form, token);

                return rsp;
#else
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(pwdHashStr.Replace("-", "").ToLower()), "password" }
                };

                var rsp = await PostAsync(uri, formData, token);

                return rsp;
#endif
            }
        }
        public static async UniTask<EndpointResponse> SendLikeAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await LoginAsync(song.ServerInfo, token);
                var interactUrl = BuildMaiChartUri(API_POST_MAICHART_INTERACT, song.Id);

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                var rsp = await GetAsync(interactUrl, token);

                if (!rsp.IsDeserializable)
                {
                    return rsp;
                }
                var intlist = await rsp.DeserializeAsync<MajNetSongInteract>();
                if (intlist.IsLiked)
                {
                    return new()
                    {
                        ErrorCode = HttpErrorCode.NoError,
                        StatusCode = rsp.StatusCode,
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        Message = "THUMBUP_ALREADY"
                    };
                }

                var form = new WWWForm();
                form.AddField("type", "like");
                form.AddField("content", "...");

                rsp = await PostAsync(interactUrl, form, token);

                if (!rsp.IsSuccessfully)
                {
                    return new()
                    {
                        ErrorCode = rsp.ErrorCode,
                        StatusCode = rsp.StatusCode,
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        Message = "THUMBUP_FAILED"
                    };
                }
                return rsp;
#else
                var rsp = await GetAsync(interactUrl, token);
                if(!rsp.IsDeserializable)
                {
                    return rsp;
                }
                var intlist = await rsp.DeserializeAsync<MajNetSongInteract>();

                if (intlist.IsLiked)
                {
                    return new()
                    {
                        ErrorCode = HttpErrorCode.NoError,
                        StatusCode = rsp.StatusCode,
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        Message = "THUMBUP_ALREADY"
                    };
                }

                var formData = new MultipartFormDataContent
                {
                    { new StringContent("like"), "type" },
                    { new StringContent("..."), "content" },
                };
                rsp = await PostAsync(interactUrl, formData, token);

                if (!rsp.IsSuccessfully)
                {
                    return new()
                    {
                        ErrorCode = rsp.ErrorCode,
                        StatusCode = rsp.StatusCode,
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        Message = "THUMBUP_FAILED"
                    };
                }
                return rsp;
#endif
            }
        }
        public static async UniTask<EndpointResponse> PostScoreAsync(OnlineSongDetail song, MaiScore score, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await LoginAsync(serverInfo);
                var scoreUrl = BuildMaiChartUri(API_POST_MAICHART_SCORE, song.Id);
                var json = await Serializer.Json.SerializeAsync(score, DEFAULT_JSON_SERIALIZER);

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                return await PostAsync(scoreUrl, json, "application/json", token);
#else
                return await PostAsync(scoreUrl, new StringContent(json, Encoding.UTF8, "application/json"), token);
#endif
            }
        }
        public static async ValueTask<EndpointResponse> FetchChartListAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var url = apiEndpoint.Url.Combine(API_GET_MAICHART_LIST);

                return await GetAsync(url, token);
            }
        }
        
        
        static async ValueTask<EndpointResponse> GetAsync(Uri uri, CancellationToken token = default)
        {
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Get(uri);
                try
                {
                    var asyncOperation = getReq.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            getReq.Abort();
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Canceled);
                        }
                        await UniTask.Yield();
                    }

                    getReq.EnsureSuccessStatusCode();
                    var nativeBuffer = getReq.downloadHandler.nativeData;
                    var buffer = Array.Empty<byte>();
                    if(nativeBuffer.Length != 0)
                    {
                        buffer = new byte[nativeBuffer.Length];
                        nativeBuffer.CopyTo(buffer);
                    }

                    return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = true && buffer.Length != 0,
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
                var client = MajEnv.SharedHttpClient;
                var rsp = await client.GetAsync(uri, token);
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


#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
        static async ValueTask<EndpointResponse> PostAsync(Uri uri, WWWForm form, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Post(uri, form);
                try
                {
                    var asyncOperation = getReq.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            getReq.Abort();
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Canceled);
                        }
                        await UniTask.Yield();
                    }

                    getReq.EnsureSuccessStatusCode();
                    var nativeBuffer = getReq.downloadHandler.nativeData;
                    var buffer = Array.Empty<byte>();
                    if (nativeBuffer.Length != 0)
                    {
                        buffer = new byte[nativeBuffer.Length];
                        nativeBuffer.CopyTo(buffer);
                    }

                    return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = true && buffer.Length != 0,
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
            }
        }
        static async ValueTask<EndpointResponse> PostAsync(Uri uri, string content, string contentType, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Post(uri, content, contentType);
                try
                {
                    var asyncOperation = getReq.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            getReq.Abort();
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Canceled);
                        }
                        await UniTask.Yield();
                    }

                    getReq.EnsureSuccessStatusCode();
                    var nativeBuffer = getReq.downloadHandler.nativeData;
                    var buffer = Array.Empty<byte>();
                    if (nativeBuffer.Length != 0)
                    {
                        buffer = new byte[nativeBuffer.Length];
                        nativeBuffer.CopyTo(buffer);
                    }

                    return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = true && buffer.Length != 0,
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
            }
        }
#else
        static ValueTask<EndpointResponse> PostAsync(Uri uri, string content, CancellationToken token = default)
        {
            return PostAsync(uri, new StringContent(content), token);
        }
        static ValueTask<EndpointResponse> PostAsync(Uri uri, byte[] content, CancellationToken token = default)
        {
            return PostAsync(uri, new ByteArrayContent(content), token);
        }
        static ValueTask<EndpointResponse> PostAsync(Uri uri, ArraySegment<byte> content, CancellationToken token = default)
        {
            return PostAsync(uri, new ByteArrayContent(content.Array, content.Offset, content.Count), token);
        }
        static async ValueTask<EndpointResponse> PostAsync(Uri uri, HttpContent content, CancellationToken token = default)
        {
            try
            {
                var client = MajEnv.SharedHttpClient;
                var rsp = await client.PostAsync(uri, content, token);
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
        }
#endif

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