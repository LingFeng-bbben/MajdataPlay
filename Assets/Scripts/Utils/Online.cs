using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Net;
using MajdataPlay.Settings;
using MajdataPlay.Unsafe;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Printing;
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
        public const string API_GET_USER_ICON = "account/icon?username={0}";
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

        static SpinLock _dictLock = new();
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
                                var isAlive = await GetUserInfoAsync(statistics.Endpoint, token) != null;
                                statistics.IsUserLoggedIn = isAlive;
                                if(!isAlive)
                                {
                                    var runtimeConfig = statistics.Endpoint.RuntimeConfig;
                                    runtimeConfig.Avatar = null;
                                    runtimeConfig.Username = string.Empty;
                                }
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
        public static async ValueTask<UserSummary?> GetUserInfoAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                try
                {
                    var uri = apiEndpoint.Url.Combine(API_GET_USER_INFO);
                    var rsp = default(EndpointResponse);
                    for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                    {
                        rsp = await GetAsync(uri, token);
                        if(rsp.StatusCode is HttpStatusCode.Unauthorized)
                        {
                            return default;
                        }
                        else if(!rsp.IsSuccessfully || !rsp.IsDeserializable)
                        {
                            MajDebug.LogError("Failed to get user info");
                            MajDebug.LogError($"Url:{uri}\nStatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                            continue;
                        }
                        var userinfo = await rsp.DeserializeAsync<UserSummary>();
                        MajDebug.LogInfo("Login as " + userinfo.Username);
                        return userinfo;
                    }
                    return default;
                }
                catch(Exception e)
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
                var rsp = default(EndpointResponse);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                rsp = await PostAsync(uri, null, token);
#else
                rsp = await PostAsync(uri, token);
#endif
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
                var rsp = await GetAsync(uri, token);
                return rsp;
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
                var rsp = default(EndpointResponse);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                rsp = await PostAsync(uri, null, token);
#else
                rsp = await PostAsync(uri, token);
#endif
                return rsp;
            }
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
                if (await GetUserInfoAsync(apiEndpoint)!=null)
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
                if(apiEndpoint.RuntimeConfig.AuthMethod != NetAuthMethodOption.Plain)
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
                            var rsp = default(EndpointResponse);
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                            rsp = await PostAsync(uri, null, token);
#else
                            rsp = await PostAsync(uri, token);
#endif
                            MajDebug.LogInfo(rsp.Message + rsp.ErrorCode + rsp.StatusCode);
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogException(e);
                        }
                        finally
                        {
                            apiEndpoint.RuntimeConfig.AuthMethod = NetAuthMethodOption.None;
                            apiEndpoint.RuntimeConfig.Avatar = null;
                            apiEndpoint.RuntimeConfig.Username = "???";
                            apiEndpoint.RuntimeConfig.AuthUsername = apiEndpoint.Username;
                            apiEndpoint.RuntimeConfig.AuthPassword = apiEndpoint.Password;
                        }
                    }
                    finally
                    {
                        statistics.Unlock();
                    }
                }
            }
        }
        public static async ValueTask<MajNetSongInteract?> GetChartInteractAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                var interactUrl = BuildMaiChartUri(song.ServerInfo, API_POST_MAICHART_INTERACT, song.Id);
                var rsp = default(EndpointResponse);

                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    rsp = await GetAsync(interactUrl, token);
                    if (rsp.IsSuccessfully && rsp.IsDeserializable && rsp.TryDeserialize<MajNetSongInteract?>(out var intlist) && intlist is not null)
                    {
                        MajDebug.LogDebug(rsp);
                        return intlist;
                    }
                    else
                    {
                        MajDebug.LogError(rsp);
                    }
                    if (rsp.ErrorCode == HttpErrorCode.Canceled)
                    {
                        break;
                    }
                    else if (rsp.StatusCode is HttpStatusCode.BadRequest
                        or HttpStatusCode.NotFound
                        or HttpStatusCode.Unauthorized
                        or HttpStatusCode.Forbidden)
                    {
                        return null;
                    }
                    else if (!rsp.IsSuccessfully && rsp.StatusCode is not null)
                    {
                        return null;
                    }
                }
                return null;                
            }
        }
        public static async ValueTask<EndpointResponse> PostLikeAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                var interactUrl = BuildMaiChartUri(song.ServerInfo, API_POST_MAICHART_INTERACT, song.Id);
                var rsp = default(EndpointResponse);

                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                    var form = new WWWForm();
                    form.AddField("type", "like");
                    form.AddField("content", "...");

                    rsp = await PostAsync(interactUrl, form, token);
#else
                    var formData = new MultipartFormDataContent
                    {
                        { new StringContent("like"), "type" },
                        { new StringContent("..."), "content" },
                    };
                    rsp = await PostAsync(interactUrl, formData, token);
