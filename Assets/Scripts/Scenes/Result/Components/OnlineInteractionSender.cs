using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Scenes.Result.Components.Indicators;
using MajdataPlay.Utils;
using Nito.AsyncEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Result.Components
{
    public class OnlineInteractionSender : MonoBehaviour
    {
        public TextMeshProUGUI infotext;
        public Image thumb;

        [SerializeField]
        [FormerlySerializedAs("scoreUploadIndicator")]
        ScoreUploadIndicator _scoreUploadIndicator;

        OnlineSongDetail? _onlineDetail;
        MaiScore? _score;

        bool _isLocalOrGuest = false;
        bool _isInited = false;
        bool _isThumbUpRequested = false;
        bool _isAlreadyThumbUp = false;
        bool _isScorePosted = false;

        readonly AsyncLock _sendScoreLock = new();
        readonly AsyncLock _thumbUpLock = new();
        readonly CancellationTokenSource _cts = new();
        readonly string[] SFX_LIST = new string[] { "dianzan_comment.wav", "dianzan_comment_2.wav", "dianzan_comment_3.wav" };

        public void Init(ISongDetail song, MaiScore score)
        {
            if (song is not OnlineSongDetail onlineDetail)
            {
                infotext.text = "";
                thumb.gameObject.SetActive(false);
                _isLocalOrGuest = true;
                return;
            }
                
            var serverInfo = onlineDetail.ServerInfo;
            if (serverInfo is null || serverInfo.RuntimeConfig.AuthMethod == NetAuthMethodOption.None)
            {
                infotext.text = "";
                thumb.gameObject.SetActive(false);
                _isLocalOrGuest = true;
                return;
            }
            _isInited = true;
            _score = score;
            _onlineDetail = onlineDetail;
            infotext.text = "MAJTEXT_THUMBUP_INFO".i18n();
        }
        void Update()
        {
            if(!_isInited || _isThumbUpRequested || _onlineDetail is null || _isLocalOrGuest)
            {
                return;
            }
            if(!_isAlreadyThumbUp && 
                (InputManager.IsSensorClickedInThisFrame(SensorArea.E3) || 
                InputManager.IsSensorClickedInThisFrame(SensorArea.B3))
                )
            {
                _ = SendLikeAsync();
            }
            if (!_isScorePosted && !MajInstances.GameManager.Settings.Mod.IsAnyModActive() && 
                (InputManager.IsSensorClickedInThisFrame(SensorArea.E4) || 
                InputManager.IsSensorClickedInThisFrame(SensorArea.D4) || 
                InputManager.IsSensorClickedInThisFrame(SensorArea.A3))
                )
            {
                _ = SendScoreAsync();
            }
        }
        private void OnDestroy()
        {
            _cts.Cancel();
        }

        async Task SendLikeAsync(CancellationToken token = default)
        {
            if (_onlineDetail is null || _isAlreadyThumbUp)
            {
                await UniTask.SwitchToMainThread();
                infotext.text = "MAJTEXT_THUMBUP_ALREADY".i18n();
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
                    infotext.text = "MAJTEXT_THUMBUP_SENDING".i18n();
                    var intList = await Online.GetChartInteractAsync(_onlineDetail, token);
                    if (intList is MajNetSongInteract interact)
                    {
                        if(interact.IsLiked)
                        {
                            _isAlreadyThumbUp = true;
                            await UniTask.SwitchToMainThread();
                            infotext.text = "MAJTEXT_THUMBUP_ALREADY".i18n();
                            return;
                        }
                    }
                    else
                    {
                        await UniTask.SwitchToMainThread();
                        infotext.text = "MAJTEXT_THUMBUP_FAILED".i18n();
                        return;
                    }
                    var rsp = await Online.PostLikeAsync(_onlineDetail, token);
                    await UniTask.SwitchToMainThread();
                    if(rsp.IsSuccessfully)
                    {
                        infotext.text = "MAJTEXT_THUMBUP_SENDED".i18n();
                        MajInstances.AudioManager.PlaySFX(SFX_LIST[UnityEngine.Random.Range(0, SFX_LIST.Length)]);
                    }
                    else
                    {
                        if (rsp.StatusCode is HttpStatusCode.Unauthorized)
                        {
                            infotext.text = "MAJTEXT_LOGIN_SESSION_EXPIRED".i18n();
                        }
                        else
                        {
                            infotext.text = "MAJTEXT_THUMBUP_FAILED".i18n();
                        }
                    }
                }
            }
        }
        public async Task SendScoreAsync(CancellationToken token = default)
        {
            if(!_isInited || _onlineDetail is null || _score is null || _isScorePosted || _isLocalOrGuest)
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
                    _scoreUploadIndicator.SetUploading();
                    _scoreUploadIndicator.SetText("MAJTEXT_SCORE_SENDING".i18n());
                    await UniTask.SwitchToThreadPool();
                    var rsp = await Online.PostScoreAsync(_onlineDetail, _score, token);
                    await UniTask.SwitchToMainThread();

                    if (rsp.IsSuccessfully)
                    {
                        _scoreUploadIndicator.SetText("MAJTEXT_SCORE_SENDED".i18n());
                        _scoreUploadIndicator.SetSuccess();
                        _isScorePosted = true;
                    }
                    else
                    {
                        _scoreUploadIndicator.SetError();
                        if(rsp.StatusCode is HttpStatusCode.Unauthorized)
                        {
                            _scoreUploadIndicator.SetText("MAJTEXT_LOGIN_SESSION_EXPIRED".i18n());
                        }
                        else
                        {
                            _scoreUploadIndicator.SetText("MAJTEXT_SCORE_FAILED".i18n());
                        }
                    }
                }
            }
        }
    }
}