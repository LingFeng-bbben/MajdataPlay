using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using MajdataPlay.Settings;
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
        public static async UniTask<bool> CheckLogin(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                using var req = UnityWebRequestFactory.Get(apiEndpoint.Url + "/account/Info");
                var asyncOperation = req.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        req.Abort();
                        throw new HttpException(HttpErrorCode.Canceled);
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
                var rsp = await _client.GetAsync(apiEndpoint.Url + "/account/Info", token);
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
        public static async UniTask Login(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                if (apiEndpoint == null)
                {
                    throw new ArgumentNullException(nameof(apiEndpoint));
                }
                if (apiEndpoint.Username == "YourUsername" || apiEndpoint.Password == "YourUsername")
                {
                    throw new Exception("Username or Password is unset");
                }
                if (string.IsNullOrEmpty(apiEndpoint.Username) || string.IsNullOrEmpty(apiEndpoint.Password))
                {
                    throw new Exception("Username or Password is null");
                }
                if (await CheckLogin(apiEndpoint))
                {
                    return;
                }
                var pwdHashStr = HashHelper.ToHexString(await HashHelper.ComputeHashAsync(Encoding.UTF8.GetBytes(apiEndpoint.Password)));
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var form = new WWWForm();
                form.AddField("username", apiEndpoint.Username);
                form.AddField("password", pwdHashStr.Replace("-", "").ToLower());
                using var req = UnityWebRequestFactory.Post(apiEndpoint.Url + "/account/Login", form);
                var asyncOperation = req.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        req.Abort();
                        throw new HttpException(HttpErrorCode.Canceled);
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
                    { new StringContent(apiEndpoint.Username), "username" },
                    { new StringContent(pwdHashStr.Replace("-", "").ToLower()), "password" }
                };

                var rsp = await _client.PostAsync(apiEndpoint.Url + "/account/Login", formData, token);
                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Login failed");
                }
#endif
            }
        }
        public static async UniTask SendLike(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await Login(song.ServerInfo, token);
                var interactUrl = serverInfo.Url + "/maichart/" + song.Id + "/interact";

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                await UniTask.SwitchToMainThread();
                var getReq = UnityWebRequestFactory.Get(interactUrl);
                var asyncOperation = getReq.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        getReq.Abort();
                        throw new HttpException(HttpErrorCode.Canceled);
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
                        throw new HttpException(HttpErrorCode.Canceled);
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
        public static async UniTask SendScore(OnlineSongDetail song, MaiScore score, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var serverInfo = song.ServerInfo;
                await Login(serverInfo);
                var scoreUrl = serverInfo.Url + "/maichart/" + song.Id + "/score";
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
                        throw new HttpException(HttpErrorCode.Canceled);
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
    }

}