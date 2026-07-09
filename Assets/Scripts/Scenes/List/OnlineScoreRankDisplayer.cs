using LitMotion;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class OnlineScoreRankDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("rankerDisplayerListRoot")]
        GameObject _rankerDisplayerListRoot;

        [SerializeField]
        [FormerlySerializedAs("rankDisplayer")]
        TextMeshProUGUI _rankDisplayer;

        [SerializeField]
        [FormerlySerializedAs("loadingIndicator")]
        GameObject _loadingIndicator;

        [SerializeField]
        [FormerlySerializedAs("emptyIndicator")]
        GameObject _emptyIndicator;

        RankerDisplayer[] _rankerDisplayers = Array.Empty<RankerDisplayer>();

        float _loadTimer = 0f;

        OnlineSongDetail? _currentSongDetail;
        ChartLevel _currentLevel;

        Task<MajNetSongScoreInfo?>? _onlineScoreFetchTask;

        CancellationToken _cancellationToken;

        bool _isIn = false;
        MotionHandle _displayerAnim;
        RectTransform _rectTransform;

        const float LOAD_DEBOUNCE_INTERVAL_SEC = 0.4f;
        const float DISPLAYER_ANIM_DURATION_SEC = 0.3f;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            var rankerDisplayerListRoot = _rankerDisplayerListRoot.transform;
            var displayerCount = rankerDisplayerListRoot.childCount;
            _rankerDisplayers = new RankerDisplayer[displayerCount];
            for (var i = 0; i < displayerCount; i++)
            {
                var displayer = rankerDisplayerListRoot.GetChild(i);
                _rankerDisplayers[i] = new()
                {
                    Object = displayer.gameObject,
                    AvatarDisplayer = displayer.Find("Avatar").GetComponent<Image>(),
                    RankDisplayer = displayer.Find("Rank").GetComponent<TextMeshProUGUI>(),
                    UsernameDisplayer = displayer.Find("Username").GetComponent<TextMeshProUGUI>(),
                    AccurateDisplayer = displayer.Find("Accurate").GetComponent<TextMeshProUGUI>()
                };
            }
        }

        void LateUpdate()
        {
            if (_currentSongDetail is null)
            {
                return;
            }
            else if (_loadTimer < LOAD_DEBOUNCE_INTERVAL_SEC)
            {
                _loadTimer += MajTimeline.DeltaTime;
                return;
            }
            else if (_onlineScoreFetchTask is null)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    _currentSongDetail = null;
                    _loadTimer = 0f;
                    return;
                }
                _onlineScoreFetchTask = Online.GetChartScoreInfoAsync(_currentSongDetail, _cancellationToken).AsTask();
                ListManager.AllBackgroundTasks.Add(_onlineScoreFetchTask);
                return;
            }
            else if (!_onlineScoreFetchTask.IsCompleted)
            {
                return;
            }
            try
            {
                if (_onlineScoreFetchTask.IsCompletedSuccessfully)
                {
                    if(_onlineScoreFetchTask.Result is not MajNetSongScoreInfo scoreInfo)
                    {
                        return;
                    }
                    DisplayRanker(scoreInfo.Scores[(int)_currentLevel]);
                }
                else
                {
                    MajDebug.LogException(_onlineScoreFetchTask.Exception);
                }
            }
            finally
            {
                _currentSongDetail = null;
                _onlineScoreFetchTask = null;
                _loadTimer = 0f;
            }
        }
        void DisplayRanker(MajNetSongScore[] scores)
        {
            if(scores.Length == 0)
            {
                SetEmpty();
                return;
            }
            _loadingIndicator.SetActive(false);
            _emptyIndicator.SetActive(false);
            var rankerIndex = 0;
            foreach(var scoreInfo in scores.OrderByDescending(x => x.Acc))
            {
                if(rankerIndex >= _rankerDisplayers.Length)
                {
                    break;
                }
                var displayer = _rankerDisplayers[rankerIndex++];
                var playerInfo = scoreInfo.Player;
                displayer.Object.SetActive(true);
                displayer.UsernameDisplayer.text = playerInfo.Username;
                displayer.AccurateDisplayer.text = $"{(int)scoreInfo.Acc}.<size=70%>{(int)((scoreInfo.Acc - MathF.Truncate(scoreInfo.Acc)) * 1000)}%";
            }
        }
        void SetLoading()
        {
            _loadingIndicator.SetActive(true);
            _emptyIndicator.SetActive(false);
        }
        void SetEmpty()
        {
            _emptyIndicator.SetActive(true);
            _loadingIndicator.SetActive(false);
        }

        public void SetSongDetail(ISongDetail songDetail, ChartLevel level, CancellationToken token = default)
        {
            if(songDetail is not OnlineSongDetail onlineSongDetail)
            {
                if(_isIn)
                {
                    _isIn = false;
                    _displayerAnim.TryCancel();
                    _displayerAnim = LMotion.Create(0f, 1f, DISPLAYER_ANIM_DURATION_SEC)
                                            .WithEase(Ease.OutQuad)
                                            .Bind(x =>
                                            {
                                                const float X_POS_START_AT = 260.92f;
                                                const float X_POS_END_AT = 745f;

                                                var nP = Vector2.Lerp(new Vector2(X_POS_START_AT, 75.053f), new Vector2(X_POS_END_AT, 75.053f), x);
                                                _rectTransform.anchoredPosition = nP;
                                            });
                }
                return;
            }
            else
            {
                if (!_isIn)
                {
                    _isIn = true;
                    _displayerAnim.TryCancel();
                    _displayerAnim = LMotion.Create(1f, 0f, DISPLAYER_ANIM_DURATION_SEC)
                                            .WithEase(Ease.OutQuad)
                                            .Bind(x =>
                                            {
                                                const float X_POS_START_AT = 260.92f;
                                                const float X_POS_END_AT = 745f;

                                                var nP = Vector2.Lerp(new Vector2(X_POS_START_AT, 75.053f), new Vector2(X_POS_END_AT, 75.053f), x);
                                                _rectTransform.anchoredPosition = nP;
                                            });
                }
            }
            
            SetLoading();
            HideAllDisplayer();
            _cancellationToken = token;
            _currentSongDetail = onlineSongDetail;
            _currentLevel = level;
            _onlineScoreFetchTask = null;
            _loadTimer = 0f;
        }

        void HideAllDisplayer()
        {
            for (var i = 0; i < _rankerDisplayers.Length; i++)
            {
                var displayer = _rankerDisplayers[i];
                displayer.Object.SetActive(false);
            }
        }

        readonly struct RankerDisplayer
        {
            public required GameObject Object { get; init; }
            public required Image AvatarDisplayer { get; init; }
            public required TextMeshProUGUI RankDisplayer { get; init; }
            public required TextMeshProUGUI UsernameDisplayer { get; init; }
            public required TextMeshProUGUI AccurateDisplayer { get; init; }
        }
    }
}
