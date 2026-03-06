using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace MajdataPlay
{
    public class RankingDisplayer : MonoBehaviour
    {
        public TMP_Text[] PlayerNames;
        public TMP_Text[] Scores;
        public string[] NameTemplates;
        public string ScoresTemplate;

        CancellationTokenSource _cts = new();

        // Start is called before the first frame update
        void Start()
        {
            Hide();
        }

        public async Task SetSongScoreRanking(ISongDetail detail,ChartLevel selectedLevel, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                if (!_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                    _cts = new();
                }
                Hide();
                if (detail is OnlineSongDetail onlineDetail)
                {
                    await UniTask.SwitchToThreadPool();
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token))
                    {
                        token = linkedCts.Token;
                        var (isSuccessfully, scoreInfo) = await GetOnlineScoresAsync(onlineDetail, token);
                        await UniTask.SwitchToMainThread(token);
                        if (isSuccessfully)
                        {
                            SetScores(scoreInfo.Scores?[(int)selectedLevel] ?? Array.Empty<MajNetSongScore>());
                        }
                    }
                }
            }
        }

        public void Hide()
        {
            var childs = transform.GetChildren();
            foreach (var child in childs) { 
                child.gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            var childs = transform.GetChildren();
            foreach (var child in childs)
            {
                child.gameObject.SetActive(true);
            }
        }

        private void SetScores(ReadOnlySpan<MajNetSongScore> scores)
        {
            if(scores.IsEmpty)
            {
                Hide();
                return;
            }
            Show();

            for (var i = 0; i < PlayerNames.Length; i++)
            {
                PlayerNames[i].text = string.Empty;
                Scores[i].text = string.Empty;
            }

            for (var i = 0; i < scores.Length && i < 3; i++)
            {
                ref readonly var score = ref scores[i];
                PlayerNames[i].text = string.Format(NameTemplates[i], score.Player.Username);
                var @int = MathF.Truncate(score.Acc);
                var @float = (int)((score.Acc - @int) * 1000);
                var comboState = CombostateToStr(score.ComboState);
                Scores[i].text = string.Format(ScoresTemplate, @int, @float, comboState);
            }
        }

        private string CombostateToStr(ComboState cs)
        {
            if (cs == ComboState.APPlus) {
                return "<color=#FFF808>AP<sup>+</sup></color>";
            }
            else if (cs == ComboState.FCPlus)
            {
                return "<color=#72FD59>FC<sup>+</sup></color>";
            }
            else if (cs == ComboState.AP)
            {
                return "<color=#FFF808>AP</color>";
            }
            else if (cs == ComboState.FC)
            {
                return "<color=#72FD59>FC</color>";
            }
            else
            {
                return string.Empty;
            }
        }

        private async UniTask<(bool IsSuccessfully, MajNetSongScoreInfo ScoreInfo)> GetOnlineScoresAsync(OnlineSongDetail song, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                try
                {
                    await UniTask.SwitchToThreadPool();
                    var scoreInfo = await Online.GetChartScoreInfoAsync(song, token);
                    token.ThrowIfCancellationRequested();
                    if (scoreInfo is null)
                    {
                        return (false, default);
                    }

                    return (true, (MajNetSongScoreInfo)scoreInfo);
                }
                catch (Exception ex)
                {
                    if (ex is HttpException e)
                    {
                        if (e.ErrorCode != HttpErrorCode.Canceled)
                        {
                            MajDebug.LogException(ex);
                        }
                    }
                    else if (ex is not OperationCanceledException)
                    {
                        MajDebug.LogException(ex);
                    }
                }
                return (false, default);
            }
        }
    }
}
