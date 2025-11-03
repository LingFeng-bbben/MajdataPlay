using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using MajdataPlay.Extensions;
using MajdataPlay.Scenes.Game;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Unsafe;
using MajdataPlay.Utils;
using MajSimai;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Practice
{
#nullable enable
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    public class PracticeManager : MonoBehaviour
    {
        public TextMeshProUGUI startTimeText;
        public TextMeshProUGUI endTimeText;
        public ChartAnalyzer chartAnalyzer;
        public RectTransform selectionBox;
        public Text timeText;
        public Text rTimeText;
        public Slider progress;

        const string TIME_STRING = "{0}:{1:00}.{2:000}";

        private float _startTime = 0;
        private float _endTime = 0;
        private float _totalTime = 0;
        float _iterationThrottle = 0f;
        private AudioSampleWrap _audioTrack = AudioSampleWrap.Empty;

        [SerializeField]
        TextMeshProUGUI _playbackSpeedTitle;
        [SerializeField]
        TextMeshPro _playbackSpeedValue;

        private CancellationTokenSource cts = new CancellationTokenSource();

        int _practiceCount = 114514;
        float _step = 0.2f;
        float _playbackSpeed = 1f;

        bool _isInited = false;
        bool _isExited = false;

        GameInfo _gameInfo;
        SimaiFile _simaiFile;
        
        readonly SwitchStatistic[] _buttonStatistics = new SwitchStatistic[12];
        readonly SwitchStatistic[] _sensorStatistics = new SwitchStatistic[33];

        private void Start()
        {
            _gameInfo = Majdata<GameInfo>.Instance!;
            _gameInfo.PracticeCount = _practiceCount;
            _playbackSpeed = MajEnv.Settings.Mod.PlaybackSpeed;
            _playbackSpeedTitle.text = "MAJSETTING_PROPERTY_PlaybackSpeed".i18n();
            _playbackSpeedValue.text = ZString.Format("{0:F2}", _playbackSpeed);
            InitAsync().Forget();
        }
        async UniTaskVoid InitAsync()
        {
            await using (UniTask.ReturnToMainThread())
            {
                await UniTask.SwitchToThreadPool();
                var songinfo = _gameInfo.Charts.FirstOrDefault();
                var level = _gameInfo.Levels.FirstOrDefault();
                await songinfo.PreloadAsync();
                _audioTrack = await songinfo.GetAudioTrackAsync();
                _totalTime = (float)_audioTrack.Length.TotalSeconds;
                _simaiFile = await songinfo.GetMaidataAsync(true);
                var levelIndex = (int)_gameInfo.CurrentLevel;
                var maidata = _simaiFile.Charts[levelIndex].Fumen;

                if (string.IsNullOrEmpty(maidata))
                {
                    await UniTask.SwitchToMainThread();
                    MajInstances.SceneSwitcher.SwitchScene("List", false);
                    return;
                }

                var chart = await SimaiParser.ParseChartAsync(songinfo.Levels[levelIndex], songinfo.Designers[levelIndex], maidata);

                await chartAnalyzer.AnalyzeAndDrawGraphAsync(chart, _totalTime);
                if (_gameInfo.TimeRange is not null)
                {
                    _startTime = (float)_gameInfo.TimeRange.Value.Start;
                    _endTime = (float)_gameInfo.TimeRange.Value.End;

                }
                else
                {
                    _startTime = _simaiFile.Offset;
                    _endTime = _totalTime;
                }
            }
            var bgmSFX = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            if(bgmSFX.IsPlaying)
            {
                bgmSFX.Stop();
            }
            _audioTrack.Play();
            _audioTrack.CurrentSec = _startTime;
            _audioTrack.Volume = MajInstances.Settings.Audio.Volume.BGM;
            LedRing.SetAllLight(Color.white);
            LedRing.SetButtonLight(Color.green, 3);
            LedRing.SetButtonLight(Color.red, 4);
            MajInstances.SceneSwitcher.FadeOut();
            _isInited = true;
        }
        void ButtonStatisticUpdate()
        {
            ReadOnlySpan<ButtonZone> zones = stackalloc ButtonZone[]
            {
                ButtonZone.A1,
                ButtonZone.A2,
                ButtonZone.A3,
                ButtonZone.A4,
                ButtonZone.A5,
                ButtonZone.A6,
                ButtonZone.A7,
                ButtonZone.A8,
                ButtonZone.Test,
                ButtonZone.P1,
                ButtonZone.Service,
                ButtonZone.P2,
            };
            for (var i = 0; i < zones.Length; i++)
            {
                ref readonly var zone = ref zones[i];
                ref var btnStatistic = ref _buttonStatistics[i];
                var isPressed = InputManager.CheckButtonStatusInThisFrame(zone, SwitchStatus.On);

                btnStatistic.IsPressed = isPressed;
                btnStatistic.IsReleased = InputManager.CheckButtonStatusInPreviousFrame(zone, SwitchStatus.On) &&
                                          InputManager.CheckButtonStatusInThisFrame(zone, SwitchStatus.Off);
                btnStatistic.IsClicked = InputManager.IsButtonClickedInThisFrame(zone);
                if (btnStatistic.IsClicked)
                {
                    btnStatistic.IsClickEventUsed = false;
                }
                if (isPressed)
                {
                    btnStatistic.PressTime += MajTimeline.DeltaTime;
                }
                else
                {
                    btnStatistic.PressTime = 0;
                }
            }
        }
        void SensorStatisticUpdate()
        {
            ReadOnlySpan<SensorArea> areas = stackalloc SensorArea[]
            {
                SensorArea.A1,
                SensorArea.A2,
                SensorArea.A3,
                SensorArea.A4,
                SensorArea.A5,
                SensorArea.A6,
                SensorArea.A7,
                SensorArea.A8,
                SensorArea.B1,
                SensorArea.B2,
                SensorArea.B3,
                SensorArea.B4,
                SensorArea.B5,
                SensorArea.B6,
                SensorArea.B7,
                SensorArea.B8,
                SensorArea.C,
                SensorArea.D1,
                SensorArea.D2,
                SensorArea.D3,
                SensorArea.D4,
                SensorArea.D5,
                SensorArea.D6,
                SensorArea.D7,
                SensorArea.D8,
                SensorArea.E1,
                SensorArea.E2,
                SensorArea.E3,
                SensorArea.E4,
                SensorArea.E5,
                SensorArea.E6,
                SensorArea.E7,
                SensorArea.E8,
            };

            for (var i = 0; i < areas.Length; i++)
            {
                ref readonly var area = ref areas[i];
                ref var sensorStatistic = ref _sensorStatistics[i];
                var isPressed = InputManager.CheckSensorStatusInThisFrame(area, SwitchStatus.On);

                sensorStatistic.IsPressed = isPressed;
                sensorStatistic.IsReleased = InputManager.CheckSensorStatusInPreviousFrame(area, SwitchStatus.On) &&
                                          InputManager.CheckSensorStatusInThisFrame(area, SwitchStatus.Off);
                sensorStatistic.IsClicked = InputManager.IsSensorClickedInThisFrame(area);

                if (sensorStatistic.IsClicked)
                {
                    sensorStatistic.IsClickEventUsed = false;
                }
                if (isPressed)
                {
                    sensorStatistic.PressTime += MajTimeline.DeltaTime;
                }
                else
                {
                    sensorStatistic.PressTime = 0;
                }
            }
        }
        void ButtonCheck()
        {
            ref var btnA4Statistic = ref _buttonStatistics[(int)ButtonZone.A4];
            ref var btnA5Statistic = ref _buttonStatistics[(int)ButtonZone.A5];

            if(btnA4Statistic.IsClicked && !btnA4Statistic.IsClickEventUsed)
            {
                btnA4Statistic.IsClickEventUsed = true;
                _isExited = true;
                _gameInfo.TimeRange = new Range<double>(_startTime, _endTime);
                MajEnv.Settings.Mod.PlaybackSpeed = _playbackSpeed;
                MajInstances.SceneSwitcher.SwitchScene("Game", false);
                throw new OperationCanceledException();
            }
            else if(btnA5Statistic.IsClicked && !btnA5Statistic.IsClickEventUsed)
            {
                btnA5Statistic.IsClickEventUsed = true;
                _isExited = true;
                MajInstances.SceneSwitcher.SwitchScene("List", false);
                throw new OperationCanceledException();
            }
        }
        void SensorCheck()
        {
            // Start Time "<"
            ref var e6Statistic = ref _sensorStatistics[(int)SensorArea.E6];
            // Start Time ">"
            ref var b5Statistic = ref _sensorStatistics[(int)SensorArea.B5];
            // End Time "<"
            ref var b4Statistic = ref _sensorStatistics[(int)SensorArea.B4];
            // End Time ">"
            ref var e4Statistic = ref _sensorStatistics[(int)SensorArea.E4];
            //Playback Speed "<"
            ref var e8Statistic = ref _sensorStatistics[(int)SensorArea.E8];
            ref var b7Statistic = ref _sensorStatistics[(int)SensorArea.B7];
            //Playback Speed ">"
            ref var e2Statistic = ref _sensorStatistics[(int)SensorArea.E2];
            ref var b2Statistic = ref _sensorStatistics[(int)SensorArea.B2];

            if(e6Statistic.IsClicked && !e6Statistic.IsClickEventUsed)
            {
                e6Statistic.IsClickEventUsed = true;
                _startTime = Mathf.Clamp(_startTime - 0.2f, 0, _totalTime);
                _audioTrack.CurrentSec = _startTime;
            }
            else if(b5Statistic.IsClicked && !b5Statistic.IsClickEventUsed)
            {
                b5Statistic.IsClickEventUsed = true;
                _startTime = Mathf.Clamp(_startTime + 0.2f, 0, _totalTime);
                _audioTrack.CurrentSec = _startTime;
            }
            else if (b4Statistic.IsClicked && !b4Statistic.IsClickEventUsed)
            {
                b4Statistic.IsClickEventUsed = true;
                _endTime = Mathf.Clamp(_endTime - 0.2f, 0, _totalTime);
                _audioTrack.CurrentSec = _endTime;
            }
            else if (e4Statistic.IsClicked && !e4Statistic.IsClickEventUsed)
            {
                e4Statistic.IsClickEventUsed = true;
                _endTime = Mathf.Clamp(_endTime + 0.2f, 0, _totalTime);
                _audioTrack.CurrentSec = _endTime;
            }

            var needUpdatePBSValue = false;
            if(e8Statistic.IsClicked && !e8Statistic.IsClickEventUsed)
            {
                e8Statistic.IsClickEventUsed = true;
                _playbackSpeed -= 0.01f;
                needUpdatePBSValue = true;
            }
            else if(b7Statistic.IsClicked && !b7Statistic.IsClickEventUsed)
            {
                b7Statistic.IsClickEventUsed = true;
                _playbackSpeed -= 0.01f;
                needUpdatePBSValue = true;
            }
            else if (e2Statistic.IsClicked && !e2Statistic.IsClickEventUsed)
            {
                e2Statistic .IsClickEventUsed = true;
                _playbackSpeed += 0.01f;
                needUpdatePBSValue = true;
            }
            else if (b2Statistic.IsClicked && !b2Statistic.IsClickEventUsed)
            {
                b2Statistic .IsClickEventUsed = true;
                _playbackSpeed += 0.01f;
                needUpdatePBSValue = true;
            }
            var playbackSpeedPressTime = Mathf.Max(Mathf.Max(Mathf.Max(e8Statistic.PressTime, b7Statistic.PressTime), e2Statistic.PressTime), b2Statistic.PressTime);
            
            if(playbackSpeedPressTime >= 0.4f)
            {
                var iterationSpeed = MajEnv.Settings.Debug.MenuOptionIterationSpeed;
                if (_iterationThrottle <= 1f / (iterationSpeed is 0 ? 15 : iterationSpeed))
                {
                    _iterationThrottle += MajTimeline.DeltaTime;
                }
                else
                {
                    var isUp = e2Statistic.IsPressed || b2Statistic.IsPressed;
                    var isDown = e8Statistic.IsPressed || b7Statistic.IsPressed;
                    if(isUp)
                    {
                        _playbackSpeed += 0.01f;
                        needUpdatePBSValue = true;
                    }
                    else if(isDown)
                    {
                        _playbackSpeed -= 0.01f;
                        needUpdatePBSValue = true;
                    }
                }
            }
            else
            {
                _iterationThrottle = 0;
            }
            _playbackSpeed = Mathf.Max(_playbackSpeed , 0.01f);
            if(needUpdatePBSValue)
            {
                _playbackSpeedValue.text = ZString.Format("{0:F2}", _playbackSpeed);
            }
            var pressTime = Mathf.Max(Mathf.Max(Mathf.Max(e6Statistic.PressTime, b5Statistic.PressTime), b4Statistic.PressTime), e4Statistic.PressTime);
            if(pressTime < 0.5f)
            {
                return;
            }
            var ratio = pressTime switch
            {
                > 4 => 128,
                > 3 => 64,
                > 2 => 32,
                > 1 => 16,
                > 0.5f => 8,
                _ => 0
            };
            ref var value = ref Unsafe.NullRef<float>();
            var direction = 0;
            var minValue = 0f;
            var maxValue = 0f;
            if (e6Statistic.IsPressed)
            {
                value = ref _startTime;
                direction = -1;
                maxValue = _endTime;
                minValue = 0;
            }
            else if(b5Statistic.IsPressed)
            {
                value = ref _startTime;
                direction = 1;
                maxValue = _endTime;
                minValue = 0;
            }
            else if(b4Statistic.IsPressed)
            {
                value = ref _endTime;
                direction = -1;
                maxValue = _totalTime;
                minValue = _startTime;
            }
            else if(e4Statistic.IsPressed)
            {
                value = ref _endTime;
                direction = 1;
                maxValue = _totalTime;
                minValue = _startTime;
            }
            else
            {
                return;
            }
            value = (value + _step * MajTimeline.DeltaTime * ratio * direction).Clamp(minValue, maxValue);

            _audioTrack.CurrentSec = value;
            _audioTrack.Play();
        }
        void Update()
        {
            if (!_isInited || _isExited || _audioTrack is null)
            {
                return;
            }
            UpdateSBTextMeshProUGUI();
            ButtonStatisticUpdate();
            SensorStatisticUpdate();
            ButtonCheck();
            SensorCheck();

            var currentSec = _audioTrack.CurrentSec;

            if (currentSec > _endTime)
            {
                _audioTrack.CurrentSec = _startTime;
            }
        }
        void UpdateSBTextMeshProUGUI()
        {
            var start = TimeSpan.FromSeconds(_startTime - _simaiFile.Offset);
            var end = TimeSpan.FromSeconds(_endTime - _simaiFile.Offset);
            var anarect = chartAnalyzer.GetComponent<RectTransform>().rect;
            var x = (_startTime - _simaiFile.Offset) / _totalTime * anarect.width;
            var width = (_endTime - _startTime) / _totalTime * anarect.width;

            startTimeText.text = ZString.Format(TIME_STRING, start.Minutes, start.Seconds, start.Milliseconds);
            endTimeText.text = ZString.Format(TIME_STRING, end.Minutes, end.Seconds, end.Milliseconds);
            selectionBox.sizeDelta = new Vector2((float)width, anarect.height);
            selectionBox.anchoredPosition = new Vector2((float)x, 0);

            var audioLen = _audioTrack.Length;
            var current = TimeSpan.FromSeconds(_audioTrack.CurrentSec - _simaiFile.Offset);
            var remaining = audioLen - current;
            timeText.text = ZString.Format(TIME_STRING, current.Minutes, current.Seconds, current.Milliseconds);
            rTimeText.text = ZString.Format(TIME_STRING, remaining.Minutes, remaining.Seconds, remaining.Milliseconds);
            progress.value = ((float)(current.TotalMilliseconds / audioLen.TotalMilliseconds)).Clamp(0, 1);
        }
        private void OnDestroy()
        {
            cts?.Cancel();
            _audioTrack?.Stop();
            _audioTrack = null;
            _isExited = true;
        }
    }
}