#endif
                    if (rsp.IsSuccessfully)
                    {
                        MajDebug.LogDebug(rsp);
                        return rsp;
                    }
                    else
                    {
                        MajDebug.LogError(rsp);
                    }
                    if (rsp.ErrorCode == HttpErrorCode.Canceled)
                    {
                        break;
                    }
                    else if (rsp.StatusCode is HttpStatusCode.BadRequest
                        or HttpStatusCode.NotFound
                        or HttpStatusCode.Unauthorized
                        or HttpStatusCode.Forbidden)
                    {
                        break;
                    }
                    else if (!rsp.IsSuccessfully && rsp.StatusCode is not null)
                    {
                        break;
                    }
                }
                return rsp;
            }
        }
        public static async ValueTask<EndpointResponse> PostScoreAsync(OnlineSongDetail song, MaiScore score, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                var scoreUrl = BuildMaiChartUri(song.ServerInfo, API_POST_MAICHART_SCORE, song.Id);
                var json = await Serializer.Json.SerializeAsync(score, DEFAULT_JSON_SERIALIZER);
                var rsp = default(EndpointResponse);


                for (var i = 0; i < MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                    rsp = await PostAsync(scoreUrl, json, "application/json", token);
#else
                    rsp = await PostAsync(scoreUrl, new StringContent(json, Encoding.UTF8, "application/json"), token);
#endif
                    if (rsp.IsSuccessfully)
                    {
                        MajDebug.LogDebug(rsp);
                        return rsp;
                    }
                    else
                    {
                        MajDebug.LogError(rsp);
                    }
                    if (rsp.ErrorCode == HttpErrorCode.Canceled)
                    {
                        break;
                    }
                    else if(rsp.StatusCode is HttpStatusCode.BadRequest 
                        or HttpStatusCode.NotFound 
                        or HttpStatusCode.Unauthorized 
                        or HttpStatusCode.Forbidden)
                    {
                        break;
                    }
                    else if (!rsp.IsSuccessfully && rsp.StatusCode is not null)
                    {
                        break;
                    }
                }

                return rsp;
            }
        }
        public static async ValueTask<MajnetSongDetail[]?> GetChartListAsync(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var url = apiEndpoint.Url.Combine(API_GET_MAICHART_LIST);
                var rsp = default(EndpointResponse);

                for (var i = 0; i < MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    rsp = await GetAsync(url, token);
                    if(rsp.IsSuccessfully && rsp.TryDeserialize<MajnetSongDetail[]>(out var chartList) && chartList is not null)
                    {
                        MajDebug.LogDebug(rsp);
                        return chartList;
                    }
                    else
                    {
                        MajDebug.LogError(rsp);
                    }
                    if (rsp.ErrorCode == HttpErrorCode.Canceled)
                    {
                        break;
                    }
                    else if (!rsp.IsSuccessfully && rsp.StatusCode is not HttpStatusCode.OK)
                    {
                        break;
                    }
                }

                return null;
            }
        }
        public static async ValueTask<Sprite?> GetUserIconAsync(ApiEndpoint apiEndpoint,string username, CancellationToken token = default)
        {
            if(string.IsNullOrEmpty(username))
            {
                return null;
            }
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var url = apiEndpoint.Url.Combine(string.Format(API_GET_USER_ICON, username));
                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        var rsp = await GetAsync(url, token);
                        if(!rsp.IsSuccessfully)
                        {
                            MajDebug.LogError("Failed to download user icon");
                            MajDebug.LogError($"Url:{url}\nStatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                            continue;
                        }
                        var avatar = await SpriteLoader.LoadAsync(rsp.AsMemory());
                        return avatar;
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError("Failed to download user icon");
                        MajDebug.LogError(e);
                    }
                }
                return null;
            }
        }

        static async ValueTask<EndpointResponse> GetAsync(Uri uri, CancellationToken token = default)
        {
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
            await UniTask.SwitchToMainThread();
            var getReq = UnityWebRequestFactory.Get(uri);
            var headers = default(IReadOnlyDictionary<string, IEnumerable<string>>);
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
                headers = getReq.GetResponseHeaders()?.GroupBy(x => x.Key)
                                                      .ToDictionary(x => x.Key, x => x.Select(x => x.Value).AsEnumerable());
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
                    IsSuccessfully = true,
                    IsDeserializable = true && buffer.Length != 0,
                    ErrorCode = default,
                    StatusCode = (HttpStatusCode)getReq.responseCode,
                    Message = "",
                    Headers = headers ?? EndpointResponse.EMPTY_HEADERS,
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
                    Message = httpE.Message,
                    Headers = headers ?? EndpointResponse.EMPTY_HEADERS,
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
                    Message = e.ToString(),
                    Headers = headers ?? EndpointResponse.EMPTY_HEADERS,
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
        static async ValueTask<EndpointResponse> PostAsync(Uri uri, WWWForm? form = null, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Post(uri, form);
                var headers = default(IReadOnlyDictionary<string, IEnumerable<string>>);
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
                    headers = getReq.GetResponseHeaders()?.GroupBy(x => x.Key)
                                                          .ToDictionary(x => x.Key, x => x.Select(x => x.Value).AsEnumerable());
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
                        IsSuccessfully = true,
                        IsDeserializable = true && buffer.Length != 0,
                        ErrorCode = default,
                        StatusCode = (HttpStatusCode)getReq.responseCode,
                        Message = "",
                        Headers = headers ?? EndpointResponse.EMPTY_HEADERS
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
                        Message = httpE.Message,
                        Headers = headers ?? EndpointResponse.EMPTY_HEADERS
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
                        Message = e.ToString(),
                        Headers = headers ?? EndpointResponse.EMPTY_HEADERS
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
                var headers = default(IReadOnlyDictionary<string, IEnumerable<string>>);
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
                    headers = getReq.GetResponseHeaders()?.GroupBy(x => x.Key)
                                                          .ToDictionary(x => x.Key, x => x.Select(x => x.Value).AsEnumerable());
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
                        IsSuccessfully = true,
                        IsDeserializable = true && buffer.Length != 0,
                        ErrorCode = default,
                        StatusCode = (HttpStatusCode)getReq.responseCode,
                        Message = "",
                        Headers = headers ?? EndpointResponse.EMPTY_HEADERS
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
                        Message = httpE.Message,
                        Headers = headers ?? EndpointResponse.EMPTY_HEADERS
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
                        Message = e.ToString(),
                        Headers = headers ?? EndpointResponse.EMPTY_HEADERS
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