using Cysharp.Threading.Tasks;
using MajdataPlay.Scenes.Game;
using MajdataPlay.IO;
using MajdataPlay.Recording;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using MajdataPlay.Scenes.Setting;
using System.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Settings.Runtime;

namespace MajdataPlay.Scenes.List
{
    public class ListManager : MonoBehaviour
    {
        public CancellationToken CancellationToken => _cts.Token;
        public static List<Task> AllBackgroundTasks { get; } = new(8192);

        int _delta = 0;
        float _pressTime = 0;
        bool _isPressed = false;

        // Update control
        bool _isInited = false;
        bool _isExited = false;

        bool _isPlayedExplosion = false;

        float _autoSlideTimer = 0f;
        float _enterPracticeTimer = 0f;

        CoverListDisplayer _coverListDisplayer;

        const float AUTO_SLIDE_INTERVAL_SEC = 0.15f;
        const float AUTO_SLIDE_TRIGGER_TIME_SEC = 0.4f;

        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();
        readonly SwitchStatistic[] _buttonPressTimes = new SwitchStatistic[12];
        readonly CancellationTokenSource _cts = new();


        void Awake()
        {
            Majdata<ListManager>.Instance = this;

            if(AllBackgroundTasks.Count > 4096)
            {
                var indexs = Pool<int>.RentArray(AllBackgroundTasks.Count);
                try
                {
                    var i2 = -1;
                    var count = 0;
                    Parallel.For(0, AllBackgroundTasks.Count, i =>
                    {
                        var task = AllBackgroundTasks[i];
                        if (task is null || task.IsCompleted)
                        {
                            indexs[Interlocked.Increment(ref i2)] = i;
                            Interlocked.Increment(ref count);
                        }
                    });
                    for (var i = 0; i < count; i++)
                    {
                        AllBackgroundTasks.RemoveAt(indexs[i]);
                    }
                }
                finally
                {
                    Pool<int>.ReturnArray(indexs);
                }
            }
            else
            {
                for (var i = 0; i < AllBackgroundTasks.Count; i++)
                {
                    var task = AllBackgroundTasks[i];
                    if (task is null || task.IsCompleted)
                    {
                        AllBackgroundTasks.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        void Start()
        {
            _coverListDisplayer = Majdata<CoverListDisplayer>.Instance!;
            InitializeCoverListAsync().Forget();
            var selectsfx = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            if (!selectsfx.IsPlaying)
            {
                MajInstances.AudioManager.PlaySFX("bgm_select.mp3", true);
                var list = new string[] { "select_song.wav", "select_song_2.wav", "select_song_3.wav", "select_song_4.wav" };
                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
            }
        }
        async UniTaskVoid InitializeCoverListAsync()
        {
            try
            {
                await UniTask.Yield();
                _coverListDisplayer.SwitchToDirList();
                _coverListDisplayer.SwitchToSongList();
                await UniTask.Yield();
            }
            finally
            {
                MajInstances.SceneSwitcher.FadeOut();
                _coverListDisplayer.SlideToDifficulty((int)_listConfig.SelectedDiff);
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
            ButtonStatisticsUpdate();
            SensorCheck();
            ButtonCheck();
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
                var list = new string[]
                            {
                                "no_touch.wav",
                                "no_touch_2.wav",
                                "no_touch_3.wav",
                                "no_touch_4.wav",
                                "no_touch_5.wav",
                                "no_touch_6.wav",
                                "no_touch_7.wav"
                            };
                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                XxlbAnimation.instance.PlayTouchAnimation();
            }
            if (InputManager.IsSensorClickedInThisFrame(SensorArea.B2))
            {
                _coverListDisplayer.FavoriteAdder.FavoratePressed();
            }
        }
        void ButtonStatisticsUpdate()
        {
            if (_isExited || !_isInited)
            {
                return;
            }
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
                ref var btnStatistic = ref _buttonPressTimes[i];
                var isPressed = InputManager.CheckButtonStatusInThisFrame(zone, SwitchStatus.On);

                btnStatistic.IsPressed = isPressed;
                btnStatistic.IsReleased = InputManager.CheckButtonStatusInPreviousFrame(zone, SwitchStatus.On) &&
                                          InputManager.CheckButtonStatusInThisFrame(zone, SwitchStatus.Off);
                btnStatistic.IsClicked = InputManager.IsButtonClickedInThisFrame(zone);
                if(btnStatistic.IsClicked)
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
        void ButtonCheck()
        {
            if (_isExited || !_isInited)
            {
                return;
            }

            ref var a1Statistic = ref _buttonPressTimes[(int)ButtonZone.A1];
            ref var a2Statistic = ref _buttonPressTimes[(int)ButtonZone.A2];
            ref var a3Statistic = ref _buttonPressTimes[(int)ButtonZone.A3];
            ref var a4Statistic = ref _buttonPressTimes[(int)ButtonZone.A4];
            ref var a5Statistic = ref _buttonPressTimes[(int)ButtonZone.A5];
            ref var a6Statistic = ref _buttonPressTimes[(int)ButtonZone.A6];
            ref var a7Statistic = ref _buttonPressTimes[(int)ButtonZone.A7];
            ref var a8Statistic = ref _buttonPressTimes[(int)ButtonZone.A8];
            ref var p1Statistic = ref _buttonPressTimes[(int)ButtonZone.P1];

            if (a8Statistic.IsClicked)
            {
                _coverListDisplayer.SlideDifficulty(-1);
                var list = new string[] { "easy.wav", "basic.wav", "advanced.wav", "expert.wav", "master.wav", "remaster.wav", "original.wav" };
                MajInstances.AudioManager.PlaySFX(list[(int)_listConfig.SelectedDiff]);
            }
            else if (a1Statistic.IsClicked)
            {
                _coverListDisplayer.SlideDifficulty(1);
                var list = new string[] { "easy.wav", "basic.wav", "advanced.wav", "expert.wav", "master.wav", "remaster.wav", "original.wav" };
                MajInstances.AudioManager.PlaySFX(list[(int)_listConfig.SelectedDiff]);
            }
            

            if (a3Statistic.IsPressed)
            {
                _delta = 1;
                if (a3Statistic.IsClicked)
                {
                    _coverListDisplayer.SlideList(1);
                }
                else
                {
                    if(a3Statistic.PressTime > AUTO_SLIDE_TRIGGER_TIME_SEC)
                    {
                        if(_autoSlideTimer > AUTO_SLIDE_INTERVAL_SEC)
                        {
                            _coverListDisplayer.DisableAnimation = true;
                            _coverListDisplayer.SlideList(_delta);
                            _autoSlideTimer = 0;
                        }
                        else
                        {
                            _autoSlideTimer += MajTimeline.DeltaTime;
                        }
                    }
                }
                return;
            }
            else if (a6Statistic.IsPressed)
            {
                _delta = -1;
                if (a6Statistic.IsClicked)
                {
                    _coverListDisplayer.SlideList(-1);
                }
                else
                {
                    if (a6Statistic.PressTime > AUTO_SLIDE_TRIGGER_TIME_SEC)
                    {
                        if (_autoSlideTimer > AUTO_SLIDE_INTERVAL_SEC)
                        {
                            _coverListDisplayer.DisableAnimation = true;
                            _coverListDisplayer.SlideList(_delta);
                            _autoSlideTimer = 0;
                        }
                        else
                        {
                            _autoSlideTimer += MajTimeline.DeltaTime;
                        }
                    }
                }
                return;
            }
            else
            {
                _autoSlideTimer = 0;
                _coverListDisplayer.DisableAnimation = false;
            }

            if (a4Statistic.IsClicked)
            {
                if(_coverListDisplayer.IsDirList)
                {
                    if (_coverListDisplayer.SelectedCollection.Type == ChartStorageType.Dan)
                    {
                        EnterDan();
                    }
                    else
                    {
                        LedRing.SetButtonLight(Color.red, 4);
                        _coverListDisplayer.SwitchToSongList();
                        _coverListDisplayer.SlideListToTop();
                        if (SongStorage.WorkingCollection.IsOnline)
                        {
                            MajInstances.AudioManager.PlaySFX("online_page.wav");
                        }
                    }
                    a4Statistic.IsClickEventUsed = true;
                }
            }
            else if (a4Statistic.IsPressed)
            {
                if (_coverListDisplayer.IsChartList)
                {
                    if(!_isPlayedExplosion)
                    {
                        if(_enterPracticeTimer > 1f)
                        {
                            MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                            _isPlayedExplosion = true;
                        }
                    }
                    if (_enterPracticeTimer > 1.6f)
                    {
                        EnterPractice();
                    }
                    _enterPracticeTimer += MajTimeline.DeltaTime;
                    return;
                }
            }
            else if (a4Statistic.IsReleased)
            {
                if (_coverListDisplayer.IsChartList && !a4Statistic.IsClickEventUsed)
                {
                    if (_enterPracticeTimer > 1f)
                    {
                        EnterPractice();
                    }
                    else
                    {
                        EnterGame();
                    }
                }
                return;
            }
            else if (a5Statistic.IsClicked)
            {
                if (_coverListDisplayer.IsChartList)
                {
                    _coverListDisplayer.SwitchToDirList();
                    LedRing.SetButtonLight(Color.white, 4);
                    SongStorage.WorkingCollection.Index = 0;
                    return;
                }
            }
            else
            {
                _enterPracticeTimer = 0;
            }

            if (p1Statistic.IsClicked || p1Statistic.IsPressed || p1Statistic.IsReleased)
            {
                if(p1Statistic.PressTime >= 3f)
                {
                    RefreshList();
                }
                else if(p1Statistic.IsReleased)
                {
                    EnterSortAndFind();
                }
                return;
            }
            else if (a2Statistic.IsClicked)
            {
                //const int MOD_PAGE_INDEX = 5;
                //MajInstances.GameManager.LastSettingPage = MOD_PAGE_INDEX;
                SettingManager.JmpToModPage();
                if (_coverListDisplayer.Mode == CoverListMode.Directory)
                {
                    SettingManager.IgnoreChartSettingPage();
                }
                MajInstances.SceneSwitcher.SwitchScene("Setting");
                _isExited = true;
                return;
            }
            else if (a7Statistic.IsClicked)
            {
                //MajInstances.GameManager.LastSettingPage = 0;
                SettingManager.JmpToDefaultPage();
                if(_coverListDisplayer.Mode == CoverListMode.Directory)
                {
                    SettingManager.IgnoreChartSettingPage();
                }
                MajInstances.SceneSwitcher.SwitchScene("Setting");
                _isExited = true;
                return;
            }
        }
        void EnterGame()
        {
            _cts.Cancel();
            MajInstances.AudioManager.StopSFX("bgm_select.mp3");
            var list = new string[] { "track_start.wav", "track_start_2.wav", "track_start_3.wav" };
            MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
            var levels = new ChartLevel[]
            {
                _listConfig.SelectedDiff
            };
            var charts = new ISongDetail[]
            {
                _coverListDisplayer.SelectedSong
            };
            var info = new GameInfo(GameMode.Normal, charts, levels);
            Majdata<GameInfo>.Instance = info;
            _pressTime = 0;
            _isExited = true;
            MajInstances.SceneSwitcher.SwitchScene("Game", false);
        }
        void EnterPractice()
        {
            _cts.Cancel();
            var levels = new ChartLevel[]
            {
                _listConfig.SelectedDiff
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
            _isExited = true;
            MajInstances.SceneSwitcher.SwitchScene("Practice", false);
        }
        void EnterSortAndFind()
        {
            _cts.Cancel();
            MajInstances.SceneSwitcher.SwitchScene("SortFind");
            _isExited = true;
        }
        void RefreshList()
        {
            _cts.Cancel();
            MajInstances.AudioManager.StopSFX("bgm_select.mp3");
            _pressTime = 0;
            _isExited = true;
            RefreshListBackgroundAsync();
        }
        static async void RefreshListBackgroundAsync()
        {
            var sceneSwitcher = MajInstances.SceneSwitcher;
            await sceneSwitcher.FadeInAsync();
            sceneSwitcher.SwitchScene("Empty", false);
            await UniTask.Delay(400);
            sceneSwitcher.SetLoadingText("Waiting for all background tasks to suspend".i18n());
            await UniTask.Delay(100);
            var bTasks = WaitForBackgroundTaskSuspendAsync();
            while(!bTasks.IsCompleted)
            {
                await UniTask.Yield();
            }
            var progress = new Progress<string>();
            progress.ProgressChanged += (o, e) =>
            {
                MajInstances.SceneSwitcher.SetLoadingText(e);
            };
            var task = SongStorage.RefreshAsync(progress);
            while(!task.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (!task.IsCompletedSuccessfully)
            {
                sceneSwitcher.SetLoadingText("MAJTEXT_SCAN_CHARTS_FAILED".i18n(), Color.red);
            }
            else
            {
                sceneSwitcher.SetLoadingText(string.Empty);
            }
            await UniTask.Delay(3000);
            sceneSwitcher.SwitchScene("List");
        }
        void EnterDan()
        {
            _cts.Cancel();
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
            MajInstances.AudioManager.PlaySFX("challenge_mode.wav");
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
        public static Task WaitForBackgroundTaskSuspendAsync()
        {
            if (AllBackgroundTasks.Count == 0)
            {
                return Task.CompletedTask;
            }
            return Task.WhenAll(AllBackgroundTasks);
        }
    }
}
