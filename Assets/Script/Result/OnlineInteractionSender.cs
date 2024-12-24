using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnityEngine;
using UnityEngine.UI;

public class OnlineInteractionSender : MonoBehaviour
{
    public Text infotext;
    public Image thumb;

    SongDetail SongDetail;

    public bool Init(SongDetail song)
    {
        if (song.ApiEndpoint == null)
        {
            infotext.text = "";
            thumb.gameObject.SetActive(false);
            return false;
        }
        SongDetail = song;
        MajInstances.InputManager.BindAnyArea(OnAreaDown);
        //MajInstances.LightManager.SetButtonLight(Color.yellow, 2);
        return true;
    }

    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (e.IsClick && (e.Type == SensorType.E3|| e.Type == SensorType.B3))
        {
            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
            SendInteraction(SongDetail);
        }
    }

    private void OnDestroy()
    {
        MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
    }

    public void SendInteraction(SongDetail song)
    {
        SendLike(song).Forget();
    }

    async UniTask SendLike(SongDetail song)
    {
        infotext.text = "稍等...";
        //MajInstances.LightManager.SetButtonLight(Color.blue, 4);
        var client = HttpTransporter.ShareClient;
        try
        {
            if (song.ApiEndpoint == null)
            {
                infotext.text = "";
                //MajInstances.LightManager.SetButtonLight(Color.red, 4);
                return;
            }

            if (song.ApiEndpoint.Username == null || song.ApiEndpoint.Password == null)
            {
                infotext.text = "登录失败";
                //MajInstances.LightManager.SetButtonLight(Color.red, 4);
                return;
            }

            var interactUrl = song.ApiEndpoint.Url + "/maichart/" + song.OnlineId + "/interact";
            var task = client.GetStringAsync(interactUrl);
            while (!task.IsCompleted)
            {
                await UniTask.Yield();
            }
            var intjson = task.Result;

            var intlist = JsonSerializer.Deserialize<MajNetSongInteract>(intjson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (intlist.Likes.Any(o => o == song.ApiEndpoint.Username))
            {
                infotext.text = "你已经点过赞了！@！";
                //MajInstances.LightManager.SetButtonLight(Color.red, 4);
                return;
            }

            var formData = new MultipartFormDataContent
        {
            { new StringContent(song.ApiEndpoint.Username), "username" },
            { new StringContent(ComputeMD5(song.ApiEndpoint.Password)), "password" }
        };

            var tokentask = client.PostAsync(song.ApiEndpoint.Url + "/account/Login", formData);
            while (!tokentask.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (tokentask.Result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                infotext.text = "登录失败";
                return;
            }

            formData = new MultipartFormDataContent
            {
                { new StringContent("like"), "type" },
                { new StringContent("..."), "content" },
            };
            var liketask = client.PostAsync(interactUrl, formData);
            while (!liketask.IsCompleted)
            {
                await UniTask.Yield();
            }

            if (liketask.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                infotext.text = "点赞成功";
                //MajInstances.LightManager.SetButtonLight(Color.green, 4);
            }
        }catch (Exception ex)
        {
            infotext.text = "点赞失败";
            Debug.LogError(ex);
            //MajInstances.LightManager.SetButtonLight(Color.red, 4);
            return;
        }
    }
    public static string ComputeMD5(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

}
