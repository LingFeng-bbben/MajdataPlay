using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Net;
using MajdataPlay.Unsafe;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public const string API_GET_MACHINE_INFO = "machine/info";
        public const string API_GET_AUTH_CHECK = "machine/auth/check";

        public const string API_POST_USER_LOGIN = "account/login";
        public const string API_POST_USER_LOGOUT = "account/logout";
        public const string API_POST_MAICHART_INTERACT = "maichart/{0}/interact";
        public const string API_POST_MAICHART_SCORE = "maichart/{0}/score";
        public const string API_POST_MACHINE_REGISTER = "machine/register";
        public const string API_POST_AUTH_REQUEST = "machine/auth/request";
        public const string API_POST_AUTH_REVOKE = "machine/auth/revoke";

        static SpinLock _dictLock = new ();
        readonly static Dictionary<ApiEndpoint, ApiEndpointStatistics> _endpointStatistics = new();

        public static async ValueTask HeartbeatAsync(CancellationToken token = default)
        {
            using var rentedBuffer = new RentedList<ApiEndpointStatistics>();
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                GetAllApiEndpointStatistic(rentedBuffer);
                foreach (var statistics in rentedBuffer)
                {
                    await statistics.LockAsync(token);
                    try
                    {
                        if (statistics.IsMachineRegistered is true)
                        {
                            try
                            {
                                var isAlive = await CheckMachineRegisterAsync(statistics.Endpoint, token);
                                statistics.IsMachineRegistered = isAlive;
                            }
                            catch (Exception e)
                            {
                                MajDebug.LogException(e);
                            }
                        }
                        if (statistics.IsUserLoggedIn is true)
                        {
                            try
                            {
                                var isAlive = await CheckLoginAsync(statistics.Endpoint, token) != null;
                                statistics.IsUserLoggedIn = isAlive;
                            }
                            catch (Exception e)
                            {
                                MajDebug.LogException(e);
                            }
                        }
                    }
                    finally
                    {
                        statistics.Unlock();
                    }
                }
                MajDebug.LogDebug("Online heartbeat has been completed");
            }
        }
        public static async ValueTask<UserSummary?> CheckLoginAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                try
                {
                    var uri = apiEndpoint.Url.Combine(API_GET_USER_INFO);
                    var rsp = await GetAsync(uri, token);
                    var userinfo = await rsp.DeserializeAsync<UserSummary>();
                    MajDebug.LogInfo("Login as " + userinfo.Username);
                    return userinfo;
                }catch(Exception e)
                {
                    MajDebug.LogError("Get Userinfo failed: ");
                    MajDebug.LogException(e);
                    return null;
                }
            }
        }
        public static async ValueTask<bool> CheckMachineRegisterAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var uri = apiEndpoint.Url.Combine(API_GET_MACHINE_INFO);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                var rsp = await GetAsync(uri, token);

                return rsp.IsSuccessfully;
#else
                var rsp = await GetAsync(uri, token);

                return rsp.IsSuccessfully;
