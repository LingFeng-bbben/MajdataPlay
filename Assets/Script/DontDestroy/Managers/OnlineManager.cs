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
using UnityEngine;

public class OnlineManager : MonoBehaviour
{
    private HttpClient _client => HttpTransporter.ShareClient;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        MajInstances.OnlineManager = this;

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public async UniTask<bool> CheckLogin(ApiEndpoint apiEndpoint)
    {
        var task = _client.GetAsync(apiEndpoint.Url + "/account/Info");
        while (!task.IsCompleted)
        {
            await UniTask.Yield();
        }
        if (task.Result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return false;
        }
        else { return true; }
    }

    public async UniTask Login(ApiEndpoint apiEndpoint)
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
        if(await CheckLogin(apiEndpoint))
        {
            return;
        }
        var formData = new MultipartFormDataContent
        {
            { new StringContent(apiEndpoint.Username), "username" },
            { new StringContent(ComputeMD5(apiEndpoint.Password)), "password" }
        };

        var task = _client.PostAsync(apiEndpoint.Url + "/account/Login", formData);
        while (!task.IsCompleted)
        {
            await UniTask.Yield();
        }
        if (task.Result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Login failed");
        }
    }

    public async UniTask SendLike(SongDetail song)
    {
        await Login(song.ApiEndpoint);
        var interactUrl = song.ApiEndpoint.Url + "/maichart/" + song.OnlineId + "/interact";
        var task = _client.GetStringAsync(interactUrl);
        while (!task.IsCompleted)
        {
            await UniTask.Yield();
        }
        var intjson = task.Result;

        var intlist = JsonSerializer.Deserialize<MajNetSongInteract>(intjson, new JsonSerializerOptions
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
        var liketask = _client.PostAsync(interactUrl, formData);
        while (!liketask.IsCompleted)
        {
            await UniTask.Yield();
        }

        if (liketask.Result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("点赞失败");
        }
    }

    public async UniTask SendScore(SongDetail song, MaiScore score)
    {
        await Login(song.ApiEndpoint);
        var scoreUrl = song.ApiEndpoint.Url + "/maichart/" + song.OnlineId + "/score";
        var json = JsonSerializer.Serialize(score);
        var task = _client.PostAsync(scoreUrl, new StringContent(json,Encoding.UTF8, "application/json"));
        while (!task.IsCompleted)
        {
            await UniTask.Yield();
        }
        var task2 = task.Result.Content.ReadAsStringAsync();
        while (!task2.IsCompleted)
        {
            await UniTask.Yield();
        }
        if (task.Result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception(task2.Result);
        }
    }
    static string ComputeMD5(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
