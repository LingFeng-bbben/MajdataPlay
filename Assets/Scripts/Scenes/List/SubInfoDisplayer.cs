using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class SubInfoDisplayer : MonoBehaviour
    {
        public TMP_Text id_text;
        public TMP_Text LikeCount;
        public TMP_Text PlayCount;
        public TMP_Text CommentCount;
        public TMP_Text CommentText;
        public GameObject CommentBox;
        public GameObject[] Icons;


        CancellationTokenSource _cts = new();

        public async UniTask RefreshContentAsync(ISongDetail detail, CancellationToken token = default)
        {
            if (detail is OnlineSongDetail onlineDetail)
            {
                id_text.text = "ID: " + onlineDetail.Id;
                HideInteraction();
                _cts = new();
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token))
                {
                    token = linkedCts.Token;
                    var (isSuccessfully1, interact) = await GetOnlineInteractionAsync(onlineDetail, token);
                    
                    await UniTask.SwitchToMainThread();
                    var task1 = UniTask.CompletedTask;
                    if (isSuccessfully1)
                    {
                        task1 = UniTask.Create(async () =>
                        {
                            LikeCount.text = (interact.Likes.Length - interact.DisLikeCount).ToString();
                            PlayCount.text = interact.Plays.ToString();
                            CommentCount.text = interact.Comments.Length.ToString();
                            //interact.IsLiked
                            foreach (var icon in Icons)
                            {
                                icon.SetActive(true);
                            }
                            CommentBox.SetActive(true);
                            foreach (var comment in interact.Comments)
                            {
                                var text = comment.Sender + "หตฃบ\n" + comment.Content + "\n";
                                CommentText.text = text;
                                await UniTask.Delay(5000, cancellationToken: token);
                                token.ThrowIfCancellationRequested();
                            }
                            CommentBox.SetActive(false);
                        });
                    }
                    await UniTask.WhenAll(task1);
                }
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
            LikeCount.text = "";
            PlayCount.text = "";
            CommentCount.text = "";
            _cts.Cancel();
            CommentBox.SetActive(false);
            foreach( var icon in Icons)
            {
                icon.SetActive(false);
            }
        }
        void OnDestroy()
        {
            _cts.Cancel();
        }
        async UniTask<(bool IsSuccessfully, MajNetSongInteract Interact)> GetOnlineInteractionAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                try
                {
                    await UniTask.SwitchToThreadPool();
                    var interact = await Online.GetChartInteractAsync(song, token);
                    token.ThrowIfCancellationRequested();
                    if (interact is null)
                    {
                        return (false, default);
                    }

                    return (true, (MajNetSongInteract)interact);
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
                }
                return (false, default);
            } 
        }
        
    }
}