#endif
            }
        }
        public static async ValueTask<EndpointResponse> RegisterAsync(ApiEndpoint apiEndpoint, MachineInfo machineInfo, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                var statistics = GetApiEndpointStatistic(apiEndpoint);
            FAST_RETURN:
                if (statistics.IsMachineRegistrationSupported is false)
                {
                    return new()
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorCode = HttpErrorCode.NotSupported,
                        Message = "MAJTEXT_ONLINE_MACHINE_REGISTRATION_UNSUPPORTED"
                    };
                }
                try
                {
                    await statistics.LockAsync();
                    if(statistics.IsMachineRegistrationSupported is false)
                    {
                        goto FAST_RETURN;
                    }
                    var uri = apiEndpoint.Url.Combine(API_POST_MACHINE_REGISTER);
                    var json = await Serializer.Json.SerializeAsync(machineInfo, DEFAULT_JSON_SERIALIZER);

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                    var rsp = await PostAsync(uri, json, "application/json", token);
                    if (!rsp.IsSuccessfully && rsp.StatusCode == HttpStatusCode.NotFound)
                    {
                        statistics.IsMachineRegistrationSupported = false;
                        statistics.IsMachineRegistered = false;
                        return new()
                        {
                            IsSuccessfully = false,
                            IsDeserializable = false,
                            StatusCode = HttpStatusCode.NotFound,
                            ErrorCode = HttpErrorCode.NotSupported,
                            Message = "MAJTEXT_ONLINE_MACHINE_REGISTRATION_UNSUPPORTED"
                        };
                    }
                    statistics.IsMachineRegistrationSupported = true;
                    statistics.IsMachineRegistered = rsp.IsSuccessfully;
                    return rsp;
#else
                    var rsp = await PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"), token);
                    if (!rsp.IsSuccessfully && rsp.StatusCode == HttpStatusCode.NotFound)
                    {
                        statistics.IsMachineRegistrationSupported = false;
                        statistics.IsMachineRegistered = false;
                        return new()
                        {
                            IsSuccessfully = false,
                            IsDeserializable = false,
                            StatusCode = HttpStatusCode.NotFound,
                            ErrorCode = HttpErrorCode.NotSupported,
                            Message = "MAJTEXT_ONLINE_MACHINE_REGISTRATION_UNSUPPORTED"
                        };
                    }
                    statistics.IsMachineRegistrationSupported = true;
                    statistics.IsMachineRegistered = rsp.IsSuccessfully;
                    return rsp;
#endif
                }
                finally
                {
                    statistics.Unlock();
                }
            }
        }
        public static async ValueTask<EndpointResponse> AuthRequestAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                var statistics = GetApiEndpointStatistic(apiEndpoint);
                if (statistics.IsMachineRegistrationSupported is false)
                {
                    return new()
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorCode = HttpErrorCode.NotSupported,
                        Message = "MAJTEXT_ONLINE_MACHINE_REGISTRATION_UNSUPPORTED"
                    };
                }
                else if (statistics.IsMachineRegistrationSupported is null)
                {
                    throw new InvalidOperationException();
                }
                var uri = apiEndpoint.Url.Combine(API_POST_AUTH_REQUEST);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
#else
                var rsp = await PostAsync(uri, token);
                if (rsp.StatusCode == HttpStatusCode.Created)
                {
                    return new(rsp.AsMemory(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = true,
                        IsDeserializable = true,
                        StatusCode = rsp.StatusCode,
                        ErrorCode = HttpErrorCode.NoError,
                        Headers = rsp.Headers,
                        Message = string.Empty
                    };
                }
                else
                {
                    return new()
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        StatusCode = rsp.StatusCode,
                        ErrorCode = rsp.ErrorCode,
                        Headers = rsp.Headers,
                        Message = "MAJTEXT_ONLINE_MACHINE_AUTH_REQUEST_FAILED"
                    };
                }
#endif
            }
        }
        public static async ValueTask<EndpointResponse> AuthCheckAsync(ApiEndpoint apiEndpoint, string authId, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                var statistics = GetApiEndpointStatistic(apiEndpoint);
                if (statistics.IsMachineRegistrationSupported is false)
                {
                    return new()
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorCode = HttpErrorCode.NotSupported,
                        Message = "MAJTEXT_ONLINE_MACHINE_REGISTRATION_UNSUPPORTED"
                    };
                }
                else if (statistics.IsMachineRegistrationSupported is null)
                {
                    throw new InvalidOperationException();
                }
                var uriBuilder = new UriBuilder(apiEndpoint.Url.Combine(API_GET_AUTH_CHECK));
                uriBuilder.Query = $"auth-id={authId}";
                var uri = uriBuilder.Uri;
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
#else
                var rsp = await GetAsync(uri, token);
                return rsp;
