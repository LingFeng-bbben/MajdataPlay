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
               
                thumb.gameObject.SetActive(false);
                return false;
            }
            _onlineDetail = onlineDetail;
            InputManager.BindAnyArea(OnAreaDown); 
            infotext.text = Localization.GetLocalizedText("THUMBUP_INFO");
            return true;
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (e.IsDown && (e.Type == SensorArea.E3 || e.Type == SensorArea.B3))
            {
                InputManager.UnbindAnyArea(OnAreaDown);
                SendInteraction(_onlineDetail);
            }
        }

        private void OnDestroy()
        {
            InputManager.UnbindAnyArea(OnAreaDown);
        }

        internal void SendInteraction(OnlineSongDetail song)
        {
            SendLike(song).Forget();
        }

        async UniTask SendLike(OnlineSongDetail song)
        {
            infotext.text = Localization.GetLocalizedText("THUMBUP_SENDING");
            //MajInstances.LightManager.SetButtonLight(Color.blue, 4);
            try
            {
                await Online.SendLike(song);
                infotext.text = Localization.GetLocalizedText("THUMBUP_SENDED");
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
            uploadtext.text = Localization.GetLocalizedText("SCORE_SENDING");
            try
            {
                await Online.SendScore(_onlineDetail, score);
                uploadtext.text = Localization.GetLocalizedText("SCORE_SENDED");
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