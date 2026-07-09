using Cysharp.Threading.Tasks;
using MajdataPlay.Drawing;
using MajdataPlay.IO;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Settings;
using MajdataPlay.Settings.Runtime;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CenterCoverDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("levelRingDisplayer")]
        Image _levelRingDisplayer;
        [SerializeField]
        [FormerlySerializedAs("songCoverDisplayer")]
        Image _songCoverDisplayer;

        [SerializeField]
        [FormerlySerializedAs("loadingObj")]
        GameObject _loadingObj;

        [SerializeField]
        [FormerlySerializedAs("diffColors")]
        Color[] _diffColors = new Color[6];

        [SerializeField]
        [FormerlySerializedAs("scoreDisplayer")]
        MaiScoreDisplayer _scoreDisplayer;

        [SerializeField]
        [FormerlySerializedAs("metadataDisplayer")]
        ChartMetadataDisplayer _metadataDisplayer;

        [SerializeField]
        [FormerlySerializedAs("chartAnalyzer")]
        ChartVisualDisplayer _chartAnalyzer;

        [SerializeField]
        [FormerlySerializedAs("onlineInfoDisplayer")]
        OnlineInfoDisplayer _onlineInfoDisplayer;

        [SerializeField]
        [FormerlySerializedAs("bgSongCoverDisplayer")]
        Image _bgSongCoverDisplayer;

        [SerializeField]
        [FormerlySerializedAs("levelDisplayerListRoot")]
        GameObject _levelDisplayerListRoot;

        [SerializeField]
        [FormerlySerializedAs("selectedLevelTitle")]
        TextMeshProUGUI _selectedLevelTitle;

        [SerializeField]
        [FormerlySerializedAs("selectedLevelText")]
        TextMeshProUGUI _selectedLevelText;

        [SerializeField]
        [FormerlySerializedAs("selectedLevelColor")]
        Image _selectedLevelColor;

        int _diff = 0;

        ISongDetail? _currentSongDetail = null;

        CancellationTokenSource? _cts = null;
        
        ListManager _listManager;

        LevelBinding[] _levelBindings = Array.Empty<LevelBinding>();
        LevelDisplayer[] _levelDisplayers = Array.Empty<LevelDisplayer>();

        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();
        readonly ChartLevel[] _levelValues = (ChartLevel[])Enum.GetValues(typeof(ChartLevel));

        const float LEVEL_RING_LEFT_START_ROTATION = 67.5f;
        const float LEVEL_RING_RIGHT_START_ROTATION = 32.5f;
        const float LEVEL_RING_ROTATION_STEP = 10f;
        
        void Awake()
        {
            var levelDisplayerListRoot = _levelDisplayerListRoot.transform;
            var displayerCount = levelDisplayerListRoot.childCount;
            _levelDisplayers = new LevelDisplayer[displayerCount];
            for (var i = 0; i < displayerCount; i++)
            {
                var child = levelDisplayerListRoot.GetChild(i);
                var textTransform = child.Find("Text");
                if (textTransform == null)
                {
                    throw new Exception($"Level displayer {i} does not have a child named 'Text'.");
                }
                var textDisplayer = textTransform.GetComponent<TextMeshProUGUI>();
                if (textDisplayer == null)
                {
                    throw new Exception($"Level displayer {i}'s 'Text' child does not have a TextMeshProUGUI component.");
                }
                _levelDisplayers[i] = new LevelDisplayer
                {
                    Object = child.gameObject,
                    Transform = child,
                    TextTransform = textTransform,
                    TextDisplayer = textDisplayer
                };
            }
            var levelValues = (ChartLevel[])Enum.GetValues(typeof(ChartLevel));
            _levelBindings = new LevelBinding[levelValues.Length];
            for (var i = 0; i < levelValues.Length; i++)
            {
                _levelBindings[i] = new LevelBinding
                {
                    Level = levelValues[i],
                    IsLevelEmpty = true
                };
            }
            SetDifficulty((int)_listConfig.SelectedDiff);
        }
        void Start()
        {
            _listManager = Majdata<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            _cts?.Cancel();
        }

        public void SetDifficulty(int i)
        {
            _levelRingDisplayer.color = _diffColors[i];
            _selectedLevelColor.color = _diffColors[i];
            _diff = i;
            if (i + 1 < _diffColors.Length)
            {
                CabinetLed.SetButtonLight(_diffColors[i + 1], 0);
            }
            else
            {
                CabinetLed.SetButtonLight(_diffColors.First(), 0);
            }
            if (i - 1 >= 0)
            {
                CabinetLed.SetButtonLight(_diffColors[i - 1], 7);
            }
            else
            {
                CabinetLed.SetButtonLight(_diffColors.Last(), 7);
            }
            UpdateLevelRing();
            UpdateMetadataAndScoreDisplayer();            
        }
        public void SetSongDetail(ISongDetail detail)
        {
            if(detail == _currentSongDetail)
            {
                return;
            }
            else if(_cts is not null)
            {
                _cts.Cancel();
            }
            _currentSongDetail = detail;
            _cts = new();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_listManager.CancellationToken, _cts.Token);
            var chartLevels = detail.Levels;
            for (var i = 0; i < chartLevels.Length; i++)
            {
                var level = chartLevels[i];
                var displayer = _levelDisplayers[i];
                ref var binding = ref _levelBindings[i];
                if (string.IsNullOrEmpty(level))
                {
                    binding.IsLevelEmpty = true;
                    binding.Displayer = null;
                    binding.Value = null;
                    displayer.Object.SetActive(false);
                    continue;
                }
                binding.IsLevelEmpty = false;
                binding.Value = level;
                binding.Displayer = displayer;
                displayer.TextDisplayer.text = level;
                displayer.Object.SetActive(true);
            }
            UpdateLevelRing();
            UpdateMetadataAndScoreDisplayer();
            ListManager.AllBackgroundTasks.Add(SetCoverAsync(detail, linkedCts.Token));
        }

        void UpdateLevelRing()
        {
            var currentLevel = (ChartLevel)_diff;
            var currentLevelBinding = _levelBindings[_diff];
            var leftIndex = 0;
            var rightIndex = 0;

            _selectedLevelText.text = currentLevelBinding.Value ?? "-";
            switch(currentLevel)
            {
                case ChartLevel.Easy:
                    _selectedLevelTitle.text = "Easy";
                    break;
                case ChartLevel.Basic:
                    _selectedLevelTitle.text = "Basic";
                    break;
                case ChartLevel.Advance:
                    _selectedLevelTitle.text = "Advance";
                    break;
                case ChartLevel.Expert:
                    _selectedLevelTitle.text = "Expert";
                    break;
                case ChartLevel.Master:
                    _selectedLevelTitle.text = "Master";
                    break;
                case ChartLevel.ReMaster:
                    _selectedLevelTitle.text = "Re:Master";
                    break;
                case ChartLevel.UTAGE:
                    _selectedLevelTitle.text = "UTAGE";
                    break;
            }
            for (var i = _diff - 1; i >= 0; i--)
            {
                var binding = _levelBindings[i];
                if (binding.IsLevelEmpty)
                {
                    continue;
                }
                if(binding.Displayer is LevelDisplayer displayer)
                {
                    displayer.Transform.localRotation = Quaternion.Euler(0, 0, LEVEL_RING_LEFT_START_ROTATION + (leftIndex * LEVEL_RING_ROTATION_STEP));
                    displayer.TextTransform.localRotation = Quaternion.Euler(0, 0, -(LEVEL_RING_LEFT_START_ROTATION + (leftIndex * LEVEL_RING_ROTATION_STEP)));
                    leftIndex++;
                }
            }
            if (!currentLevelBinding.IsLevelEmpty)
            {
                if (currentLevelBinding.Displayer is LevelDisplayer displayer)
                {
                    displayer.Transform.localRotation = Quaternion.Euler(0, 0, 50);
                    displayer.TextTransform.localRotation = Quaternion.Euler(0, 0, -50);
                }
            }
            for (var i = _diff + 1; i < _levelBindings.Length; i++)
            {
                var binding = _levelBindings[i];
                if (binding.IsLevelEmpty)
                {
                    continue;
                }
                if (binding.Displayer is LevelDisplayer displayer)
                {
                    displayer.Transform.localRotation = Quaternion.Euler(0, 0, LEVEL_RING_RIGHT_START_ROTATION - (rightIndex * LEVEL_RING_ROTATION_STEP));
                    displayer.TextTransform.localRotation = Quaternion.Euler(0, 0, -(LEVEL_RING_RIGHT_START_ROTATION - (rightIndex * LEVEL_RING_ROTATION_STEP)));
                    rightIndex++;
                }
            }
        }
        void UpdateMetadataAndScoreDisplayer()
        {
            if(_currentSongDetail is null)
            {
                return;
            }
            var cancellationToken = _cts?.Token ?? default;
            _metadataDisplayer.SetMetadataFromSongDetail(_currentSongDetail, (ChartLevel)_diff, cancellationToken);
            _scoreDisplayer.SetScore(_currentSongDetail, (ChartLevel)_diff);
            _onlineInfoDisplayer.SetSongDetail(_currentSongDetail, cancellationToken);
            _chartAnalyzer.SetSongDeatil(_currentSongDetail, (ChartLevel)_diff, null, cancellationToken);
        }
        
        async Task SetCoverAsync(ISongDetail detail, CancellationToken token = default)
        {
            _loadingObj.SetActive(true);
            _songCoverDisplayer.sprite = SpriteLoader.EmptySprite;
            var cover = await detail.GetCoverAsync(true, token: token);
            await UniTask.SwitchToMainThread();
            token.ThrowIfCancellationRequested();
            _songCoverDisplayer.sprite = cover;
            _loadingObj.SetActive(false);
        }

        readonly struct LevelDisplayer
        {
            public required GameObject Object { get; init; }
            public required Transform Transform { get; init; }
            public required Transform TextTransform { get; init; }
            public required TextMeshProUGUI TextDisplayer { get; init; }
        }
        struct LevelBinding
        {
            public required ChartLevel Level { get; init; }
            public bool IsLevelEmpty { get; set; }
            public string? Value { get; set; }
            public LevelDisplayer? Displayer { get; set; }
        }
    }
}