#endif
            }
        }
        public static async ValueTask<EndpointResponse> AuthRevokeAsync(ApiEndpoint apiEndpoint, string authId, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                var statistics = GetApiEndpointStatistic(apiEndpoint);
                if (statistics.IsMachineRegistrationSupported is false)
                {
                    return new()
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorCode = HttpErrorCode.NotSupported,
                        Message = "MAJTEXT_ONLINE_MACHINE_REGISTRATION_UNSUPPORTED"
                    };
                }
                else if (statistics.IsMachineRegistrationSupported is null)
                {
                    throw new InvalidOperationException();
                }
                var uriBuilder = new UriBuilder(apiEndpoint.Url.Combine(API_POST_AUTH_REVOKE));
                uriBuilder.Query = $"auth-id={authId}";
                var uri = uriBuilder.Uri;
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
#else
                var rsp = await PostAsync(uri, token);
                return rsp;
#endif
            }
        }
        public static ValueTask<EndpointResponse> LoginAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            return LoginAsync(apiEndpoint, apiEndpoint?.Username ?? string.Empty, apiEndpoint?.Password ?? string.Empty, token);
        }
        public static async ValueTask<EndpointResponse> LoginAsync(ApiEndpoint apiEndpoint, string username, string password, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                if (apiEndpoint == null)
                {
                    throw new ArgumentNullException(nameof(apiEndpoint));
                }
                if (await CheckLoginAsync(apiEndpoint)!=null)
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
                if(apiEndpoint.AuthMethod != Settings.NetAuthMethodOption.Plain)
                {
                    var returnValue = new EndpointResponse()
                    {
                        ErrorCode = HttpErrorCode.NotSupported,
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        Message = "MAJTEXT_ONLINE_UNSUPPORTED_AUTH_METHOD"
                    };
                }
                else
                {
                    var returnValue = new EndpointResponse()
                    {
                        ErrorCode = HttpErrorCode.InvalidRequest,
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        Message = "MAJTEXT_ONLINE_USERNAME_OR_PASSWORD_UNSET"
                    };
                    if (username == "YourUsername" || password == "YourUsername")
                    {
                        return returnValue;
                    }
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        return returnValue;
                    }
                }

               var pwdHashStr = HashHelper.ToHexString(await HashHelper.ComputeHashAsync(Encoding.UTF8.GetBytes(password)));
                var uri = apiEndpoint.Url.Combine(API_POST_USER_LOGIN);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var form = new WWWForm();
                form.AddField("username", username);
                form.AddField("password", pwdHashStr.Replace("-", "").ToLower());
                var rsp = await PostAsync(uri, form, token);

#else
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(pwdHashStr.Replace("-", "").ToLower()), "password" }
                };

                var rsp = await PostAsync(uri, formData, token);
