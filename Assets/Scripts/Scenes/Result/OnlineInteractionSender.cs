using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using NeoSmart.AsyncLock;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

        OnlineSongDetail? _onlineDetail;

        bool _isInited = false;
        bool _isThumbUpRequested = false;
        bool _isAlreadyThumbUp = false;
        bool _isScorePosted = false;

        readonly AsyncLock _sendScoreLock = new();
        readonly AsyncLock _thumbUpLock = new();
        readonly CancellationTokenSource _cts = new();
        readonly string[] SFX_LIST = new string[] { "dianzan_comment.wav", "dianzan_comment_2.wav", "dianzan_comment_3.wav" };

        public void Init(ISongDetail song)
        {
            if (song is not OnlineSongDetail onlineDetail)
            {
                infotext.text = "";
                thumb.gameObject.SetActive(false);
                return;
            }
                
            var serverInfo = onlineDetail.ServerInfo;
            if (serverInfo is null || serverInfo.RuntimeConfig.AuthMethod == NetAuthMethodOption.None)
            {
                thumb.gameObject.SetActive(false);
                return;
            }
            _isInited = true;
            _onlineDetail = onlineDetail;
            infotext.text = "THUMBUP_INFO".i18n();
        }
        void Update()
        {
            if(!_isInited || _isThumbUpRequested || _onlineDetail is null)
            {
                return;
            }
            if(!_isAlreadyThumbUp && (InputManager.IsSensorClickedInThisFrame(SensorArea.E3) || InputManager.IsSensorClickedInThisFrame(SensorArea.B3)))
            {
                SendInteraction(_onlineDetail);
            }
        }
        private void OnDestroy()
        {
            _cts.Cancel();
        }
        void SendInteraction(OnlineSongDetail song)
        {
            _ = SendLikeAsync(song);
        }

        async Task SendLikeAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            if (_isAlreadyThumbUp)
            {
                await UniTask.SwitchToMainThread();
                infotext.text = "THUMBUP_ALREADY".i18n();
                return;
            }
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            token = cts.Token;
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                using (await _thumbUpLock.LockAsync(token))
                {
                    if (_isAlreadyThumbUp)
                    {
                        return;
                    }
                    await UniTask.SwitchToMainThread();
                    infotext.text = "THUMBUP_SENDING".i18n();
                    var intList = await Online.GetChartInteractAsync(song, token);
                    if (intList is MajNetSongInteract interact)
                    {
                        if(interact.IsLiked)
                        {
                            _isAlreadyThumbUp = true;
                            await UniTask.SwitchToMainThread();
                            infotext.text = "THUMBUP_ALREADY".i18n();
                            return;
                        }
                    }
                    else
                    {
                        await UniTask.SwitchToMainThread();
                        infotext.text = "THUMBUP_FAILED".i18n();
                        return;
                    }
                    var rsp = await Online.PostLikeAsync(song, token);
                    await UniTask.SwitchToMainThread();
                    if(rsp.IsSuccessfully)
                    {
                        infotext.text = "THUMBUP_SENDED".i18n();
                        MajInstances.AudioManager.PlaySFX(SFX_LIST[UnityEngine.Random.Range(0, SFX_LIST.Length)]);
                    }
                    else
                    {
                        infotext.text = "THUMBUP_FAILED".i18n();
                    }
                }
            }
        }

        public async Task SendScoreAsync(MaiScore score, CancellationToken token = default)
        {
            if(_onlineDetail is null || _isScorePosted)
            {
                return;
            }
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            token = cts.Token;
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                using (await _sendScoreLock.LockAsync(token))
                {
                    if(_isScorePosted)
                    {
                        return;
                    }
                    await UniTask.SwitchToMainThread();
                    uploadtext.text = "SCORE_SENDING".i18n();
                    await UniTask.SwitchToThreadPool();
                    var rsp = await Online.PostScoreAsync(_onlineDetail, score, token);
                    await UniTask.SwitchToMainThread();

                    if (rsp.IsSuccessfully)
                    {
                        uploadtext.text = "SCORE_SENDED".i18n();
                        _isScorePosted = true;
                    }
                    else
                    {
                        uploadtext.text = "SCORE_FAILED".i18n();
                    }
                }
            }
        }
    }
}