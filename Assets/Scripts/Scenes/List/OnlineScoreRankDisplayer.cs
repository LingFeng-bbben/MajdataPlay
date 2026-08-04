using LitMotion;
using MajdataPlay.Diagnostics;
using MajdataPlay.Net;
using MajdataPlay.Scenes.Game;
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

        [SerializeField]
        [FormerlySerializedAs("rankerRankColors")]
        Color[] _rankerRankColors = Array.Empty<Color>();

        [SerializeField]
        [FormerlySerializedAs("apColor")]
        Color _apColor = Color.white;

        [SerializeField]
        [FormerlySerializedAs("fcColor")]
        Color _fcColor = Color.white;

        RankerDisplayer[] _rankerDisplayers = Array.Empty<RankerDisplayer>();

        float _loadTimer = 0f;
        float _loadDelayTimer = 0f;

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
                _loadDelayTimer -= MajTimeline.DeltaTime;
                return;
            }
            else if(_loadDelayTimer > 0)
            {
                _loadDelayTimer -= MajTimeline.DeltaTime;
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
            var apiEndpoint = _currentSongDetail!.ServerInfo;
            var apiRuntimeInfo = apiEndpoint.RuntimeConfig;
            var selfUsername = string.Empty;
            if(apiRuntimeInfo.IsLoggedIn)
            {
                selfUsername = apiRuntimeInfo.Username;
                _rankDisplayer.text = "--";
            }
            foreach(var scoreInfo in scores.OrderByDescending(x => x.Acc))
            {
                try
                {
                    var playerInfo = scoreInfo.Player;
                    if (rankerIndex >= _rankerDisplayers.Length)
                    {
                        if (string.IsNullOrEmpty(selfUsername))
                        {
                            break;
                        }
                        else
                        {
                            if (playerInfo.Username == selfUsername)
                            {
                                _rankDisplayer.text = $"#{rankerIndex + 1}";
                                break;
                            }
                        }
                        continue;
                    }
                    var displayer = _rankerDisplayers[rankerIndex];
                    displayer.Object.SetActive(true);
                    displayer.UsernameDisplayer.text = playerInfo.Username;
                    displayer.RankDisplayer.color = _rankerRankColors[rankerIndex];
                    var combostate = CombostateToStr(scoreInfo.ComboState);
                    displayer.AccurateDisplayer.text = $"{combostate}<pos=30%>{(int)scoreInfo.Acc}.<size=70%>{(int)((scoreInfo.Acc - MathF.Truncate(scoreInfo.Acc)) * 10000):D4}%";
                }
                finally
                {
                    rankerIndex++;
                }
            }
        }

        private string CombostateToStr(ComboState cs)
        {
            var apcolorstr = "#" + ColorUtility.ToHtmlStringRGB(_apColor);
            var fccolorstr = "#" + ColorUtility.ToHtmlStringRGB(_fcColor);
            if (cs == ComboState.APPlus)
            {
                return $"<color={apcolorstr}>AP<sup>+</sup></color>";
            }
            else if (cs == ComboState.FCPlus)
            {
                return $"<color={fccolorstr}>FC<sup>+</sup></color>";
            }
            else if (cs == ComboState.AP)
            {
                return $"<color={apcolorstr}>AP</color>";
            }
            else if (cs == ComboState.FC)
            {
                return $"<color={fccolorstr}>FC</color>";
            }
            else
            {
                return string.Empty;
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

        public void SetSongDetail(ISongDetail songDetail, ChartLevel level, int loadDelayMS = 0, CancellationToken token = default)
        {
            if(songDetail is not OnlineSongDetail onlineSongDetail)
            {
                Hide();
                return;
            }
            else
            {
                Show();
            }
            
            SetLoading();
            HideAllDisplayer();
            _cancellationToken = token;
            _currentSongDetail = onlineSongDetail;
            _currentLevel = level;
            _onlineScoreFetchTask = null;
            _rankDisplayer.text = string.Empty;
            _loadTimer = 0f;
            _loadDelayTimer = loadDelayMS / 1000f;
        }
        public void Hide()
        {
            if (_isIn)
            {
                _isIn = false;
                _displayerAnim.TryCancel();
                _displayerAnim = LMotion.Create(0f, 1f, DISPLAYER_ANIM_DURATION_SEC)
                                        .WithEase(Ease.OutQuad)
                                        .Bind(x =>
                                        {
                                            var subDisplayerScale = MajEnv.Settings.Display.SubDisplayScale;
                                            if (subDisplayerScale == 0)
                                            {
                                                return;
                                            }
                                            const float START = 290f;
                                            const float DISTANCE = 800f - START;
                                            const float MAX_OFFSET = 80f;

                                            var offset = MAX_OFFSET * ((1f / subDisplayerScale) - 1f);
                                            var end = START + (DISTANCE / subDisplayerScale) + offset;

                                            var pos = Vector2.Lerp(
                                                                new Vector2(START, 75.053f),
                                                                new Vector2(end, 75.053f),
                                                                x);

                                            _rectTransform.anchoredPosition = pos;
                                        });
            }
        }
        public void Show()
        {
            if (!_isIn)
            {
                _isIn = true;
                _displayerAnim.TryCancel();
                _displayerAnim = LMotion.Create(1f, 0f, DISPLAYER_ANIM_DURATION_SEC)
                                        .WithEase(Ease.OutQuad)
                                        .Bind(x =>
                                        {
                                            var subDisplayerScale = MajEnv.Settings.Display.SubDisplayScale;
                                            if (subDisplayerScale == 0)
                                            {
                                                return;
                                            }
                                            const float START = 290f;
                                            const float DISTANCE = 800f - START;
                                            const float MAX_OFFSET = 80f;

                                            var offset = MAX_OFFSET * ((1f / subDisplayerScale) - 1f);
                                            var end = START + (DISTANCE / subDisplayerScale) + offset;

                                            var pos = Vector2.Lerp(
                                                                new Vector2(START, 75.053f),
                                                                new Vector2(end, 75.053f),
                                                                x);

                                            _rectTransform.anchoredPosition = pos;
                                        });
            }
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
