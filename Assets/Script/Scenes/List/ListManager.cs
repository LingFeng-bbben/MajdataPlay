using Cysharp.Threading.Tasks;
using MajdataPlay.Game.Types;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MajdataPlay.List
{
    public class ListManager : MonoBehaviour
    {
        public CoverListDisplayer CoverListDisplayer;

        int _delta = 0;
        float _pressTime = 0;
        bool _isPressed = false;
        
        // Start is called before the first frame update
        void Start()
        {
            CoverListDisplayer.SwitchToDirList(SongStorage.Collections);
            CoverListDisplayer.SwitchToSongList();
            //MajInstances.LightManager.SetAllLight(Color.white);
            MajInstances.LightManager.SetButtonLight(Color.green, 3);
            MajInstances.LightManager.SetButtonLight(Color.red, 4);
            MajInstances.LightManager.SetButtonLight(Color.blue, 2);
            MajInstances.LightManager.SetButtonLight(Color.blue, 5);
            MajInstances.LightManager.SetButtonLight(Color.yellow, 6);
            MajInstances.LightManager.SetButtonLight(Color.yellow, 1);
            CoverListDisplayer.SlideToDifficulty((int)MajInstances.GameManager.SelectedDiff);
            var selectsfx = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            if (!selectsfx.IsPlaying)
            {
                MajInstances.AudioManager.PlaySFX("bgm_select.mp3", true);
                MajInstances.AudioManager.PlaySFX("SelectSong.wav");
            }
            MajInstances.InputManager.BindAnyArea(OnAreaDown);
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            
            if (!e.IsButton&&e.IsDown)
            {
                switch (e.Type)
                {
                    case SensorType.A7:
                        CoverListDisplayer.SlideList(1);
                        break;
                    case SensorType.A8:
                        CoverListDisplayer.SlideList(2);
                        break;
                    case SensorType.A1:
                        CoverListDisplayer.SlideList(3);
                        break;
                    case SensorType.A6:
                        CoverListDisplayer.SlideList(-1);
                        break;
                    case SensorType.A5:
                        CoverListDisplayer.SlideList(-2);
                        break;
                    case SensorType.A4:
                        CoverListDisplayer.SlideList(-3);
                        break;
                    // xxlb
                    case SensorType.B7:
                    case SensorType.B6:
                    case SensorType.E7:
                        var list = new string[] { "notouch.wav", "notouch_2.wav", "notouch_3.wav", "notouch_4.wav", "notouch_5.wav" };
                        MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                        XxlbAnimation.instance.PlayTouchAnimation();
                        break;
                }
            }
            else if (e.IsButton)
            {
                if (e.IsUp)
                {
                    switch (e.Type)
                    {
                        case SensorType.A6:
                        case SensorType.A3:
                            _isPressed = false;
                            _delta = 0;
                            _pressTime = 0;
                            break;
                        case SensorType.A4:
                            if (CoverListDisplayer.IsDirList)
                            {
                                if (SongStorage.WorkingCollection.Type == ChartStorageType.Dan)
                                {
                                    var danInfo = SongStorage.WorkingCollection.DanInfo;
                                    var collection = SongStorage.WorkingCollection;
                                    if (danInfo is null)
                                        return;
                                    else if (danInfo.SongLevels.Length != collection.Count)
                                        return;
                                    MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
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
                                        MaxHP = SongStorage.WorkingCollection.DanInfo.StartHP,
                                        CurrentHP = SongStorage.WorkingCollection.DanInfo.StartHP,
                                        HPRecover = SongStorage.WorkingCollection.DanInfo.RestoreHP,
                                        DanInfo = danInfo
                                    };
                                    MajInstanceHelper<GameInfo>.Instance = info;
                                    //MajInstances.GameManager.isDanMode = true;
                                    //MajInstances.GameManager.DanHP = SongStorage.WorkingCollection.DanInfo.StartHP;
                                    //MajInstances.GameManager.DanResults.Clear();
                                    SongStorage.WorkingCollection.Index = 0;
                                    MajInstances.SceneSwitcher.SwitchScene("Game", false);
                                }
                                else
                                {
                                    CoverListDisplayer.SwitchToSongList();
                                    MajInstances.LightManager.SetButtonLight(Color.red, 4);
                                }
                            }
                            else
                            {
                                MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                                MajInstances.AudioManager.StopSFX("bgm_select.mp3");
                                var list = new string[] { "track_start.wav", "track_start_2.wav" };
                                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                                var levels = new ChartLevel[]
                                {
                                MajInstances.GameManager.SelectedDiff
                                };
                                var charts = new SongDetail[]
                                {
                                SongStorage.WorkingCollection.Current
                                };
                                if (_pressTime > 1f)
                                {
                                    var oldinfo = MajInstanceHelper<GameInfo>.Instance;
                                    var info = new GameInfo(GameMode.Practice, charts, levels, 114514);
                                    if (oldinfo is not null && oldinfo.TimeRange is not null)
                                    {
                                        info.TimeRange = oldinfo.TimeRange;
                                    }
                                    
                                    MajInstanceHelper<GameInfo>.Instance = info;
                                    _pressTime = 0;
                                    _isPressed = false;
                                    MajInstances.SceneSwitcher.SwitchScene("Practice", false);
                                }
                                else
                                {
                                    var info = new GameInfo(GameMode.Normal, charts, levels);
                                    MajInstanceHelper<GameInfo>.Instance = info;
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
                        case SensorType.A3:
                            if (_isPressed)
                                return;
                            CoverListDisplayer.SlideList(1);
                            _isPressed = true;
                            _delta = 1;
                            AutoSlide().Forget();
                            break;
                        case SensorType.A6:
                            if (_isPressed)
                                return;
                            CoverListDisplayer.SlideList(-1);
                            _isPressed = true;
                            _delta = -1;
                            AutoSlide().Forget();
                            break;
                        case SensorType.A8:
                            CoverListDisplayer.SlideDifficulty(-1);
                            break;
                        case SensorType.A1:
                            CoverListDisplayer.SlideDifficulty(1);
                            break;
                        case SensorType.P1:
                            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.SceneSwitcher.SwitchScene("SortFind");
                            break;
                        case SensorType.A5:
                            if (CoverListDisplayer.IsChartList)
                            {
                                CoverListDisplayer.SwitchToDirList(SongStorage.Collections);
                                MajInstances.LightManager.SetButtonLight(Color.white, 4);
                                SongStorage.WorkingCollection.Index = 0;
                            }
                            break;
                        case SensorType.A4:
                            if (!CoverListDisplayer.IsDirList)
                            {
                                _isPressed = true;
                                PracticeTimer().Forget();
                            }
                            break;
                        case SensorType.A7:
                            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.GameManager.LastSettingPage = 0;
                            MajInstances.SceneSwitcher.SwitchScene("Setting");
                            break;
                        case SensorType.A2:
                            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
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
                CoverListDisplayer.SlideList(_delta);
                await UniTask.Delay(100);
            }
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
    }
}