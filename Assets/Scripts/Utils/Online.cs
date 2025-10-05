using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MajdataPlay.Settings;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    internal static class Online
    {
        readonly static HttpClient _client = MajEnv.SharedHttpClient;
        public static async UniTask<bool> CheckLogin(ApiEndpoint apiEndpoint)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    var rsp = await _client.GetAsync(apiEndpoint.Url + "/account/Info");
                    if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                });
            }
            catch
            {
                throw;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public static async UniTask Login(ApiEndpoint apiEndpoint)
        {
            try
            {
                await Task.Run(async () =>
                {
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
                    var formData = new MultipartFormDataContent
                    {
                        { new StringContent(apiEndpoint.Username), "username" },
                        { new StringContent(pwdHashStr.Replace("-", "").ToLower()), "password" }
                    };

                    var rsp = await _client.PostAsync(apiEndpoint.Url + "/account/Login", formData);
                    if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Login failed");
                    }
                });
            }
            catch
            {
                throw;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public static async UniTask SendLike(OnlineSongDetail song)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var serverInfo = song.ServerInfo;
                    await Login(song.ServerInfo);
                    var interactUrl = serverInfo.Url + "/maichart/" + song.Id + "/interact";
                    var intStream = await _client.GetStreamAsync(interactUrl);

                    var intlist = await Serializer.Json.DeserializeAsync<MajNetSongInteract>(intStream);

                    if (intlist.IsLiked)
                    {
                        throw new Exception(Localization.GetLocalizedText("THUMBUP_ALREADY"));
                    }

                    var formData = new MultipartFormDataContent
                    {
                        { new StringContent("like"), "type" },
                        { new StringContent("..."), "content" },
                    };
                    var rsp = await _client.PostAsync(interactUrl, formData);

                    if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception(Localization.GetLocalizedText("THUMBUP_FAILED"));
                    }
                });
            }
            catch
            {
                throw;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public static async UniTask SendScore(OnlineSongDetail song, MaiScore score)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var serverInfo = song.ServerInfo;
                    await Login(serverInfo);
                    var scoreUrl = serverInfo.Url + "/maichart/" + song.Id + "/score";
                    var json = await Serializer.Json.SerializeAsync(score);
                    var rsp = await _client.PostAsync(scoreUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                    var rspContent = await rsp.Content.ReadAsStringAsync();

                    if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception(rspContent);
                    }
                });
            }
            catch
            {
                throw;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
    }

}