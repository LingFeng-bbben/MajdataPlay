using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.IO;
using MajdataPlay.Recording;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Setting;
using MajdataPlay.Settings;
using MajdataPlay.Settings.Runtime;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Net;
using UnityEngine;
using UnityEngine.Serialization;
using LitMotion;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class ListManager : MonoBehaviour, ISceneManager
    {
        const int MAX_ALLOWED_INACTIVE_TIME_MIN = 5;
        public CancellationToken CancellationToken
        {
            get
            {
                return _cts.Token;
            }
        }
        public static List<Task> AllBackgroundTasks { get; } = new(8192);

        int _delta = 0;
        float _pressTime = 0;
        bool _isPressed = false;

        // Update control
        bool _isInited = false;
        bool _isExited = false;

        bool _isOnlineEnabled = false;

        bool _isPlayedExplosion = false;

        float _autoSlideTimer = 0f;
        float _enterPracticeTimer = 0f;
        float _inactiveTimeSec = 0f;

        [SerializeField]
        [FormerlySerializedAs("coverListManager")]
        CoverListManager _coverListManager;

        [SerializeField]
        [FormerlySerializedAs("collectionListManager")]
        CollectionListManager _collectionListManager;

        [SerializeField]
        [FormerlySerializedAs("userProfileDisplayer")]
        UserInfoDisplayer _userProfileDisplayer;

        [SerializeField]
        [FormerlySerializedAs("favoriteAdder")]
        FavoriteAdder _favoriteAdder;

        const float AUTO_SLIDE_INTERVAL_SEC = 0.15f;
        const float AUTO_SLIDE_TRIGGER_TIME_SEC = 0.4f;
        const int QUICK_SLIDE_POSITION_INCREASE = 9;
        const float QUICK_SLIDE_DURATION_SEC = 1f;

        int _quickSlideRemaining = 0;
        int _quickSlideDirection = 0;
        MotionHandle _quickSlideAnim;

        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();
        readonly SwitchStatistic[] _buttonPressTimes = new SwitchStatistic[12];
        readonly CancellationTokenSource _cts = new();
        readonly QuickSlideJudge _quickSlideJudge = new();


        void Awake()
        {
            InputManager.Override_TouchSimulationRadius = 0.1f;
            InputManager.Override_TouchAAreaExtraRadius = 0f;
            InputManager.Override_TouchBAreaExtraRadius = 0f;
            InputManager.Override_TouchCAreaExtraRadius = 0f;
            InputManager.Override_TouchDAreaExtraRadius = 0f;
            InputManager.Override_TouchEAreaExtraRadius = 0f;

            Majdata<ListManager>.Instance = this;
            InputManager.TouchButtonRingEdge = 4.8f;
            if (AllBackgroundTasks.Count > 4096)
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
            _isOnlineEnabled = MajEnv.Settings.Online.Enable;


            _favoriteAdder.PressToAddTime = 0.5f;
            _favoriteAdder.PressToRemoveTime = 0.5f;

            InputManager.BindAnyArea(OnAnyInput);
        }
        void Start()
        {
            _coverListManager = Majdata<CoverListManager>.Instance!;
            _collectionListManager = Majdata<CollectionListManager>.Instance!;
            InitializeCoverListAsync().Forget();
            var selectsfx = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            if (!selectsfx.IsPlaying)
            {
                MajInstances.AudioManager.PlaySFX("bgm_select.mp3", true);
                var list = new string[] { "select_song.wav", "select_song_2.wav", "select_song_3.wav", "select_song_4.wav" };
                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
            }
            DisplayUserInfo();
        }

        void DisplayUserInfo()
        {
            //TODO: display multiple endpoints
            var apiendpoint = MajEnv.Settings.Online.ApiEndpoints.FirstOrDefault();
            if (apiendpoint is not null)
            {
                _userProfileDisplayer.DisplayUserInfo(apiendpoint);
            }
            else
            {
                _userProfileDisplayer.gameObject.SetActive(false);
            }
        }

        async UniTaskVoid InitializeCoverListAsync()
        {
            try
            {
                await UniTask.Yield();
                _collectionListManager.Init();
                //_coverListDisplayer.SwitchToDirList();
                //_coverListDisplayer.SwitchToSongList();
                await UniTask.Yield();
            }
            finally
            {
                MajInstances.SceneSwitcher.FadeOut();
                _isInited = true;
                CabinetLed.SetButtonLight(Color.green, 3);
                CabinetLed.SetButtonLight(Color.red, 4);
                CabinetLed.SetButtonLight(Color.blue, 2);
                CabinetLed.SetButtonLight(Color.blue, 5);
                CabinetLed.SetButtonLight(Color.yellow, 6);
                CabinetLed.SetButtonLight(Color.yellow, 1);
            }
        }
        void OnDestroy()
        {
            _isExited = true;
            _cts.Cancel();

            InputManager.Override_TouchSimulationRadius = default;
            InputManager.Override_TouchAAreaExtraRadius = default;
            InputManager.Override_TouchBAreaExtraRadius = default;
            InputManager.Override_TouchCAreaExtraRadius = default;
            InputManager.Override_TouchDAreaExtraRadius = default;
            InputManager.Override_TouchEAreaExtraRadius = default;

            InputManager.TouchButtonRingEdge = 5.4f;
            InputManager.UnbindAnyArea(OnAnyInput);
            Majdata<ListManager>.Free();
            MajEnv.SharedHttpClient.CancelPendingRequests();
        }
        void Update()
        {
            if (_isExited || !_isInited)
            {
                return;
            }
            ButtonStatisticsUpdate();
            _quickSlideJudge.OnUpdate();
            var isMathed = false;
            if(_quickSlideJudge.IsLeftMatch)
            {
                isMathed = true;
                if (_quickSlideDirection == -1)
                {
                    _quickSlideRemaining -= QUICK_SLIDE_POSITION_INCREASE;
                    if(_quickSlideRemaining < 0)
                    {
                        _quickSlideDirection = 1;
                        _quickSlideRemaining *= -1;
                    }
                }
                else
                {
                    _quickSlideDirection = 1;
                    _quickSlideRemaining += QUICK_SLIDE_POSITION_INCREASE;
                }
                MajDebug.LogDebug($"[List] Quick slide remaining: {_quickSlideRemaining}");
            }
            else if(_quickSlideJudge.IsRightMatch)
            {
                isMathed = true;
                if (_quickSlideDirection == 1)
                {
                    _quickSlideRemaining -= QUICK_SLIDE_POSITION_INCREASE;
                    if (_quickSlideRemaining < 0)
                    {
                        _quickSlideDirection = -1;
                        _quickSlideRemaining *= -1;
                    }
                }
                else
                {
                    _quickSlideDirection = -1;
                    _quickSlideRemaining += QUICK_SLIDE_POSITION_INCREASE;
                }
                MajDebug.LogDebug($"[List] Quick slide remaining: {_quickSlideRemaining}");
            }
            if(isMathed)
            {
                _quickSlideAnim.TryCancel();
                var lastIndex = 0;
                _quickSlideAnim = LMotion.Create(0, _quickSlideRemaining * _quickSlideDirection, QUICK_SLIDE_DURATION_SEC)
                                         .WithEase(Ease.Linear)
                                         .WithOnComplete(() =>
                                         {
                                             _quickSlideRemaining = 0;
                                             _quickSlideDirection = 0;
                                         })
                                         .Bind(x =>
                                         {
                                             if(x == lastIndex)
                                             {
                                                 return;
                                             }
                                             lastIndex = x;
                                             _quickSlideRemaining -= 1;
                                             _coverListManager.SlideList(1 * _quickSlideDirection, loadDelayMS: 1000);
                                         });
            }
            if (_quickSlideRemaining == 0)
            {
                var areaIgnoreList = (stackalloc bool[33]);
                _quickSlideJudge.GenerateIgnoreAreaList(areaIgnoreList);
                SensorCheck(areaIgnoreList);
                ButtonCheck();
                _inactiveTimeSec += MajTimeline.UnscaledDeltaTime;
                if (_isOnlineEnabled && TimeSpan.FromSeconds(_inactiveTimeSec) > TimeSpan.FromMinutes(MAX_ALLOWED_INACTIVE_TIME_MIN))
                {
                    EnterLogin();
                    return;
                }
            }            
        }
        void OnAnyInput(object? sender, InputEventArgs args)
        {
            _inactiveTimeSec = 0;
        }
        void SensorCheck(ReadOnlySpan<bool> areaIgnoreList)
        {
            if (_isExited || !_isInited)
            {
                return;
            }

            if (InputManager.IsSensorClickedInThisFrame(SensorArea.B8))
            {
                _collectionListManager.PreviousCollection();
                return;
            }
            else if(InputManager.IsSensorClickedInThisFrame(SensorArea.B1))
            {
                _collectionListManager.NextCollection();
                return;
            }

            if (InputManager.IsSensorClickedInThisFrame_OR(SensorArea.D5, SensorArea.E5))
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
            if (areaIgnoreList[(int)SensorArea.B2])
            {
                _favoriteAdder.SetState(false);
            }
            else if (!_favoriteAdder.State && InputManager.IsSensorClickedInThisFrame(SensorArea.B2))
            {
                _favoriteAdder.SetState(true);
            }
            else if (_favoriteAdder.State && InputManager.CheckSensorStatus(SensorArea.B2, SwitchStatus.Off))
            {
                _favoriteAdder.SetState(false);
            }
            if (InputManager.IsSensorClickedInThisFrame(SensorArea.C) && !areaIgnoreList[(int)SensorArea.C])
            {
                //TODO: _coverListDisplayer.RandomSelect();
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

            if (a3Statistic.IsPressed)
            {
                _delta = 1;
                if (a3Statistic.IsClicked)
                {
                    _coverListManager.SlideList(1);
                }
                else
                {
                    if(a3Statistic.PressTime > AUTO_SLIDE_TRIGGER_TIME_SEC)
                    {
                        if(_autoSlideTimer > AUTO_SLIDE_INTERVAL_SEC)
                        {
                            //_coverListDisplayer.DisableAnimation = true;
                            _coverListManager.SlideList(_delta);
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
                    _coverListManager.SlideList(-1);
                }
                else
                {
                    if (a6Statistic.PressTime > AUTO_SLIDE_TRIGGER_TIME_SEC)
                    {
                        if (_autoSlideTimer > AUTO_SLIDE_INTERVAL_SEC)
                        {
                            //_coverListDisplayer.DisableAnimation = true;
                            _coverListManager.SlideList(_delta);
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
                //_coverListDisplayer.DisableAnimation = false;
            }

            if (a4Statistic.IsPressed && _coverListManager.SelectedSong is not null)
            {
                if (!_isPlayedExplosion)
                {
                    if (_enterPracticeTimer > 1f)
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
            else if (a4Statistic.IsReleased)
            {
                if (!a4Statistic.IsClickEventUsed)
                {
                    if (_enterPracticeTimer > 1f)
                    {
                        EnterPractice();
                    }
                    else if (_collectionListManager.SelectedCollection.Type == ChartStorageType.Dan)
                    {
                        EnterDan();
                    }
                    else if(_coverListManager.SelectedSong is not null)
                    {
                        EnterGame();
                    }
                }
                return;
            }
            else if (a5Statistic.IsClicked)
            {
                if (_isOnlineEnabled)
                {
                    EnterLogin();
                }
                return;
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
                MajEnv.RuntimeConfig.Setting.SelectedMenu = nameof(GameSetting.Mod);
                MajEnv.RuntimeConfig.Setting.SelectedOption = string.Empty;
                //if (_coverListDisplayer.Mode == CoverListMode.Directory)
                //{
                //    MajEnv.RuntimeConfig.Setting.IgnoreChartSettingPage = true;
                //}
                //else
                //{
                //    MajEnv.RuntimeConfig.Setting.IgnoreChartSettingPage = false;
                //}
                MajInstances.SceneSwitcher.SwitchScene("Setting", false);
                _isExited = true;
                return;
            }
            else if (a7Statistic.IsClicked)
            {
                //MajInstances.GameManager.LastSettingPage = 0;
                //if (_coverListDisplayer.Mode == CoverListMode.Directory)
                //{
                //    MajEnv.RuntimeConfig.Setting.IgnoreChartSettingPage = true;
                //}
                //else
                //{
                //    MajEnv.RuntimeConfig.Setting.IgnoreChartSettingPage = false;
                //}
                MajInstances.SceneSwitcher.SwitchScene("Setting", false);
                _isExited = true;
                return;
            }

            if (a8Statistic.IsClicked)
            {
                _collectionListManager.SlideDifficulty(-1);
                var list = new string[] { "easy.wav", "basic.wav", "advanced.wav", "expert.wav", "master.wav", "remaster.wav", "original.wav" };
                MajInstances.AudioManager.PlaySFX(list[(int)_listConfig.SelectedDiff]);
            }
            else if (a1Statistic.IsClicked)
            {
                _collectionListManager.SlideDifficulty(1);
                var list = new string[] { "easy.wav", "basic.wav", "advanced.wav", "expert.wav", "master.wav", "remaster.wav", "original.wav" };
                MajInstances.AudioManager.PlaySFX(list[(int)_listConfig.SelectedDiff]);
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
                _coverListManager.SelectedSong
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
                _coverListManager.SelectedSong
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
            sceneSwitcher.SetLoadingText("MAJTEXT_WAITING_FOR_BACKGROUND_TASKS_SUSPEND".i18n());
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
                sceneSwitcher.SetLoadingText("MAJTEXT_ERR_SCAN_CHARTS_FAILED".i18n(), Color.red);
            }
            else
            {
                sceneSwitcher.SetLoadingText(string.Empty);
            }
            await UniTask.Delay(3000);
            sceneSwitcher.SwitchScene("List", false);
        }
        void EnterDan()
        {
            _cts.Cancel();
            var danInfo = _collectionListManager.SelectedCollection.DanInfo;
            var collection = _collectionListManager.SelectedCollection;
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
                MaxHP = danInfo.StartHP,
                CurrentHP = danInfo.StartHP,
                HPRecover = danInfo.RestoreHP,
                DanInfo = danInfo
            };
            Majdata<GameInfo>.Instance = info;
            _collectionListManager.SelectedCollection.Index = 0;
            _isExited = true;
            MajInstances.SceneSwitcher.SwitchScene("Game", false);
        }
        void EnterLogin()
        {
            _cts.Cancel();
            _pressTime = 0;
            _isExited = true;
            MajInstances.AudioManager.StopSFX("bgm_select.mp3");
            ScoreManager.UnloadOnlineScores();
            EnterLoginBackgroundAsync();
        }
        async void EnterLoginBackgroundAsync()
        {
            var sceneSwitcher = MajInstances.SceneSwitcher;
            await sceneSwitcher.FadeInAsync();
            sceneSwitcher.SwitchScene("Empty", false);
            await UniTask.Delay(400);
            sceneSwitcher.SetLoadingText("MAJTEXT_WAITING_FOR_BACKGROUND_TASKS_SUSPEND".i18n());
            await UniTask.Delay(100);
            var bTasks = WaitForBackgroundTaskSuspendAsync();
            while (!bTasks.IsCompleted)
            {
                await UniTask.Yield();
            }
            await UniTask.Delay(100);
            sceneSwitcher.SetLoadingText("MAJTEXT_LOGGING_OUT".i18n() + "...");
            var task = Online.LogoutAllAsync();
            while (!task.IsCompleted)
            {
                await UniTask.Yield();
            }
            sceneSwitcher.SetLoadingText(string.Empty);
            await UniTask.Delay(1000);
            sceneSwitcher.SwitchScene("Login");
        }
        public static Task WaitForBackgroundTaskSuspendAsync()
        {
            if (AllBackgroundTasks.Count == 0)
            {
                return Task.CompletedTask;
            }
            var isAnyRunning = false;
            using var tasks = new RentedList<Task>();
            foreach(var task in AllBackgroundTasks)
            {
                if (!task.IsCompleted)
                {
                    isAnyRunning |= true;
                    tasks.Add(task);
                }
            }
            if (!isAnyRunning)
            {
                return Task.CompletedTask;
            }
            return Task.WhenAll(tasks);
        }

        class QuickSlideJudge
        {
            public bool IsLeftMatch { get; private set; }
            public bool IsRightMatch { get; private set; }

            readonly Path[] _leftPathList = new Path[]
            {
                // Left
                new Path(SensorArea.C, SensorArea.B7, new SensorArea[] { SensorArea.E7 }),
                new Path(SensorArea.C, SensorArea.B6, new SensorArea[] { SensorArea.E7 }),

                new Path(SensorArea.B2, SensorArea.C, new SensorArea[] { SensorArea.B7 }),
                new Path(SensorArea.B2, SensorArea.C, new SensorArea[] { SensorArea.B6 }),
                
                new Path(SensorArea.B3, SensorArea.C, new SensorArea[] { SensorArea.B7 }),
                new Path(SensorArea.B3, SensorArea.C, new SensorArea[] { SensorArea.B6 }),

                new Path(SensorArea.B2, SensorArea.C, new SensorArea[] { SensorArea.B7 }),
                new Path(SensorArea.B3, SensorArea.C, new SensorArea[] { SensorArea.B6 }),

                new Path(SensorArea.E3, SensorArea.B2, new SensorArea[] { SensorArea.C }),
                new Path(SensorArea.E3, SensorArea.B3, new SensorArea[] { SensorArea.C }),


                new Path(SensorArea.D3, SensorArea.E3, new SensorArea[] { SensorArea.B2, SensorArea.C }),
                new Path(SensorArea.D3, SensorArea.E3, new SensorArea[] { SensorArea.B3, SensorArea.C }),
            };
            readonly Path[] _rightPathList = new Path[]
            {
                // Right
                new Path(SensorArea.C, SensorArea.B2, new SensorArea[] { SensorArea.E3 }),
                new Path(SensorArea.C, SensorArea.B3, new SensorArea[] { SensorArea.E3 }),

                new Path(SensorArea.B7, SensorArea.C, new SensorArea[] { SensorArea.B2 }),
                new Path(SensorArea.B7, SensorArea.C, new SensorArea[] { SensorArea.B3 }),

                new Path(SensorArea.B6, SensorArea.C, new SensorArea[] { SensorArea.B3 }),
                new Path(SensorArea.B6, SensorArea.C, new SensorArea[] { SensorArea.B2 }),

                new Path(SensorArea.D7, SensorArea.E7, new SensorArea[] { SensorArea.B7, SensorArea.C }),
                new Path(SensorArea.D7, SensorArea.E7, new SensorArea[] { SensorArea.B6, SensorArea.C }),
            };

            public void OnUpdate()
            {
                IsLeftMatch = false;
                IsRightMatch = false;

                for (var i = 0; i < _leftPathList.Length; i++)
                {
                    ref var path = ref _leftPathList[i];
                    if(IsLeftMatch || IsRightMatch)
                    {
                        path.Reset();
                    }
                    else
                    {
                        path.OnUpdate();
                        IsLeftMatch |= path.IsMatch;
                    }
                }
                for (var i = 0; i < _rightPathList.Length; i++)
                {
                    ref var path = ref _rightPathList[i];
                    if(IsLeftMatch || IsRightMatch)
                    {
                        path.Reset();
                    }
                    else
                    {
                        path.OnUpdate();
                        IsRightMatch |= path.IsMatch;
                    }                        
                }
            }
            public void GenerateIgnoreAreaList(Span<bool> areaList)
            {
                for (var i = 0; i < 33; i++)
                {
                    var area = (SensorArea)i;
                    for (var x = 0; x < _leftPathList.Length; x++)
                    {
                        var path = _leftPathList[x];
                        areaList[i] |= path.IsShouldBeIgnored(area);
                    }
                    for (var x = 0; x < _rightPathList.Length; x++)
                    {
                        var path = _rightPathList[x];
                        areaList[i] |= path.IsShouldBeIgnored(area);
                    }
                }
            }

            struct Path
            {
                public bool IsMatch { get; private set; }

                byte _state;
                TimeSpan _detectStartAt;

                readonly SensorArea _headArea;
                readonly SensorArea _area2;
                readonly SensorArea[] _tailAreaList;
                readonly TimeSpan _timeout;
                public Path(SensorArea headArea, SensorArea area2, SensorArea[] tailAreaList) 
                    : this(headArea, area2, tailAreaList, TimeSpan.FromMilliseconds(300))
                {

                }
                public Path(SensorArea headArea, SensorArea area2, SensorArea[] tailAreaList, TimeSpan timeout)
                {
                    _headArea = headArea;
                    _area2 = area2;
                    _tailAreaList = tailAreaList;
                    _timeout = timeout;
                }

                public void OnUpdate()
                {
                    if (IsMatch)
                    {
                        Reset();
                    }
                    var currentTimestamp = MajTimeline.UnscaledTime;
                    if (_state == 0)
                    {
                        if (InputManager.IsSensorClickedInThisFrame(_headArea))
                        {
                            _state = 1;
                            _detectStartAt = currentTimestamp;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if(currentTimestamp - _detectStartAt > _timeout)
                    {
                        Reset();
                        return;
                    }
                    if((_state & (1 << 1)) == 0)
                    {
                        if (InputManager.IsSensorClickedInThisFrame(_area2))
                        {
                            _state |= 1 << 1;
                        }
                        else
                        {
                            return;
                        }
                    }
                    var allFinished = 0b0000_0011;
                    for (var i = 0; i < _tailAreaList.Length; i++)
                    {
                        var area = _tailAreaList[i];
                        var mask = (byte)(1 << (i + 2));
                        allFinished |= mask;
                        if ((_state & mask) == 0)
                        {
                            if (InputManager.IsSensorClickedInThisFrame(area))
                            {
                                _state |= mask;
                            }
                            else
                            {
                                break;
                            }
                        }                            
                    }
                    if(_state == allFinished)
                    {
                        IsMatch = true;
                    }
                }
                public void Reset()
                {
                    IsMatch = false;
                    _state = 0;
                    _detectStartAt = TimeSpan.Zero;
                }
                public bool IsShouldBeIgnored(SensorArea area)
                {
                    if((_state & (1 << 1)) == 0)
                    {
                        return false;
                    }
                    if(area == _area2 || area == _headArea)
                    {
                        return true;
                    }
                    else
                    {
                        for (var i = 0; i < _tailAreaList.Length; i++)
                        {
                            if(area == _tailAreaList[i] && ((_state & (1 << (i + 2))) != 0))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
        }
    }
}
