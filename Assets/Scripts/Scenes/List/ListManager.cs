using Cysharp.Threading.Tasks;
using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Recording;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace MajdataPlay.List
{
    public class ListManager : MonoBehaviour
    {
        public CancellationToken CancellationToken => _cts.Token;
        public static List<UniTask> AllBackgroundTasks { get; } = new(8192);

        int _delta = 0;
        float _pressTime = 0;
        bool _isPressed = false;

        // Update control
        bool _isInited = false;
        bool _isExited = false;

        static ReadOnlyMemory<string> _xxlbVoices = new string[]
        {
            "notouch.wav",
            "notouch_2.wav",
            "notouch_3.wav",
            "notouch_4.wav",
            "notouch_5.wav"
        };
        CoverListDisplayer _coverListDisplayer;
        readonly CancellationTokenSource _cts = new();

        void Awake()
        {
            Majdata<ListManager>.Instance = this;
            AllBackgroundTasks.Clear();
        }
        void Start()
        {
            _coverListDisplayer = Majdata<CoverListDisplayer>.Instance!;
            InitializeCoverListAsync().Forget();
            var selectsfx = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            if (!selectsfx.IsPlaying)
            {
                MajInstances.AudioManager.PlaySFX("bgm_select.mp3", true);
                MajInstances.AudioManager.PlaySFX("SelectSong.wav");
            }
        }
        async UniTaskVoid InitializeCoverListAsync()
        {
            try
            {
                await UniTask.Yield();
                await _coverListDisplayer.SwitchToDirListAsync();
                await _coverListDisplayer.SwitchToSongListAsync();
                await UniTask.Yield();
            }
            finally
            {
                MajInstances.SceneSwitcher.FadeOut();
                _coverListDisplayer.SlideToDifficulty((int)MajInstances.GameManager.SelectedDiff);
                _isInited = true;
                LedRing.SetButtonLight(Color.green, 3);
                LedRing.SetButtonLight(Color.red, 4);
                LedRing.SetButtonLight(Color.blue, 2);
                LedRing.SetButtonLight(Color.blue, 5);
                LedRing.SetButtonLight(Color.yellow, 6);
                LedRing.SetButtonLight(Color.yellow, 1);
            }
        }
        void OnDestroy()
        {
            _isExited = true;
            Majdata<ListManager>.Free();
            MajEnv.SharedHttpClient.CancelPendingRequests();
            _cts.Cancel();
        }
        void Update()
        {
            SensorCheck();
            ButtonDownCheck();
            ButtonUpCheck();
        }
        void SensorCheck()
        {
            if (_isExited || !_isInited)
            {
                return;
            }

            if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.A7, SensorArea.E8))
            {
                _coverListDisplayer.SlideList(1);
            }
            else if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.B8, SensorArea.A8))
            {
                _coverListDisplayer.SlideList(2);
            }
            else if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.E1, SensorArea.D1, SensorArea.A1))
            {
                _coverListDisplayer.SlideList(3);
            }
            else if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.A6, SensorArea.E6))
            {
                _coverListDisplayer.SlideList(-1);
            }
            else if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.A5, SensorArea.B5))
            {
                _coverListDisplayer.SlideList(-2);
            }
            else if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.E5, SensorArea.D5, SensorArea.A4))
            {
                _coverListDisplayer.SlideList(-3);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.C))
            {
                _coverListDisplayer.RandomSelect();
            }

            if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.B7, SensorArea.B6, SensorArea.E7))
            {
                var list = _xxlbVoices.Span;
                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                XxlbAnimation.instance.PlayTouchAnimation();
            }
            if (InputManager.IsSensorClickedInThisFrame(SensorArea.B2))
            {
                _coverListDisplayer.FavoriteAdder.FavoratePressed();
            }
        }
        void ButtonDownCheck()
        {
            if (_isExited || !_isInited)
            {
                return;
            }

            if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A8))
            {
                _coverListDisplayer.SlideDifficulty(-1);
            }
            else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A1))
            {
                _coverListDisplayer.SlideDifficulty(1);
            }

            if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.A3, SwitchStatus.On))
            {
                if (!_isPressed)
                {
                    _coverListDisplayer.SlideList(1);
                    _isPressed = true;
                    _delta = 1;
                    AutoSlide().Forget();
                }
                return;
            }
            else if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.A6, SwitchStatus.On))
            {
                if (!_isPressed)
                {
                    _coverListDisplayer.SlideList(-1);
                    _isPressed = true;
                    _delta = -1;
                    AutoSlide().Forget();
                }
                return;
            }


            if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.A4, SwitchStatus.On))
            {
                if (!_coverListDisplayer.IsDirList)
                {
                    if(!_isPressed)
                    {
                        _isPressed = true;
                        PracticeTimer().Forget();
                    }
                    if (_pressTime > 1.6f)
                    {
                        var levels = new ChartLevel[]
                        {
                            MajInstances.GameManager.SelectedDiff
                        };
                        var charts = new ISongDetail[]
                        {
                            _coverListDisplayer.SelectedSong
                        };
                        var oldinfo = Majdata<GameInfo>.Instance;
                        var info = new GameInfo(GameMode.Practice, charts, levels, 114514);
                        if (oldinfo is not null && oldinfo.TimeRange is not null)
                        {
                            info.TimeRange = oldinfo.TimeRange;
                        }

                        Majdata<GameInfo>.Instance = info;
                        _pressTime = 0;
                        _isPressed = false;
                        _isExited = true;
                        MajInstances.SceneSwitcher.SwitchScene("Practice", false);
                    }
                    return;
                }
            }
            else if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.A5, SwitchStatus.On))
            {
                if (_coverListDisplayer.IsChartList)
                {
                    _coverListDisplayer.SwitchToDirListAsync()
                                       .AsTask()
                                       .Wait();
                    LedRing.SetButtonLight(Color.white, 4);
                    SongStorage.WorkingCollection.Index = 0;
                }
            }
            if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.P1, SwitchStatus.On))
            {
                MajInstances.SceneSwitcher.SwitchScene("SortFind");
                _isExited = true;
                return;
            }
            else if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.A2, SwitchStatus.On))
            {
                MajInstances.GameManager.LastSettingPage = 4;
                MajInstances.SceneSwitcher.SwitchScene("Setting");
                _isExited = true;
                return;
            }
            else if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.A7, SwitchStatus.On))
            {
                MajInstances.GameManager.LastSettingPage = 0;
                MajInstances.SceneSwitcher.SwitchScene("Setting");
                _isExited = true;
                return;
            }
        }
        void ButtonUpCheck()
        {
            if (_isExited || !_isInited)
            {
                return;
            }
            var isA3Up = InputManager.CheckButtonStatusInPreviousFrame(ButtonZone.A3, SwitchStatus.On) &&
                         InputManager.CheckButtonStatusInThisFrame(ButtonZone.A3, SwitchStatus.Off);
            var isA4Up = InputManager.CheckButtonStatusInPreviousFrame(ButtonZone.A4, SwitchStatus.On) &&
                         InputManager.CheckButtonStatusInThisFrame(ButtonZone.A4, SwitchStatus.Off);
            var isA6Up = InputManager.CheckButtonStatusInPreviousFrame(ButtonZone.A6, SwitchStatus.On) && 
                         InputManager.CheckButtonStatusInThisFrame(ButtonZone.A6, SwitchStatus.Off);

            if ((isA3Up && _delta == 1) || (isA6Up && _delta == -1))
            {
                _isPressed = false;
                _delta = 0;
                _pressTime = 0;
            }
            else if(isA4Up)
            {
                if (_coverListDisplayer.IsDirList)
                {
                    if (_coverListDisplayer.SelectedCollection.Type == ChartStorageType.Dan)
                    {
                        var danInfo = _coverListDisplayer.SelectedCollection.DanInfo;
                        var collection = _coverListDisplayer.SelectedCollection;
                        if (danInfo is null)
                        {
                            return;
                        }
                        else if (danInfo.SongLevels.Length != collection.Count)
                        {
                            return;
                        }
                        MajInstances.AudioManager.StopSFX("bgm_select.mp3");
                        var list = new string[] { "track_start.wav", "track_start_2.wav" };
                        MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                        var levels = new ChartLevel[danInfo.SongLevels.Length];
                        for (int i = 0; i < levels.Length; i++)
                        {
                            levels[i] = (ChartLevel)danInfo.SongLevels[i];
                        }
                        var info = new GameInfo(GameMode.Dan, collection.ToArray(), levels)
                        {
                            MaxHP = _coverListDisplayer.SelectedCollection.DanInfo.StartHP,
                            CurrentHP = _coverListDisplayer.SelectedCollection.DanInfo.StartHP,
                            HPRecover = _coverListDisplayer.SelectedCollection.DanInfo.RestoreHP,
                            DanInfo = danInfo
                        };
                        Majdata<GameInfo>.Instance = info;
                        _coverListDisplayer.SelectedCollection.Index = 0;
                        _isExited = true;
                        MajInstances.SceneSwitcher.SwitchScene("Game", false);
                    }
                    else
                    {
                        _coverListDisplayer.SwitchToSongListAsync()
                                           .AsTask()
                                           .Wait();
                        LedRing.SetButtonLight(Color.red, 4);
                    }
                }
                else
                {
                    MajInstances.AudioManager.StopSFX("bgm_select.mp3");
                    var list = new string[] { "track_start.wav", "track_start_2.wav" };
                    MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                    var levels = new ChartLevel[]
                    {
                                    MajInstances.GameManager.SelectedDiff
                    };
                    var charts = new ISongDetail[]
                    {
                                    _coverListDisplayer.SelectedSong
                    };
                    var info = new GameInfo(GameMode.Normal, charts, levels);
                    Majdata<GameInfo>.Instance = info;
                    _pressTime = 0;
                    _isPressed = false;
                    _isExited = true;
                    MajInstances.SceneSwitcher.SwitchScene("Game", false);
                }
            }
        }
        async UniTaskVoid AutoSlide()
        {
            while (_isPressed)
            {
                if (_pressTime < 0.4f)
                {
                    _pressTime += Time.deltaTime;
                    await UniTask.Yield();
                    continue;
                }
                _coverListDisplayer.DisableAnimation = true;
                _coverListDisplayer.SlideList(_delta);
                await UniTask.Delay(150);
            }
            _coverListDisplayer.DisableAnimation = false;
        }
        async UniTaskVoid PracticeTimer()
        {
            var _ = false;
            while (_isPressed)
            {
                _pressTime += Time.deltaTime;
                await UniTask.Yield();
                if(_pressTime > 1f && !_)
                {
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    _ = !_;
                }
                if(_isExited)
                {
                    break;
                }
            }
        }
        public static async UniTask WaitForBackgroundTasksSuspendAsync()
        {
            if (AllBackgroundTasks.Count == 0)
            {
                return;
            }
            await UniTask.WhenAll(AllBackgroundTasks);
            AllBackgroundTasks.Clear();
        }
    }
}