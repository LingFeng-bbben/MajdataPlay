using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Result
{
    public class OnlineInteractionSender : MonoBehaviour
    {
        public Text infotext;
        public Text uploadtext;
        public Image thumb;

        OnlineSongDetail _onlineDetail;

        bool _isInited = false;
        bool _isThumbUpRequested = false;

        readonly static string[] SFX_LIST = new string[] { "dianzan_comment.wav", "dianzan_comment_2.wav", "dianzan_comment_3.wav" };

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
            _isInited = true;
            _onlineDetail = onlineDetail;
            infotext.text = "THUMBUP_INFO".i18n();
            return true;
        }
        void Update()
        {
            if(!_isInited || _isThumbUpRequested)
            {
                return;
            }
            if(InputManager.IsSensorClickedInThisFrame(SensorArea.E3) || InputManager.IsSensorClickedInThisFrame(SensorArea.B3))
            {
                SendInteraction(_onlineDetail);
            }
        }
        void SendInteraction(OnlineSongDetail song)
        {
            _ = SendLikeAsync(song);
        }

        async Task SendLikeAsync(OnlineSongDetail song)
        {
            await UniTask.SwitchToMainThread();
            infotext.text = "THUMBUP_SENDING".i18n();
            //LightManager.SetButtonLight(Color.blue, 4);
            try
            {
                await Online.SendLike(song);
                await UniTask.SwitchToMainThread();
                infotext.text = "THUMBUP_SENDED".i18n();
                MajInstances.AudioManager.PlaySFX(SFX_LIST[UnityEngine.Random.Range(0, SFX_LIST.Length)]);
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread();
                infotext.text = ex.Message;
                MajDebug.LogError(ex);
                //LightManager.SetButtonLight(Color.red, 4);
                return;
            }
        }

        public async Task SendScoreAsync(MaiScore score)
        {
            for(int i = 0; i < MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
            {
                try
                {
                    await UniTask.SwitchToMainThread();
                    uploadtext.text = "SCORE_SENDING".i18n();
                    await Online.SendScore(_onlineDetail, score);
                    await UniTask.SwitchToMainThread();
                    uploadtext.text = "SCORE_SENDED".i18n();
                    return;
                }
                catch (Exception ex)
                {
                    if(ex is TaskCanceledException)
                    {
                        await UniTask.SwitchToMainThread();
                        uploadtext.text = "Retry in 1s..." + ex.Message;
                        MajDebug.LogError(ex);
                        await UniTask.Delay(1000);
                    }
                    else
                    {
                        await UniTask.SwitchToMainThread();
                        uploadtext.text = ex.Message;
                        MajDebug.LogError(ex);
                        return;
                    }
                }
            }
            
        }
    }
}