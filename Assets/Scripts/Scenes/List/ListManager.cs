using Cysharp.Threading.Tasks;
using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MajdataPlay.List
{
    public class ListManager : MonoBehaviour
    {
        public CancellationToken CancellationToken => _cts.Token;
        public static List<UniTask> AllBackguardTasks { get; } = new(8192);

        int _delta = 0;
        float _pressTime = 0;
        bool _isPressed = false;

        CoverListDisplayer _coverListDisplayer;
        readonly CancellationTokenSource _cts = new();

        void Awake()
        {
            Majdata<ListManager>.Instance = this;
            AllBackguardTasks.Clear();
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
                InputManager.BindAnyArea(OnAreaDown);
                MajInstances.LightManager.SetButtonLight(Color.green, 3);
                MajInstances.LightManager.SetButtonLight(Color.red, 4);
                MajInstances.LightManager.SetButtonLight(Color.blue, 2);
                MajInstances.LightManager.SetButtonLight(Color.blue, 5);
                MajInstances.LightManager.SetButtonLight(Color.yellow, 6);
                MajInstances.LightManager.SetButtonLight(Color.yellow, 1);
            }
        }
        void OnDestroy()
        {
            Majdata<ListManager>.Free();
            MajEnv.SharedHttpClient.CancelPendingRequests();
            _cts.Cancel();
        }
        private async void OnAreaDown(object sender, InputEventArgs e)
        {
            
            if (!e.IsButton&&e.IsDown)
            {
                switch (e.Type)
                {
                    case SensorArea.A7:
                    case SensorArea.E8:
                        _coverListDisplayer.SlideList(1);
                        break;
                    case SensorArea.B8:
                    case SensorArea.A8:
                        _coverListDisplayer.SlideList(2);
                        break;
                    case SensorArea.E1:
                    case SensorArea.D1:
                    case SensorArea.A1:
                        _coverListDisplayer.SlideList(3);
                        break;
                    case SensorArea.A6:
                    case SensorArea.E6:
                        _coverListDisplayer.SlideList(-1);
                        break;
                    case SensorArea.A5:
                    case SensorArea.B5:
                        _coverListDisplayer.SlideList(-2);
                        break;
                    case SensorArea.E5:
                    case SensorArea.D5:
                    case SensorArea.A4:
                        _coverListDisplayer.SlideList(-3);
                        break;
                    case SensorArea.C:
                        _coverListDisplayer.RandomSelect();
                        break;
                    // xxlb
                    case SensorArea.B7:
                    case SensorArea.B6:
                    case SensorArea.E7:
                        var list = new string[] { "notouch.wav", "notouch_2.wav", "notouch_3.wav", "notouch_4.wav", "notouch_5.wav" };
                        MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                        XxlbAnimation.instance.PlayTouchAnimation();
                        break;
                    case SensorArea.B2:
                        _coverListDisplayer.FavoriteAdder.FavoratePressed();
                        break;
                }
            }
            else if (e.IsButton)
            {
                if (e.IsUp)
                {
                    switch (e.Type)
                    {
                        case SensorArea.A6:
                        case SensorArea.A3:
                            _isPressed = false;
                            _delta = 0;
                            _pressTime = 0;
                            break;
                        case SensorArea.A4:
                            if (_coverListDisplayer.IsDirList)
                            {
                                if (_coverListDisplayer.SelectedCollection.Type == ChartStorageType.Dan)
                                {
                                    var danInfo = _coverListDisplayer.SelectedCollection.DanInfo;
                                    var collection = _coverListDisplayer.SelectedCollection;
                                    if (danInfo is null)
                                        return;
                                    else if (danInfo.SongLevels.Length != collection.Count)
                                        return;
                                    InputManager.UnbindAnyArea(OnAreaDown);
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
                                    //MajInstances.GameManager.isDanMode = true;
                                    //MajInstances.GameManager.DanHP = SongStorage.WorkingCollection.DanInfo.StartHP;
                                    //MajInstances.GameManager.DanResults.Clear();
                                    _coverListDisplayer.SelectedCollection.Index = 0;
                                    MajInstances.SceneSwitcher.SwitchScene("Game", false);
                                }
                                else
                                {
                                    await _coverListDisplayer.SwitchToSongListAsync();
                                    MajInstances.LightManager.SetButtonLight(Color.red, 4);
                                }
                            }
                            else
                            {
                                InputManager.UnbindAnyArea(OnAreaDown);
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
                                if (_pressTime > 1f)
                                {
                                    var oldinfo = Majdata<GameInfo>.Instance;
                                    var info = new GameInfo(GameMode.Practice, charts, levels, 114514);
                                    if (oldinfo is not null && oldinfo.TimeRange is not null)
                                    {
                                        info.TimeRange = oldinfo.TimeRange;
                                    }
                                    
                                    Majdata<GameInfo>.Instance = info;
                                    _pressTime = 0;
                                    _isPressed = false;
                                    MajInstances.SceneSwitcher.SwitchScene("Practice", false);
                                }
                                else
                                {
                                    var info = new GameInfo(GameMode.Normal, charts, levels);
                                    Majdata<GameInfo>.Instance = info;
                                    _pressTime = 0;
                                    _isPressed = false;
                                    MajInstances.SceneSwitcher.SwitchScene("Game", false);
                                }
                            }
                            break;
                    }
                    return;
                }
                else if(e.IsDown)
                {

                    switch (e.Type)
                    {
                        case SensorArea.A3:
                            if (_isPressed)
                                return;
                            _coverListDisplayer.SlideList(1);
                            _isPressed = true;
                            _delta = 1;
                            AutoSlide().Forget();
                            break;
                        case SensorArea.A6:
                            if (_isPressed)
                                return;
                            _coverListDisplayer.SlideList(-1);
                            _isPressed = true;
                            _delta = -1;
                            AutoSlide().Forget();
                            break;
                        case SensorArea.A8:
                            _coverListDisplayer.SlideDifficulty(-1);
                            break;
                        case SensorArea.A1:
                            _coverListDisplayer.SlideDifficulty(1);
                            break;
                        case SensorArea.P1:
                            InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.SceneSwitcher.SwitchScene("SortFind");
                            break;
                        case SensorArea.A5:
                            if (_coverListDisplayer.IsChartList)
                            {
                                await _coverListDisplayer.SwitchToDirListAsync();
                                MajInstances.LightManager.SetButtonLight(Color.white, 4);
                                SongStorage.WorkingCollection.Index = 0;
                            }
                            break;
                        case SensorArea.A4:
                            if (!_coverListDisplayer.IsDirList)
                            {
                                _isPressed = true;
                                PracticeTimer().Forget();
                            }
                            break;
                        case SensorArea.A7:
                            InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.GameManager.LastSettingPage = 0;
                            MajInstances.SceneSwitcher.SwitchScene("Setting");
                            break;
                        case SensorArea.A2:
                            InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.GameManager.LastSettingPage = 4;
                            MajInstances.SceneSwitcher.SwitchScene("Setting");
                            break;
                    }
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
            while (_isPressed)
            {
                _pressTime += Time.deltaTime;
                await UniTask.Yield();
                if(_pressTime > 1f)
                {
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    break;
                }
            }
        }
        public static async UniTask WaitForBackgroundTasksSuspendAsync()
        {
            if (AllBackguardTasks.Count == 0)
            {
                return;
            }
            await UniTask.WhenAll(AllBackguardTasks);
            AllBackguardTasks.Clear();
        }
    }
}