#endif
                if(rsp.StatusCode is HttpStatusCode.Unauthorized)
                {
                    rsp = new(rsp.AsMemory(), DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        StatusCode = rsp.StatusCode,
                        ErrorCode = rsp.ErrorCode,
                        IsSuccessfully = rsp.IsSuccessfully,
                        IsDeserializable = rsp.IsDeserializable,
                        Headers = rsp.Headers,
                        Message = "MAJTEXT_ONLINE_USERNAME_OR_PASSWORD_INCORRECT"
                    };
                }
                return rsp;
            }
        }
        public static async ValueTask LogoutAllAsync(CancellationToken token = default)
        {
            using var rentedBuffer = new RentedList<ApiEndpointStatistics>();
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                GetAllApiEndpointStatistic(rentedBuffer);
                foreach(var statistics in rentedBuffer)
                {
                    await statistics.LockAsync(token);
                    try
                    {
                        var apiEndpoint = statistics.Endpoint;
                        try
                        {
                            MajDebug.LogInfo("Logout");
                            var uri = apiEndpoint.Url.Combine(API_POST_USER_LOGOUT);
                            var rsp = await PostAsync(uri, token);
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogException(e);
                        }

                    }
                    finally
                    {
                        statistics.Unlock();
                    }
                }
            }
        }
        public static async ValueTask<EndpointResponse> PostLikeAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await LoginAsync(song.ServerInfo, token);
                var interactUrl = BuildMaiChartUri(song.ServerInfo, API_POST_MAICHART_INTERACT, song.Id);

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
        public static async ValueTask<EndpointResponse> PostScoreAsync(OnlineSongDetail song, MaiScore score, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await LoginAsync(serverInfo);
                var scoreUrl = BuildMaiChartUri(song.ServerInfo, API_POST_MAICHART_SCORE, song.Id);
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
                        Headers = rsp.Headers.ToDictionary(kv => kv.Key, kv => kv.Value),
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
                    Headers = rsp.Headers.ToDictionary(kv => kv.Key, kv => kv.Value),
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
        static ValueTask<EndpointResponse> PostAsync(Uri uri, CancellationToken token = default)
        {
            return PostAsync(uri, (HttpContent?)null, token);
        }
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
        static async ValueTask<EndpointResponse> PostAsync(Uri uri, HttpContent? content, CancellationToken token = default)
        {
            try
            {
                var client = MajEnv.SharedHttpClient;
                var rsp = await (content is null ? client.PostAsync(uri, new StringContent(string.Empty, Encoding.UTF8, "application/json"), token) : client.PostAsync(uri, content, token));
                var buffer = await rsp.Content.ReadAsByteArrayAsync();
                if (rsp.StatusCode != HttpStatusCode.OK)
                {
                    rsp.Headers.ToDictionary(kv => kv.Key, kv => kv.Value);
                    return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                    {
                        IsSuccessfully = false,
                        IsDeserializable = false,
                        ErrorCode = HttpErrorCode.Unsuccessful,
                        StatusCode = rsp.StatusCode,
                        Headers = rsp.Headers.ToDictionary(kv => kv.Key, kv => kv.Value),
                        Message = ""
                    };
                }
                
                return new EndpointResponse(buffer, DEFAULT_JSON_SERIALIZER, DEFAULT_JSON_SERIALIZER_SETTINGS)
                {
                    IsSuccessfully = true,
                    IsDeserializable = true,
                    ErrorCode = HttpErrorCode.NoError,
                    StatusCode = rsp.StatusCode,
                    Headers = rsp.Headers.ToDictionary(kv => kv.Key, kv => kv.Value),
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

        static Uri BuildMaiChartUri(ApiEndpoint endpoint, string template, string chartId)
        {
            return BuildMaiChartUri(endpoint, template, Guid.Parse(chartId));
        }
        static Uri BuildMaiChartUri(ApiEndpoint endpoint, string template, Guid chartId)
        {
            using var sb = ZString.CreateStringBuilder(true);
            sb.AppendFormat(template, chartId);

            return new Uri(endpoint.Url, sb.ToString());
        }

        static ApiEndpointStatistics GetApiEndpointStatistic(ApiEndpoint endpoint)
        {
            ref var @lock = ref _dictLock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                if (!_endpointStatistics.TryGetValue(endpoint, out var stats))
                {
                    stats = new ApiEndpointStatistics
                    {
                        Endpoint = endpoint
                    };
                    _endpointStatistics[endpoint] = stats;
                }
                return stats;
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        static int GetAllApiEndpointStatistic(IList<ApiEndpointStatistics> buffer)
        {
            ref var @lock = ref _dictLock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                var i = 0;
                foreach (var (_, statistics) in _endpointStatistics)
                {
                    i++;
                    buffer.Add(statistics);
                }
                return i;
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        class ApiEndpointStatistics
        {
            public required ApiEndpoint Endpoint { get; init; }

            public bool? IsMachineRegistered { get; set; }
            public bool? IsUserLoggedIn { get; set; }
            public bool? IsMachineRegistrationSupported { get; set; }

            readonly SemaphoreSlim _lock = new (1, 1);

            public void Lock()
            {
                _lock.Wait();
            }
            public Task LockAsync(CancellationToken token = default)
            {
                return _lock.WaitAsync(token);
            }
            public void Unlock()
            {
                _lock.Release();
            }
        }
        readonly struct NetMachineInfo
        {
            public string Id { get; init; }
            public string Name { get; init; }
            public string Description { get; init; }
            public string Place { get; init; }
        }
    }

}