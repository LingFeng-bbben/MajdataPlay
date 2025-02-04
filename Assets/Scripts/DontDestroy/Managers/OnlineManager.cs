using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal class OnlineManager : MonoBehaviour
    {
        readonly HttpClient _client = MajEnv.SharedHttpClient;
        private void Awake()
        {
            DontDestroyOnLoad(this);
            MajInstances.OnlineManager = this;

        }

        public async UniTask<bool> CheckLogin(ApiEndpoint apiEndpoint)
        {
            await Task.Yield();
            try
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

        public async UniTask Login(ApiEndpoint apiEndpoint)
        {
            await Task.Yield();
            try
            {
                if (apiEndpoint == null)
                {
                    throw new ArgumentNullException(nameof(apiEndpoint));
                }
                if (apiEndpoint.Username == "YourUsername" || apiEndpoint.Password == "YourUsername")
                {
                    throw new Exception("Username or Password is unset");
                }
                if (apiEndpoint.Username == null || apiEndpoint.Password == null)
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

                var rsp =  await _client.PostAsync(apiEndpoint.Url + "/account/Login", formData);
                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Login failed");
                }
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

        public async UniTask SendLike(OnlineSongDetail song)
        {
            await Task.Yield();
            try
            {
                var serverInfo = song.ServerInfo;
                await Login(song.ServerInfo);
                var interactUrl = serverInfo.Url + "/maichart/" + song.Id + "/interact";
                var intStream = await _client.GetStreamAsync(interactUrl);

                var intlist = await Serializer.Json.DeserializeAsync<MajNetSongInteract>(intStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (intlist.IsLiked)
                {
                    throw new Exception("你已经点过赞了！");
                }

                var formData = new MultipartFormDataContent
                {
                    { new StringContent("like"), "type" },
                    { new StringContent("..."), "content" },
                };
                var rsp = await _client.PostAsync(interactUrl, formData);

                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("点赞失败");
                }
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

        public async UniTask SendScore(OnlineSongDetail song, MaiScore score)
        {
            await Task.Yield();
            try
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