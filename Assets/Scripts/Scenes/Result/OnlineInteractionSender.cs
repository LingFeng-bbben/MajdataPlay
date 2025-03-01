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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Result
{
    public class OnlineInteractionSender : MonoBehaviour
    {
        public Text infotext;
        public Text uploadtext;
        public Image thumb;

        OnlineSongDetail _onlineDetail;

        public bool Init(ISongDetail song)
        {
            if (song is not OnlineSongDetail onlineDetail)
            {
                infotext.text = "";
                thumb.gameObject.SetActive(false);
                return false;
            }
                
            var serverInfo = onlineDetail.ServerInfo;
            if (serverInfo is null)
            {
                infotext.text = "";
                thumb.gameObject.SetActive(false);
                return false;
            }
            _onlineDetail = onlineDetail;
            MajInstances.InputManager.BindAnyArea(OnAreaDown);
            //MajInstances.LightManager.SetButtonLight(Color.yellow, 2);
            return true;
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (e.IsDown && (e.Type == SensorArea.E3 || e.Type == SensorArea.B3))
            {
                MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                SendInteraction(_onlineDetail);
            }
        }

        private void OnDestroy()
        {
            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
        }

        internal void SendInteraction(OnlineSongDetail song)
        {
            SendLike(song).Forget();
        }

        async UniTask SendLike(OnlineSongDetail song)
        {
            infotext.text = "稍等...";
            //MajInstances.LightManager.SetButtonLight(Color.blue, 4);
            try
            {
                await MajInstances.OnlineManager.SendLike(song);
                infotext.text = "点赞成功";
            }
            catch (Exception ex)
            {
                infotext.text = ex.Message;
                MajDebug.LogError(ex);
                //MajInstances.LightManager.SetButtonLight(Color.red, 4);
                return;
            }
        }

        public async UniTask SendScore(MaiScore score)
        {
            uploadtext.text = "正在上传成绩";
            try
            {
                await MajInstances.OnlineManager.SendScore(_onlineDetail, score);
                uploadtext.text = "上传成绩成功";
            }
            catch (Exception ex)
            {
                uploadtext.text = ex.Message;
                MajDebug.LogError(ex);
                return;
            }
        }
    }
}