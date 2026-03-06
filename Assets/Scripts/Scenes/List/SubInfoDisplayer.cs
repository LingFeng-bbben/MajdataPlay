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
        public Image ThumbUpImage;
        public Color ThumbUpGoldColor;
        public Color ThumbUpGreenColor;
        CancellationTokenSource _cts = new();

        public async UniTask RefreshContentAsync(ISongDetail detail, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                Hide();
                _cts = new();
                if (detail is OnlineSongDetail onlineDetail)
                {
                    id_text.text = "ID: " + onlineDetail.Id;
                    await UniTask.SwitchToThreadPool();
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token))
                    {
                        token = linkedCts.Token;
                        var (isSuccessfully1, interact) = await GetOnlineInteractionAsync(onlineDetail, token);

                        await UniTask.SwitchToMainThread(token);
                        if (isSuccessfully1)
                        {
                            var totalLikes = (interact.Likes.Length - interact.DisLikeCount);
                            LikeCount.text = totalLikes.ToString();
                            PlayCount.text = interact.Plays.ToString();
                            CommentCount.text = interact.Comments.Length.ToString();

                            foreach (var icon in Icons)
                            {
                                icon.SetActive(true);
                            }
                            CommentBox.SetActive(true);
                            foreach (var comment in interact.Comments)
                            {
                                var text = comment.Sender + "说:\n" + comment.Content + "\n";
                                CommentText.text = text;
                                await UniTask.Delay(5000, cancellationToken: token);
                                token.ThrowIfCancellationRequested();
                            }
                            CommentBox.SetActive(false);
                            if (interact.IsLiked)
                            {
                                ThumbUpImage.color = ThumbUpGreenColor;
                            }
                            else if (totalLikes > 5)
                            {
                                ThumbUpImage.color = ThumbUpGoldColor;
                            }
                            else
                            {
                                ThumbUpImage.color = Color.white;
                            }
                        }
                    }
                }
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