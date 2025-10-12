using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using System.Threading;
using MajdataPlay.Utils;
using System.Threading.Tasks;
using System;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class SubInfoDisplayer : MonoBehaviour
    {
        public TMP_Text id_text;
        public TMP_Text good_text;
        public TMP_Text CommentText;
        public GameObject CommentBox;

        CancellationTokenSource _cts = new();

        // Start is called before the first frame update
        public void RefreshContent(ISongDetail detail)
        {
            if (detail is OnlineSongDetail onlineDetail)
            {
                id_text.text = "ID: " + onlineDetail.Id;
                HideInteraction();
                _cts = new();
                GetOnlineInteraction(onlineDetail, _cts.Token).Forget();
            }
            else
            {
                Hide();
            }
        }
        public void Hide()
        {
            id_text.text = "";
            HideInteraction();
        }

        public void HideInteraction()
        {
            good_text.text = "";
            _cts.Cancel();
            CommentBox.SetActive(false);
        }
        void OnDestroy()
        {
            _cts.Cancel();
        }
        async UniTaskVoid GetOnlineInteraction(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                try
                {
                    await UniTask.SwitchToThreadPool();
                    var interactUrl = song.ServerInfo.Url + "/maichart/" + song.Id + "/interact";
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                    await UniTask.SwitchToMainThread();
                    using var req = UnityWebRequestFactory.Get(interactUrl);
                    var asyncOp = req.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            req.Abort();
                            throw new HttpException(HttpErrorCode.Canceled);
                        }
                        await UniTask.Yield();
                    }
                    if (!req.IsSuccessStatusCode())
                    {
                        HideInteraction();
                        return;
                    }
                    var list = await Serializer.Json.DeserializeAsync<MajNetSongInteract>(req.downloadHandler.text);
#else
                    var client = MajEnv.SharedHttpClient;
                    using var rsp = await client.GetAsync(interactUrl, token);
                    using var intjson = await rsp.Content.ReadAsStreamAsync();
                    var list = await Serializer.Json.DeserializeAsync<MajNetSongInteract>(intjson);
#endif
                    await UniTask.SwitchToMainThread(cancellationToken: token);
                    token.ThrowIfCancellationRequested();
                    good_text.text = "²¥: " + list.Plays + " ÔÞ: " + (list.Likes.Length - list.DisLikeCount) + " ÆÀ: " + list.Comments.Length;

                    CommentBox.SetActive(true);
                    foreach (var comment in list.Comments)
                    {
                        var text = comment.Sender.Username + "Ëµ£º\n" + comment.Content + "\n";
                        CommentText.text = text;
                        await UniTask.Delay(5000, cancellationToken: token);
                        token.ThrowIfCancellationRequested();
                    }
                    CommentBox.SetActive(false);
                }
                catch (Exception ex)
                {
                    if(ex is HttpException e)
                    {
                        if(e.ErrorCode != HttpErrorCode.Canceled)
                        {
                            MajDebug.LogException(ex);
                        }
                    }
                    else if(ex is not OperationCanceledException)
                    {
                        MajDebug.LogException(ex);
                    }
                    await UniTask.SwitchToMainThread();
                    if (!token.IsCancellationRequested)
                    {
                        HideInteraction();
                    }
                }
            } 
        }
    }
}