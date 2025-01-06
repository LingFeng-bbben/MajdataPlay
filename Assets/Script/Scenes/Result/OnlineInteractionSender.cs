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

namespace MajdataPlay.Result
{
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
            if (e.IsClick && (e.Type == SensorType.E3 || e.Type == SensorType.B3))
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
            }
            catch (Exception ex)
            {
                infotext.text = "点赞失败";
                Debug.LogError(ex);
                //MajInstances.LightManager.SetButtonLight(Color.red, 4);
                return;
            }
        }
       

    }
}